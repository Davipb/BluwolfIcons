using System;
using System.Collections.Generic;
using System.IO;

namespace BluwolfIcons
{
	/// <summary>
	/// Represents an icon image file.
	/// </summary>
	public class Icon
	{
		public IList<IIconImage> Images { get; } = new List<IIconImage>();

		/// <summary>
		/// Saves this icon to a specified file.
		/// </summary>
		/// <param name="filename">The file to save this icon to.</param>
		public void Save(string filename)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves this icon to a specified stream.
		/// </summary>
		/// <param name="stream">The stream to save this icon to.</param>
		public void Save(Stream stream)
		{
			throw new NotImplementedException();
		}
	}
}
