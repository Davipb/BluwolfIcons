using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BluwolfIcons.Tests
{
	internal static class Helper
	{
		public static BitmapSource GenerateImage(int width, int height, PixelFormat format)
		{
			var stride = width * (format.BitsPerPixel / 8);
			byte[] buffer = new byte[stride * height];

			return BitmapSource.Create(width, height, 90.0, 90.0, format, null, buffer, stride);
		}

		public static byte[] ToBytes(this BitmapSource source)
		{
			var encoder = new BmpBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(source));

			using (var stream = new MemoryStream())
			{
				encoder.Save(stream);
				return stream.GetBuffer();
			}
		}
	}
}
