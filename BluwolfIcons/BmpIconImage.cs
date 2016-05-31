using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace BluwolfIcons
{
	/// <summary>
	/// A BMP (Bitmap) image inside an icon.
	/// </summary>
	public class BmpIconImage : IIconImage
	{
		/// <summary>
		/// The original image.
		/// </summary>
		public Bitmap OriginalImage { get; set; }

		/// <summary>
		/// This image's width.
		/// </summary>
		public int Width => OriginalImage.Width;

		/// <summary>
		/// This image's height.
		/// </summary>
		public int Height
		{
			get
			{
				if (GenerateTransparencyMap)
					return OriginalImage.Height;

				// If the image already contains the transparency map, we should report its size as half the real size
				// (The transparency map takes 50% of the image)
				return OriginalImage.Height / 2;
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
					return 24;

				// If not generating the transparency map, we'll just copy the original image
				return Image.GetPixelFormatSize(OriginalImage.PixelFormat);
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
		public BmpIconImage(Bitmap image)
		{
			OriginalImage = image;
		}

		/// <summary>
		/// Generates the BMP icon image data for this image.
		/// </summary>
		/// <returns>The BMP icon image data for this image.</returns>
		public byte[] GetData()
		{
			Bitmap result = null;
			if (GenerateTransparencyMap)
			{
				// To avoid dealing with all possible formats, we'll standardize it
				var normalized = OriginalImage.Clone(new Rectangle(Point.Empty, OriginalImage.Size), PixelFormat.Format24bppRgb);
				result = new Bitmap(normalized.Width, normalized.Height * 2, normalized.PixelFormat);

				var normalizedData = normalized.LockBits(new Rectangle(Point.Empty, normalized.Size), ImageLockMode.ReadOnly, normalized.PixelFormat);
				var resultData = result.LockBits(new Rectangle(Point.Empty, result.Size), ImageLockMode.WriteOnly, result.PixelFormat);

				byte[] buffer = new byte[normalizedData.Stride * normalizedData.Height];

				// The transparency map must appear before the actual image, so we copy it now.
				// Buffer will be set to all 0s, so the whole image will be visible.
				// TODO: Implement support for already-transparent images
				Marshal.Copy(buffer, 0, resultData.Scan0, buffer.Length);

				// Now we copy the actual image.
				Marshal.Copy(normalizedData.Scan0, buffer, 0, buffer.Length);
				Marshal.Copy(buffer, 0, resultData.Scan0 + buffer.Length, buffer.Length);

				normalized.UnlockBits(normalizedData);
				normalized.Dispose();

				result.UnlockBits(resultData);
			}
			else
			{
				// We copy the actual image because it'll be disposed later
				result = OriginalImage.Clone(new Rectangle(Point.Empty, OriginalImage.Size), OriginalImage.PixelFormat);
			}

			using (var stream = new MemoryStream())
			{
				result.Save(stream, ImageFormat.MemoryBmp);
				result.Dispose();
				return stream.GetBuffer();
			}
		}
	}
}
