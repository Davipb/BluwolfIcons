using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BluwolfIcons
{
	/// <summary>
	/// A BMP (Bitmap) image inside an icon.
	/// </summary>
	public sealed class BmpIconImage : IIconImage
	{
		const int BmpFileHeaderSize = 14;

		private BitmapSource originalImage;

		/// <summary>
		/// The original image.
		/// </summary>
		/// <exception cref="T:System.ArgumentException">Thrown when an image too big is assigned.</exception>
		public BitmapSource OriginalImage
		{
			get { return originalImage; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (value.PixelWidth > 256)
					throw new ArgumentException("Image can't be wider than 256 pixels", nameof(value));

				if (GenerateTransparencyMap)
				{
					if (value.PixelHeight > 256)
						throw new ArgumentException("Image without transparency map can't be taller than 256 pixels.", nameof(value));
				}
				else
				{
					if (value.PixelHeight > 512)
						throw new ArgumentException("Image with transparency map can't be taller than 512 pixels.", nameof(value));
				}

				originalImage = value;
			}
		}

		/// <summary>
		/// This image's width.
		/// </summary>
		public int Width => OriginalImage.PixelWidth;

		/// <summary>
		/// This image's height.
		/// </summary>
		public int Height
		{
			get
			{
				if (GenerateTransparencyMap)
					return OriginalImage.PixelHeight;

				// If the image already contains the transparency map, we should report its size as half the real size
				// (The transparency map takes 50% of the image)
				return OriginalImage.PixelHeight / 2;
			}
		}

		/// <summary>
		/// This image's bits per pixel.
		/// </summary>
		public int BitsPerPixel => OriginalImage.Format.BitsPerPixel;

		private bool generateTransparencyMap = true;
		/// <summary>
		/// Whether a transparency map should be generated automatically for this image.
		/// Set to <c>false</c> if the image already contains a transparency map. See remarks for more info.
		/// </summary>
		/// <remarks>
		/// BMP images inside icons are stored with an extra transparency map above the actual image data.
		/// That transparency map is a 1-bit per pixel AND mask that defines if a pixel is visible or not,
		/// allowing for 1-bit transparency.
		/// </remarks>
		/// <exception cref="T:System.ArgumentException">Thrown when <see cref="OriginalImage"/> is taller than 256 pixels and value is being set to <c>false</c>.</exception>
		public bool GenerateTransparencyMap
		{
			get { return generateTransparencyMap; }
			set
			{
				if (value == false && OriginalImage?.PixelHeight > 256)
					throw new ArgumentException("Can't generate transparency map for an image taller than 256 pixels.", nameof(value));

				generateTransparencyMap = value;
			}
		}

		/// <summary>
		/// Creates a new BMP icon image, with <paramref name="image"/> as its original image, without a transparency map.
		/// </summary>
		/// <param name="image">The original image to use in this icon image.</param>
		public BmpIconImage(BitmapSource image) : this(image, true) { }

		/// <summary>
		/// Creates a new BMP icon image, with <paramref name="image"/> as its original image.
		/// </summary>
		/// <param name="image">The original image to use in this icon image.</param>
		/// <param name="generateMap">Whether to generate the transparency map or not.</param>
		/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="image"/> is <c>null</c>.</exception>
		public BmpIconImage(BitmapSource image, bool generateMap)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));

			GenerateTransparencyMap = generateMap;
			OriginalImage = image;
		}

		/// <summary>
		/// Generates the BMP icon image data for this image.
		/// </summary>
		/// <returns>The BMP icon image data for this image.</returns>
		public byte[] GetData()
		{
			BitmapSource result = null;

			if (GenerateTransparencyMap)
			{
				// The transparency map has to be above the actual image, so we just copy the original image's data at the bottom
				// All bytes before that will be set to 0, which is what we want (0 == Visible)
				byte[] buffer = new byte[OriginalImage.PixelWidth * OriginalImage.PixelHeight * 4 * 2];
				OriginalImage.CopyPixels(buffer, OriginalImage.PixelWidth * 4, buffer.Length / 2);

				result = BitmapSource.Create(
					OriginalImage.PixelWidth, OriginalImage.PixelHeight * 2,
					OriginalImage.DpiX, OriginalImage.DpiY,
					OriginalImage.Format,
					OriginalImage.Palette,
					buffer,
					OriginalImage.PixelWidth * 4);
			}
			else
			{
				result = OriginalImage;
			}

			using (var stream = new MemoryStream())
			{
				var encoder = new BmpBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(result));
				encoder.Save(stream);

				// Remove the BMPFILEHEADER, turning it into a Memory BMP.
				return stream.GetBuffer().Skip(BmpFileHeaderSize).ToArray();
			}
		}
	}
}
