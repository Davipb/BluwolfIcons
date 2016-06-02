using System;
using System.IO;
using System.Linq;

namespace BluwolfIcons.Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0 || args[0] == "read")
			{
				var icon = Icon.Load("icon.ico");

				int saveIndex = 0;
				foreach (var image in icon.Images.OfType<PngIconImage>())
				{
					Console.WriteLine($"{image.Width}x{image.Height}x{image.BitsPerPixel}\t{saveIndex}.png");
					image.OriginalImage.Save($"{saveIndex}.png");
					saveIndex++;
				}
				foreach (var image in icon.Images.OfType<BmpIconImage>())
				{
					Console.WriteLine($"{image.Width}x{image.Height}x{image.BitsPerPixel}\t{saveIndex}.bmp");
					image.OriginalImage.Save($"{saveIndex}.bmp");
					saveIndex++;
				}

				icon.Save("newicon.ico");
				Console.ReadKey();
			}
			else
			{
				var images = Directory.EnumerateFiles("./", "*.png");

				var icon = new Icon();
				foreach (var image in images)
				{
					var bitmap = new System.Drawing.Bitmap(image);

					icon.Images.Add(new PngIconImage(bitmap));
					icon.Images.Add(new BmpIconImage(bitmap));
				}

				icon.Save("icon.ico");
				Console.WriteLine($"Wrote {icon.Images.Count} icon images to 'icon.ico'.");
				Console.ReadKey();
			}
		}
	}
}
