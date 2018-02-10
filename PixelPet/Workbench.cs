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
		public Tileset Tileset { get; set; }
		public Tilemap Tilemap { get; set; }

		public Workbench() {
			this.Palette = new List<Color>();
			ClearBitmap(8, 8);
			this.Stream = new MemoryStream();
			this.Tileset = new Tileset(8, 8);
			this.Tilemap = new Tilemap();
		}

		public void ClearBitmap(int width, int height) {
			this.Bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			this.Graphics = Graphics.FromImage(this.Bitmap);

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

		public Bitmap GetCroppedBitmap(int x, int y, int width, int height, Cli cli) {
			// Range checks to make sure at least a 1x1 bitmap is cropped.
			if (x < 0) {
				x = 0;
				cli?.Log("WARNING: x-position outside of range; set to 0.");
			} else if (x >= this.Bitmap.Width) {
				x = this.Bitmap.Width - 1;
				cli?.Log("WARNING: x-position outside of range; set to bitmap width - 1 (= " + x + ").");
			}
			if (y < 0) {
				y = 0;
				cli?.Log("WARNING: y-position outside of range; set to 0.");
			} else if (y >= this.Bitmap.Height) {
				y = this.Bitmap.Height - 1;
				cli?.Log("WARNING: y-position outside of range; set to bitmap height - 1 (= " + y + ").");
			}
			if (width < 0) {
				width = this.Bitmap.Width - x;
			}
			if (height < 0) {
				height = this.Bitmap.Height - y;
			}
			if (width < 1) {
				width = 1;
				cli?.Log("WARNING: width outside of range; set to 1.");
			} else if (x + width > this.Bitmap.Width) {
				width = this.Bitmap.Width - x;
				cli?.Log("WARNING: width outside of range; set to bitmap width - x (= " + width + ").");
			}
			if (height < 1) {
				height = 1;
				cli?.Log("WARNING: height outside of range; set to 1.");
			} else if (y + height > this.Bitmap.Height) {
				height = this.Bitmap.Height - y;
				cli?.Log("WARNING: height outside of range; set to bitmap height - y (= " + height + ").");
			}

			Bitmap bmp = new Bitmap(width, height);
			using (Graphics g = Graphics.FromImage(bmp)) {
				g.DrawImage(this.Bitmap, 0, 0, new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
				g.Flush();
			}
			return bmp;
		}
	}
}
