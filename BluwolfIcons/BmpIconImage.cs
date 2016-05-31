using System;
using System.Drawing;

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
		/// <para>BMP images inside icons are stored with an extra transparency map above the actual image data.
		/// That transparency map is a 1-bit per pixel AND mask that defines if a pixel is visible or not,
		/// allowing for 1-bit transparency.</para>
		/// <para>When generating the transparency map, <see cref="TransparencyTolerance"/> will be used for natively
		/// transparent image formats.</para>
		/// </remarks>
		public bool GenerateTransparencyMap { get; set; } = true;

		/// <summary>
		/// When <see cref="GenerateTransparencyMap"/> is set to <c>true</c>, this defines the cutoff point
		/// when generating the 1-bit transparency map. Alphas below this value will be considered transparent,
		/// and those above or equal to it, visible.
		/// </summary>
		public byte TransparencyTolerance { get; set; } = 128;

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
			throw new NotImplementedException();
		}
	}
}
