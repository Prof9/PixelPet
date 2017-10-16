using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace PixelPet {
	/// <summary>
	/// PixelPet workbench instance.
	/// </summary>
	public class Workbench {
		public List<Color> Palette { get; }
		public Bitmap Bitmap { get; private set; }
		public Graphics Graphics { get; private set; }

		public Workbench() {
			this.Palette = new List<Color>();
			ClearBitmap(8, 8);
		}

		public void ClearBitmap(int width, int height) {
			if (this.Graphics != null) {
				this.Graphics.Dispose();
			}
			if (this.Bitmap != null) {
				this.Bitmap.Dispose();
			}
			this.Bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			this.Graphics = Graphics.FromImage(this.Bitmap);
			this.Graphics.Clear(Color.Transparent);
			this.Graphics.Flush();
		}
	}
}
