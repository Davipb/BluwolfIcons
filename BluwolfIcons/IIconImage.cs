namespace BluwolfIcons
{
	/// <summary>
	/// An image contained inside an icon.
	/// </summary>
	public interface IIconImage
	{
		/// <summary>
		/// This image's width.
		/// </summary>
		int Width { get; }

		/// <summary>
		/// This image's height.
		/// </summary>
		int Height { get; }

		/// <summary>
		/// This image's bits per pixel.
		/// </summary>
		int BitsPerPixel { get; }

		/// <summary>
		/// Generates the icon image data for this image.
		/// </summary>
		/// <returns>The data representing this image in the .ico</returns>
		byte[] GetData();
	}
}
