using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelPet {
	/// <summary>
	/// PixelPet workbench instance.
	/// </summary>
	public class Workbench {
		public IList<Color> Palette { get; }
		public Bitmap Bitmap { get; private set; }
		public Graphics Graphics { get; private set; }
		public MemoryStream Stream { get; private set; }

		public Workbench() {
			this.Palette = new List<Color>();
			ClearBitmap(8, 8);
			this.Stream = new MemoryStream();
		}

		public void ClearBitmap(int width, int height) {
			Bitmap bmp = null;
			try {
				bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			} finally {
				if (bmp != null) {
					bmp.Dispose();
				}
			}
			SetBitmap(bmp);
			this.Graphics.Clear(Color.Transparent);
			this.Graphics.Flush();
		}

		public void SetBitmap(Bitmap bmp) {
			if (this.Graphics != null) {
				this.Graphics.Dispose();
			}
			if (this.Bitmap != null) {
				this.Bitmap.Dispose();
			}
			this.Bitmap = bmp;
			this.Graphics = Graphics.FromImage(this.Bitmap);
		}
	}
}
