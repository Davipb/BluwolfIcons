using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Windows.Media;

namespace BluwolfIcons.Tests
{
	[TestClass]
	public class IconTests
	{
		[TestMethod]
		public void Save_GeneratesValidIcon()
		{
			var dummy1 = Helper.GenerateImage(16, 16, PixelFormats.Bgra32);
			var dummy2 = Helper.GenerateImage(256, 256, PixelFormats.Bgra32);

			var icon = new Icon();
			icon.Images.Add(new PngIconImage(dummy1));
			icon.Images.Add(new BmpIconImage(dummy1));

			icon.Images.Add(new PngIconImage(dummy2));
			icon.Images.Add(new BmpIconImage(dummy2));

			Icon newIcon = null;
			byte[] data = null;
			using (var stream = new MemoryStream())
			{
				icon.Save(stream);
				data = stream.GetBuffer();
			}

			using (var stream = new MemoryStream(data, true))
			{
				newIcon = Icon.Load(stream);
			}

			Assert.IsNotNull(newIcon);
			Assert.IsTrue(newIcon.Images.Count >= 2);
		}
	}
}
