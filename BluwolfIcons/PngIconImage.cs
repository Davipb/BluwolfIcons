﻿using System.IO;
using System.Windows.Media.Imaging;

namespace BluwolfIcons
{
	/// <summary>
	/// A PNG (Portable Network Graphics) image inside an icon.
	/// </summary>
	public sealed class PngIconImage : IIconImage
	{
		/// <summary>
		/// The original image.
		/// </summary>
		public BitmapSource OriginalImage { get; set; }

		/// <summary>
		/// This image's width.
		/// </summary>
		public int Width => OriginalImage.PixelWidth;

		/// <summary>
		/// This image's height.
		/// </summary>
		public int Height => OriginalImage.PixelHeight;

		/// <summary>
		/// This image's bits per pixel.
		/// </summary>
		public int BitsPerPixel => OriginalImage.Format.BitsPerPixel;

		/// <summary>
		/// Creates a new PNG icon image, with <paramref name="image"/> as its original image.
		/// </summary>
		/// <param name="image">The original image to use in this icon image.</param>
		public PngIconImage(BitmapSource image)
		{
			OriginalImage = image;
		}

		/// <summary>
		/// Gets the PNG icon image data for this image.
		/// </summary>
		/// <returns>The PNG icon image data for this image.</returns>
		public byte[] GetData()
		{
			// PNG images are represented just like in a normal file, so all we do is save to a MemoryStream and return the generated bytes.
			using (var stream = new MemoryStream())
			{
				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(OriginalImage));
				encoder.Save(stream);

				return stream.GetBuffer();
			}
		}

	}
}
