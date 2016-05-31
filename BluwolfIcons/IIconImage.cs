using System.Drawing;

namespace BluwolfIcons
{
	/// <summary>
	/// An image contained inside an icon.
	/// </summary>
	public interface IIconImage
	{
		/// <summary>
		/// The original image.
		/// </summary>
		Bitmap Image { get; set; }

		/// <summary>
		/// Generates the icon image data for this image.
		/// </summary>
		/// <returns>The data representing this image in the .ico</returns>
		byte[] GetData();
	}
}
