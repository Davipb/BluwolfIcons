using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BluwolfIcons.Tests
{
	[TestClass]
	public class PngIconImageTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_ImageTooBig_ThrowsException()
		{
			var bitmap = Helper.GenerateImage(257, 257, PixelFormats.Bgra32);
			var target = new PngIconImage(bitmap);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void SetOriginalImage_ImageTooBig_ThrowsException()
		{
			var valid = Helper.GenerateImage(16, 16, PixelFormats.Bgra32);
			var invalid = Helper.GenerateImage(257, 257, PixelFormats.Bgra32);

			var target = new PngIconImage(valid);
			target.OriginalImage = invalid;
		}

		[TestMethod]
		public void GetData_PreservesOriginalProperties()
		{
			var original = Helper.GenerateImage(16, 16, PixelFormats.Bgra32);

			var target = new PngIconImage(original);
			var data = target.GetData();

			using (var stream = new MemoryStream(data, false))
			{
				var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
				var result = decoder.Frames[0];

				Assert.AreEqual(result.PixelWidth, original.PixelWidth);
				Assert.AreEqual(result.PixelHeight, original.PixelHeight);
				Assert.AreEqual(result.Format, original.Format);
			}
		}
	}
}
