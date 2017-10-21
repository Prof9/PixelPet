using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet {
	public class Tilemap {
		public struct Entry {
			public int TileNumber;
			public int InternalTileNumber;
			public bool FlipHorizontal;
			public bool FlipVertical;

			public Entry(int iTileNum) {
				this.TileNumber = iTileNum;
				this.InternalTileNumber = iTileNum;
				this.FlipHorizontal = false;
				this.FlipVertical = false;
			}
		}

		public List<Entry> TileEntries { get; private set; }
		public int TileCount => this.TileEntries.Count;

		protected BitmapData BitmapData { get; private set; }
		protected int[] Buffer { get; private set; }

		public int TileWidth { get; private set; }
		public int TileHeight { get; private set; }
		protected int InternalHorizontalTileCount { get; private set; }
		protected int InternalVerticalTileCount { get; private set; }
		protected int InternalTileCount => this.InternalHorizontalTileCount * this.InternalVerticalTileCount;

		public Tilemap(Bitmap bmp, int tileWidth, int tileHeight) {
			this.TileWidth = tileWidth;
			this.TileHeight = tileHeight;

			this.InternalHorizontalTileCount = (bmp.Width  + tileWidth  - 1) / tileWidth;
			this.InternalVerticalTileCount   = (bmp.Height + tileHeight - 1) / tileHeight;

			this.TileEntries = new List<Entry>(this.InternalTileCount);
			for (int t = 0; t < this.InternalTileCount; t++) {
				this.TileEntries.Add(new Entry(t));
			}

			this.BitmapData = bmp.LockBits(
				new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly,
				bmp.PixelFormat
			);
			this.Buffer = new int[(this.BitmapData.Stride * this.BitmapData.Height) / 4];
			Marshal.Copy(this.BitmapData.Scan0, this.Buffer, 0, this.Buffer.Length);
			bmp.UnlockBits(this.BitmapData);
		}

		public Bitmap GetTileset() {
			int tileCount = this.TileEntries.Max(te => te.TileNumber) + 1;
			int horizontalTileCount = 1;

			Bitmap bmp = new Bitmap(this.TileWidth, tileCount * this.TileHeight, PixelFormat.Format32bppArgb);
			BitmapData bmpData = bmp.LockBits(
				new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.WriteOnly,
				PixelFormat.Format32bppArgb
			);
			int[] buffer = new int[(bmpData.Stride * bmp.Height) / 4];

			foreach (Entry entry in this.TileEntries) {
				int t = entry.TileNumber;
				int ti = t % horizontalTileCount;
				int tj = t / horizontalTileCount;

				for (int ty = 0; ty < this.TileHeight; ty++) {
					for (int tx = 0; tx < this.TileWidth; tx++) {
						int px = ti * this.TileWidth  + tx;
						int py = tj * this.TileHeight + ty;
						int ptr = (py * bmpData.Stride + px * 4) / 4;

						buffer[ptr] = this.GetPixel(entry.InternalTileNumber, tx, ty);
					}
				}
			}

			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
			bmp.UnlockBits(bmpData);
			return bmp;
		}

		public void Reduce(bool flip) {
			Dictionary<int, List<Entry>> dict = new Dictionary<int, List<Entry>>();
			int rTileNum = 0;
			for (int t = 0; t < this.InternalTileCount; t++) {
				// Try to reduce tile.
				Entry entry = ReduceTile(dict, t, false, false, rTileNum);
				this.TileEntries[t] = entry;

				// Check if tile was reduced.
				if (entry.TileNumber != rTileNum) {
					continue;
				}

				if (flip) {
					ReduceTile(dict, t, true,  false, rTileNum);
					ReduceTile(dict, t, false, true,  rTileNum);
					ReduceTile(dict, t, true,  true,  rTileNum);
				}
				rTileNum++;
			}
		}

		private Entry ReduceTile(Dictionary<int, List<Entry>> dict, int iTileNum, bool hflip, bool vflip, int rTileNum) {
			int hash = this.GetTileHash(iTileNum, hflip, vflip);

			// Check if tile already in library.
			if (!dict.ContainsKey(hash)) {
				dict[hash] = new List<Entry>();
			} else {
				foreach (Entry candidate in dict[hash]) {
					if (this.GetTile(iTileNum, hflip, vflip).SequenceEqual(this.GetTile(candidate))) {
						return candidate;
					}
				}
			}

			Entry entry = new Entry(iTileNum) {
				FlipHorizontal = hflip,
				FlipVertical = vflip,
				TileNumber = rTileNum,
			};
			dict[hash].Add(entry);
			return entry;
		}

		protected int GetTileHash(int iTileNum, bool hflip, bool vflip) {
			// FNV-1a
			uint hash = 0x811C9DC5;
			int rol = 0;
			foreach (int pixel in this.GetTile(iTileNum, hflip, vflip)) {
				// Rotate pixel based on position in tile.
				foreach (byte b in BitConverter.GetBytes(pixel)) {
					hash = (hash ^ b) * 16777619;
				}

				rol = (rol + 1) % 32;
			}

			return (int)hash;
		}

		public int GetPixel(int iTileNum, int tx, int ty) {
			int ti = iTileNum % this.InternalHorizontalTileCount;
			int tj = iTileNum / this.InternalHorizontalTileCount;
			int px = ti * this.TileWidth  + tx;
			int py = tj * this.TileHeight + ty;
			if (px < 0 || py < 0 || px >= this.BitmapData.Width || py >= this.BitmapData.Height) {
				return 0;
			} else {
				int ptr = (py * this.BitmapData.Stride + px * 4) / 4;
				return this.Buffer[ptr];
			}
		}

		public IEnumerable<int> GetTile(Entry entry) {
			return this.GetTile(entry.InternalTileNumber, entry.FlipHorizontal, entry.FlipVertical);
		}

		protected IEnumerable<int> GetTile(int iTileNum, bool hflip, bool vflip) {
			int tx, txStart, txEnd, txInc;
			if (hflip) {
				txStart = this.TileWidth - 1;
				txEnd = -1;
				txInc = -1;
			} else {
				txStart = 0;
				txEnd = this.TileWidth;
				txInc = 1;
			}

			int ty, tyStart, tyEnd, tyInc;
			if (vflip) {
				tyStart = this.TileHeight - 1;
				tyEnd = -1;
				tyInc = -1;
			} else {
				tyStart = 0;
				tyEnd = this.TileHeight;
				tyInc = 1;
			}

			ty = tyStart;
			while (ty != tyEnd) {
				tx = txStart;
				while (tx != txEnd) {
					yield return this.GetPixel(iTileNum, tx, ty);
					tx += txInc;
				}
				ty += tyInc;
			}
		}
	}
}
