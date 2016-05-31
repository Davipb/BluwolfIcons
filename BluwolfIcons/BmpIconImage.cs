using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace BluwolfIcons
{
	/// <summary>
	/// A BMP (Bitmap) image inside an icon.
	/// </summary>
	public class BmpIconImage
	{
		/// <summary>
		/// The original image.
		/// </summary>
		public Bitmap Image { get; set; }

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
			Image = image;
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
				// To avoid dealing with all possible formats, we'll standardize to 32-bit per pixel ARGB.
				var normalized = Image.Clone(new Rectangle(Point.Empty, Image.Size), PixelFormat.Format32bppArgb);
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
				result = Image.Clone(new Rectangle(Point.Empty, Image.Size), Image.PixelFormat);
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
