using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BluwolfIcons
{
	/// <summary>
	/// Represents an icon image file.
	/// </summary>
	public sealed class Icon
	{
		/// <summary>
		/// All the images contained in this icon.
		/// </summary>
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

			if (!stream.CanWrite)
				throw new ArgumentException("Stream must support writing.", nameof(stream));

			using (var writer = new BinaryWriter(stream))
			{
				// Reserved, always 0.
				writer.Write((ushort)0);
				// 1 for ICO, 2 for CUR
				writer.Write((ushort)1);
				writer.Write((ushort)Images.Count);

				var pendingImages = new Queue<byte[]>();
				var offset = 6 + 16 * Images.Count; // Header: 6; Each Image: 16

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

					var data = image.GetData();

					writer.Write((uint)data.Length);
					writer.Write((uint)offset);

					offset += data.Length;
					pendingImages.Enqueue(data);
				}

				while (pendingImages.Count > 0)
					writer.Write(pendingImages.Dequeue());

			}
		}

		/// <summary>
		/// Loads an icon from a specified file.
		/// </summary>
		/// <param name="fileName">The file to load the icon from.</param>
		/// <returns>The loaded icon.</returns>
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
		public static Icon Load(Stream stream)
			=> Load(new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad));

		/// <summary>
		/// Loads an icon from a bitmap decoder.
		/// </summary>
		/// <param name="decoder">The decoder to use when loading the icon. Every unique Frame decoded will be recognized as one individual image.</param>
		/// <returns>The loaded icon</returns>
		public static Icon Load(BitmapDecoder decoder)
		{
			if (decoder == null)
				throw new ArgumentNullException(nameof(decoder));

			var result = new Icon();

			var added = new List<byte[]>();

			foreach (var frame in decoder.Frames)
			{
				var encoder = new BmpBitmapEncoder();
				encoder.Frames.Add(frame);
				byte[] data = null;
				using (var stream = new MemoryStream())
				{
					encoder.Save(stream);
					data = stream.GetBuffer();
				}

				bool duplicate = false;
				foreach (var alreadyAdded in added)
				{
					if (data.SequenceEqual(alreadyAdded))
					{
						duplicate = true;
						break;
					}
				}

				if (duplicate)
					continue;

				added.Add(data);
				result.Images.Add(new PngIconImage(frame));
				result.Images.Add(new BmpIconImage(frame));
			}

			return result;
		}
	}
}
