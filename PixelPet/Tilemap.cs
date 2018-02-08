using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet {
	public class Tilemap {
		/// <summary>
		/// Gets the tile entries in this tilemap.
		/// </summary>
		public IList<TileEntry> TileEntries { get; private set; }

		/// <summary>
		/// Creates a new empty tilemap.
		/// </summary>
		public Tilemap() {
			this.TileEntries = new List<TileEntry>();
		}

		/// <summary>
		/// Creates a new tilemap from the specified bitmap and tileset.
		/// </summary>
		/// <param name="bmp">The bitmap.</param>
		/// <param name="tileset">The tileset.</param>
		/// <param name="reduce">If true, reduces the tilemap; otherwise, does not reduce.</param>
		public Tilemap(Bitmap bmp, Tileset tileset, bool reduce)
			: this() {
			if (bmp == null)
				throw new ArgumentNullException(nameof(bmp));
			if (tileset == null)
				throw new ArgumentNullException(nameof(tileset));

			TileCutter cutter = new TileCutter(tileset.TileWidth, tileset.TileWidth);
			foreach (Tile tile in cutter.CutTiles(bmp)) {
				TileEntry entry;
				if (reduce) {
					entry = tileset.FindOrAddTile(tile);
				} else {
					entry = tileset.AddTile(tile);
				}
				this.TileEntries.Add(entry);
			}
		}

		/// <summary>
		/// Creates a new tilemap from the specified bitmap and tileset and indexes it with the specified palettes.
		/// </summary>
		/// <param name="bmp">The bitmap to use.</param>
		/// <param name="tileset">The tileset to use.</param>
		/// <param name="palettes">The palettes to use.</param>
		/// <param name="palSize">The size of each individual palette.</param>
		/// <param name="reduce">If true, reduces the tilemap; otherwise, does not reduce.</param>
		public Tilemap(Bitmap bmp, Tileset tileset, IList<Color> palettes, int palSize, bool reduce)
			: this(bmp, tileset, reduce) {
			if (palettes == null)
				throw new ArgumentNullException(nameof(palettes));
			if (palSize < 1)
				throw new ArgumentOutOfRangeException(nameof(palSize));

			int lastPal = 0;
			int palCount = (palettes.Count + palSize - 1) / palSize;

			// Maps tile numbers to palettes.
			Dictionary<int, int> matchedPalettes = new Dictionary<int, int>();

			for (int i = 0; i < this.TileEntries.Count; i++) {
				TileEntry entry = this.TileEntries[i];
				Tile tile = tileset.GetTile(entry.TileNumber);

				// Check if we previously matched this tile.
				int curPal = 0;
				bool matched = false;
				if (matchedPalettes.ContainsKey(entry.TileNumber)) {
					curPal = matchedPalettes[entry.TileNumber];
					matched = true;
				} else {
					// Adjacent tiles are likely to use the same palette, so start from the previous tile's palette.
					int pal = tile.IndexTileAny(palettes, palSize, curPal);
					if (pal >= 0) {
						curPal = pal;
						matched = true;
					}
				}

				if (!matched) {
					//throw new InvalidOperationException("Could not find a matching palette for tile " + entry.TileNumber);
				}

				entry.PaletteNumber = curPal;
				matchedPalettes[entry.TileNumber] = curPal;
				lastPal = curPal;
				this.TileEntries[i] = entry;
			}
		}

		/// <summary>
		/// Converts this tilemap to a bitmap using the specified tileset.
		/// </summary>
		/// <param name="tileset">The tileset to use.</param>
		/// <param name="width">The width of the tilemap in number of tiles.</param>
		/// <param name="height">The height of the tilemap in number of tiles.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmap(Tileset tileset, int width, int height) {
			if (tileset == null)
				throw new ArgumentNullException(nameof(tileset));
			if (width < 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height < 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			Bitmap result = null;
			Bitmap bmp = null;
			try {
				bmp = new Bitmap(tileset.TileWidth * width, tileset.TileHeight * height, PixelFormat.Format32bppArgb);
				BitmapData bmpData = bmp.LockBits(
					new Rectangle(0, 0, bmp.Width, bmp.Height),
					ImageLockMode.WriteOnly,
					PixelFormat.Format32bppArgb
				);
				int[] buffer = new int[(bmpData.Stride * bmp.Height) / 4];

				// Draw all the tile entries in the tilemap.
				for (int t = 0; t < this.TileEntries.Count; t++) {
					TileEntry te = this.TileEntries[t];
					int ti = t % width;
					int tj = t / width;

					// Stop if height exceeded.
					if (tj >= height) {
						break;
					}

					// Draw the tile for this tile entry.
					Tile tile = tileset.GetTile(te.TileNumber);
					int pi = 0;
					foreach (int p in tile.EnumerateTile(te.HFlip, te.VFlip)) {
						int px = pi % tileset.TileWidth + ti * tileset.TileWidth;
						int py = pi / tileset.TileWidth + tj * tileset.TileHeight;
						int ptr = (py * bmpData.Stride + px * 4) / 4;

						buffer[ptr] = p;
						pi++;
					}
				}

				Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
				bmp.UnlockBits(bmpData);

				// Finished bitmap without errors, finish up to prevent disposing.
				result = bmp;
				bmp = null;
				return result;
			} finally {
				if (bmp != null) {
					bmp.Dispose();
				}
			}
		}
	}
}
