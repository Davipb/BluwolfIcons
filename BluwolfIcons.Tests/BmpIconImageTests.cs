using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;

namespace BluwolfIcons.Tests
{
	[TestClass]
	public class BmpIconImageTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_ImageTooBigWithoutMap_ThrowsException()
		{
			var bitmap = Helper.GenerateImage(257, 257, PixelFormats.Bgra32);
			var target = new BmpIconImage(bitmap, true);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void SetOriginalImage_ImageTooBigWithoutMap_ThrowsException()
		{
			var valid = Helper.GenerateImage(16, 16, PixelFormats.Bgra32);
			var invalid = Helper.GenerateImage(257, 257, PixelFormats.Bgra32);

			var target = new BmpIconImage(valid, true);
			target.OriginalImage = invalid;
		}

		[TestMethod]
		public void Constructor_BiggerImageWithMap_DoesntThrowsException()
		{
			var bitmap = Helper.GenerateImage(257, 257, PixelFormats.Bgra32);
			var target = new BmpIconImage(bitmap, false);

			Assert.AreEqual(target.OriginalImage, bitmap);
		}

		[TestMethod]
		public void SetOriginalImage_BiggerImageTooBigWithMap_DoesntThrowsException()
		{
			var small = Helper.GenerateImage(16, 16, PixelFormats.Bgra32);
			var big = Helper.GenerateImage(257, 257, PixelFormats.Bgra32);

			var target = new BmpIconImage(small, false);
			target.OriginalImage = big;

			Assert.AreEqual(target.OriginalImage, big);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_ImageTooBigWithMap_ThrowsException()
		{
			var bitmap = Helper.GenerateImage(513, 513, PixelFormats.Bgra32);
			var target = new BmpIconImage(bitmap, false);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void SetOriginalImage_ImageTooBigWithMap_ThrowsException()
		{
			var valid = Helper.GenerateImage(16, 16, PixelFormats.Bgra32);
			var invalid = Helper.GenerateImage(513, 513, PixelFormats.Bgra32);

			var target = new BmpIconImage(valid, false);
			target.OriginalImage = invalid;
		}

	}
}
