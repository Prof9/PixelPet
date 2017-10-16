using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet {
	public class BitmapImporter {
		public Bitmap Import(string path) {
			return new Bitmap(path);
		}
	}
}
