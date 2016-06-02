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
		public int BitsPerPixel
		{
			get
			{
				// We always standardize the bit depth when generating the transparency map
				if (GenerateTransparencyMap)
					return 32;

				// If not generating the transparency map, we'll just copy the original image
				return OriginalImage.Format.BitsPerPixel;
			}
		}

		/// <summary>
		/// Whether a transparency map should be generated automatically for this image.
		/// Set to <c>false</c> if the image already contains a transparency map. See remarks for more info.
		/// </summary>
		/// <remarks>
		/// BMP images inside icons are stored with an extra transparency map above the actual image data.
		/// That transparency map is a 1-bit per pixel AND mask that defines if a pixel is visible or not,
		/// allowing for 1-bit transparency.
		/// </remarks>
		public bool GenerateTransparencyMap { get; set; } = true;

		/// <summary>
		/// Creates a new BMP icon image, with <paramref name="image"/> as its original image.
		/// </summary>
		/// <param name="image">The original image to use in this icon image.</param>
		public BmpIconImage(BitmapSource image)
		{
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
				// Using ImageFormat.MemoryBmp in .Save DOES NOT work, we have to remove the header manually.
				return stream.GetBuffer().Skip(BmpFileHeaderSize).ToArray();
			}
		}
	}
}
