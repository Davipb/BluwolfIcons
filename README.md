# ![Bluwolf Logo](http://i.imgur.com/fzOKZmJ.png)

Bluwolf Icons is a C# .NET library for manipulating icon images (.ico) made using the WIC (Windows Imaging Component), 
the same technology behind WPF.

**We're on NuGet!**

`Install-Package BluwolfIcons`

**Why not just use .NET's built-in Icon support?**

All the built-in .NET libraries for icon manipulation only support a single icon: 
It automatically loads only the best quality image inside the .ico and you can only save one single bitmap as an icon.
Not only that, but they are also built on top of GDI+, which is known to be messy and incomplete.
Bluwolf Icons lets you load and save icons with as many images as you want and is built on top of the newer WIC, the same used in WPF.

## Quick Start

Creating and saving an icon from code:
```csharp
var icon = new Icon();

// This is just an example, any BitmapSource will be accepted
var bitmap = new BitmapImage();
bitmap.BeginInit();
bitmap.UriSource = new Uri("MyAwesomeImage.png");
bitmap.CacheOption = BitmapCacheOption.OnLoad;
bitmap.EndInit();

// You can manually choose how each image will be saved as: PNG or BMP
icon.Images.Add(new PngIconImage(bitmap));
icon.Images.Add(new BmpIconImage(bitmap));

// Save to a file
icon.Save("icon.ico");

// Save to a stream
using (var stream = new MemoryStream())
{
	icon.Save(stream);
}
```

Loading an icon:
```csharp
// Load from a file
var fileIcon = Icon.Load("icon.ico");

// Load from a stream
Icon streamIcon = null;
using (var stream = new MemoryStream(GetIconData()))
{
	streamIcon = Icon.Load(stream);
}

// Load from a BitmapDecoder
var decoder = new PngBitmapDecoder(
	new Uri("MyAwesomeImage.png"), 
	BitmapCreateOptions.PreservePixelFormat, 
	BitmapCacheOption.OnLoad);
var decoderIcon = Icon.Load(decoder);

// Read the icon in code
foreach (var image in fileIcon.Images)
{
	Console.WriteLine($"{image.Width}x{image.Height}x{image.BitsPerPixel}");
}
```

## FAQ

**What's the difference between a PNG and a BMP icon image?**

A .ico icon is just a container that holds various images. 
Originally, it could hold only BMP images, but Windows Vista added support for PNGs.
As such, BMPs are the most compatible format, but have less quality and occupy more space.
PNGs are more compressible and have more quality, but not all programs may support it.


