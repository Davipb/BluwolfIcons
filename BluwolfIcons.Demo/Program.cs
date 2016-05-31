using System;
using System.IO;

namespace BluwolfIcons.Demo
{
	class Program
	{
		static void Main()
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
