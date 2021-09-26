using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace PixelPet {
	/// <summary>
	/// PixelPet workbench instance.
	/// </summary>
	internal class Workbench {
		public PaletteSet PaletteSet { get; set; }
		public Bitmap Bitmap { get; private set; }
		public Graphics Graphics { get; private set; }
		public MemoryStream Stream { get; private set; }
		public Tileset Tileset { get; set; }
		public Tilemap Tilemap { get; set; }
		public ColorFormat BitmapFormat { get; set; }
		public BitmapFormat TilemapFormat { get; set; }

		public Workbench() {
			this.PaletteSet = new PaletteSet();
			this.ClearBitmap(8, 8);
			this.Stream = new MemoryStream();
			this.Tileset = new Tileset(8, 8);
			this.Tilemap = new Tilemap(LibPixelPet.BitmapFormat.GBA4BPP);
		}

		public void ClearBitmap(in int width, in int height) {
			this.Bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			this.Graphics = Graphics.FromImage(this.Bitmap);

			this.Graphics.Clear(Color.Transparent);
			this.Graphics.Flush();

			this.BitmapFormat = ColorFormat.ARGB8888;
			this.TilemapFormat = LibPixelPet.BitmapFormat.GBA4BPP;
		}

		public void SetBitmap(Bitmap bmp) {
			this.Graphics?.Dispose();
			this.Bitmap?.Dispose();

			this.Bitmap = bmp;
			this.Graphics = Graphics.FromImage(this.Bitmap);

			this.BitmapFormat = ColorFormat.ARGB8888;
			this.TilemapFormat = LibPixelPet.BitmapFormat.GBA4BPP;
		}

		public Bitmap GetCroppedBitmap(int x, int y, int width, int height, ILogger logger = null) {
			// Range checks to make sure at least a 1x1 bitmap is cropped.
			if (x < 0) {
				x = 0;
				logger?.Log("x-position outside of range; set to 0.", LogLevel.Warning);
			} else if (x >= this.Bitmap.Width) {
				x = this.Bitmap.Width - 1;
				logger?.Log("x-position outside of range; set to bitmap width - 1 (= " + x + ").", LogLevel.Warning);
			}
			if (y < 0) {
				y = 0;
				logger?.Log("y-position outside of range; set to 0.", LogLevel.Warning);
			} else if (y >= this.Bitmap.Height) {
				y = this.Bitmap.Height - 1;
				logger?.Log("y-position outside of range; set to bitmap height - 1 (= " + y + ").", LogLevel.Warning);
			}
			if (width < 0) {
				width = this.Bitmap.Width - x;
			}
			if (height < 0) {
				height = this.Bitmap.Height - y;
			}
			if (width < 1) {
				width = 1;
				logger?.Log("Width outside of range; set to 1.", LogLevel.Warning);
			} else if (x + width > this.Bitmap.Width) {
				width = this.Bitmap.Width - x;
				logger?.Log("Width outside of range; set to bitmap width - x (= " + width + ").", LogLevel.Warning);
			}
			if (height < 1) {
				height = 1;
				logger?.Log("Height outside of range; set to 1.", LogLevel.Warning);
			} else if (y + height > this.Bitmap.Height) {
				height = this.Bitmap.Height - y;
				logger?.Log("Height outside of range; set to bitmap height - y (= " + height + ").", LogLevel.Warning);
			}

			return this.CropBitmap(x, y, width, height);
		}

		private Bitmap CropBitmap(int x, int y, int width, int height) {
			Bitmap bmp = null;
			try {
				bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				BitmapData dstData = bmp.LockBits(
					new Rectangle(0, 0, width, height),
					ImageLockMode.WriteOnly,
					bmp.PixelFormat
				);
				BitmapData srcData = this.Bitmap.LockBits(
					new Rectangle(x, y, width, height),
					ImageLockMode.ReadOnly,
					this.Bitmap.PixelFormat
				);
				int[] buffer = new int[width];

				for (int j = 0; j < height; j++) {
					Marshal.Copy(srcData.Scan0 + j * srcData.Stride, buffer, 0, width);
					Marshal.Copy(buffer, 0, dstData.Scan0 + j * dstData.Stride, width);
				}

				bmp.UnlockBits(dstData);
				this.Bitmap.UnlockBits(srcData);

				return bmp;
			} catch {
				bmp?.Dispose();
				throw;
			}
		}
	}
}
