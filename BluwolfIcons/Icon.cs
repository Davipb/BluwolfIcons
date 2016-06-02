using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BluwolfIcons
{
	/// <summary>
	/// Represents an icon image file.
	/// </summary>
	public sealed class Icon : IDisposable
	{
		private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

		public IList<IIconImage> Images { get; } = new List<IIconImage>();

		/// <summary>
		/// Saves this icon to a specified file.
		/// </summary>
		/// <param name="fileName">The file to save this icon to.</param>
		public void Save(string fileName)
		{
			using (var stream = File.Open(fileName, FileMode.Create))
			{
				Save(stream);
			}
		}

		/// <summary>
		/// Saves this icon to a specified stream.
		/// </summary>
		/// <param name="stream">The stream to save this icon to.</param>
		public void Save(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (!stream.CanSeek)
				throw new ArgumentException("Stream must support seeking.", nameof(stream));

			using (var writer = new BinaryWriter(stream))
			{
				// Reserved, always 0.
				writer.Write((ushort)0);
				// 1 for ICO, 2 for CUR
				writer.Write((ushort)1);
				writer.Write((ushort)Images.Count);

				// We'll store images here with a stream position indicating where to write specific image data later
				var pendingImages = new Dictionary<long, IIconImage>();

				foreach (var image in Images)
				{
					writer.Write((byte)image.Width);
					writer.Write((byte)image.Height);

					// Number of colors in the palette. Since we always save the image ourselves (with no palette), hardcode this to 0 (No palette).
					writer.Write((byte)0);
					// Reserved, always 0.
					writer.Write((byte)0);
					// Color planes. Since we save the images ourselves, this is 1.
					writer.Write((ushort)1);
					writer.Write((ushort)image.BitsPerPixel);

					pendingImages.Add(writer.BaseStream.Position, image);

					// Image size in bytes. Since we can't know this yet, fill it with zeros.
					writer.Write(0u);
					// Image data offset from the start of the file. Since we can't know this yet, fill it with zeros.
					writer.Write(0u);
				}

				foreach (var image in pendingImages)
				{
					var data = image.Value.GetData();
					var offset = (uint)writer.BaseStream.Position;

					// We need to write specific image data now.
					writer.BaseStream.Seek(image.Key, SeekOrigin.Begin);
					writer.Write((uint)data.Length);
					writer.Write(offset);

					// Return to the proper stream location and write the image data.
					writer.BaseStream.Seek(offset, SeekOrigin.Begin);
					writer.Write(data);
				}
			}
		}

		/// <summary>
		/// Loads an icon from a specified file.
		/// </summary>
		/// <param name="fileName">The file to load the icon from.</param>
		/// <returns>The loaded icon.</returns>
		/// <exception cref="IconParseException">Thrown when the Icon file contains invalid data.</exception>
		public static Icon Load(string fileName)
		{
			using (var stream = File.OpenRead(fileName))
				return Load(stream);
		}

		/// <summary>
		/// Loads an icon from a stream.
		/// </summary>
		/// <param name="stream">The stream to load the icon from.</param>
		/// <returns>The loaded icon.</returns>
		/// <exception cref="IconParseException">Thrown when the Icon file contains invalid data.</exception>
		public static Icon Load(Stream stream)
		{
			if (!stream.CanSeek)
				throw new ArgumentException("Stream must support seeking.", nameof(stream));

			using (var reader = new BinaryReader(stream))
			{
				if (reader.ReadUInt16() != 0)
					throw new IconParseException("Invalid file header.");

				if (reader.ReadUInt16() != 1)
					throw new IconParseException("Invalid file header.");

				var imageCount = (int)reader.ReadUInt16();
				var result = new Icon();

				for (int i = 0; i < imageCount; i++)
				{
					// This skips directly to the info about where the data is stored
					reader.ReadBytes(8);

					var size = (int)reader.ReadUInt32();
					var offset = (long)reader.ReadUInt32();

					var currentOffset = reader.BaseStream.Position;

					reader.BaseStream.Seek(offset, SeekOrigin.Begin);
					var data = reader.ReadBytes(size);
					reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);

					if (data.Take(PngHeader.Length).SequenceEqual(PngHeader))
					{
						// File is PNG
						using (var memory = new MemoryStream(data, false))
						{
							result.Images.Add(
								new PngIconImage(new System.Drawing.Bitmap(memory))
								);
						}
					}
					else
					{
						// File is BMP
						// We need to reconstruct the file header so System.Drawing.Bitmap can read it properly
						byte[] header = new byte[14];

						using (var headerMemory = new MemoryStream(header))
						using (var headerWriter = new BinaryWriter(headerMemory))
						{
							// Fixed starting bytes to identify the file as BMP
							headerWriter.Write((byte)0x42);
							headerWriter.Write((byte)0x4D);
							// Size of the BMP
							headerWriter.Write((uint)(data.Length + header.Length));
							// Reserved, always 0
							headerWriter.Write(0u);
							// The offset at which the pixel array can be found
							// This is never really used, so calculating it is unnecessary, just fill it in with zeros
							headerWriter.Write(0u);
						}

						data = header.Concat(data).ToArray();

						using (var memory = new MemoryStream(data, false))
						{
							// We need to manually copy the bitmap created from our data to a fresh bitmap
							// This apparently fixes any internal errors that prevent you from saving it later
							// Why? Ask the GDI+ deities.

							var originalBitmap = new Bitmap(memory);


							var newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, originalBitmap.PixelFormat);

							var originalData = originalBitmap.LockBits(new Rectangle(Point.Empty, originalBitmap.Size), ImageLockMode.ReadOnly, originalBitmap.PixelFormat);
							var newData = newBitmap.LockBits(new Rectangle(Point.Empty, newBitmap.Size), ImageLockMode.WriteOnly, newBitmap.PixelFormat);

							byte[] buffer = new byte[originalData.Stride * originalData.Height];
							Marshal.Copy(originalData.Scan0, buffer, 0, buffer.Length);
							Marshal.Copy(buffer, 0, newData.Scan0, buffer.Length);

							originalBitmap.UnlockBits(originalData);
							originalBitmap.Dispose();
							newBitmap.UnlockBits(newData);

							// We need to set the GenerateTransparencyMap option to false, since the loaded image already has that map
							result.Images.Add(
								new BmpIconImage(newBitmap) { GenerateTransparencyMap = false }
								);
						}
					}
				}

				return result;
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var image in Images)
						image.Dispose();
				}

				disposedValue = true;
			}
		}

		/// <summary>
		/// Disposes of this <see cref="Icon"/> and all <see cref="IIconImage"/> associated with it.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion


		[Serializable]
		public class IconParseException : Exception
		{
			public IconParseException() { }
			public IconParseException(string message) : base(message) { }
			public IconParseException(string message, Exception inner) : base(message, inner) { }
			protected IconParseException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
	}
}
