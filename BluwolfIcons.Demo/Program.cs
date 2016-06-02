using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

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

					var encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(image.OriginalImage));
					using (var stream = File.Open($"{saveIndex}.png", FileMode.Create))
					{
						encoder.Save(stream);
					}

					saveIndex++;
				}

				foreach (var image in icon.Images.OfType<BmpIconImage>())
				{
					Console.WriteLine($"{image.Width}x{image.Height}x{image.BitsPerPixel}\t{saveIndex}.bmp");

					var encoder = new BmpBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(image.OriginalImage));
					using (var stream = File.Open($"{saveIndex}.bmp", FileMode.Create))
					{
						encoder.Save(stream);
					}

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
					var bitmap = new BitmapImage();
					bitmap.BeginInit();
					bitmap.UriSource = new Uri(Path.GetFullPath(image));
					bitmap.CacheOption = BitmapCacheOption.OnLoad;
					bitmap.EndInit();

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
