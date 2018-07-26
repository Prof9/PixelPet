using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LibPixelPet {
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class Tileset : IEnumerable<Tile> {
		private List<Tile> Tiles { get; }
		private MultiValueDictionary<int, TileEntry> TileDictionary { get; }

		/// <summary>
		/// Gets the width of a tile in the tileset, in pixels.
		/// </summary>
		public int TileWidth { get; }
		/// <summary>
		/// Gets the height of a tile in the tileset, in pixels.
		/// </summary>
		public int TileHeight { get; }
		/// <summary>
		/// Gets the number of tiles in the tileset.
		/// </summary>
		public int Count => this.Tiles.Count;
		/// <summary>
		/// Gets or sets the color format used for tiles in the tileset.
		/// </summary>
		public ColorFormat ColorFormat { get; set; }
		/// <summary>
		/// Gets or sets a Boolean that indicates whether this tileset is indexed.
		/// </summary>
		public bool IsIndexed { get; set; }

		/// <summary>
		/// Creates a new tileset with the specified tile width and height.
		/// </summary>
		/// <param name="tileWidth">The width of each tile in pixels.</param>
		/// <param name="tileHeight">The height of each tile in pixels.</param>
		public Tileset(int tileWidth, int tileHeight) {
			if (tileWidth < 1)
				throw new ArgumentOutOfRangeException(nameof(tileWidth));
			if (tileHeight < 1)
				throw new ArgumentOutOfRangeException(nameof(tileHeight));

			this.TileWidth = tileWidth;
			this.TileHeight = tileHeight;
			this.ColorFormat = ColorFormat.BGRA8888;
			this.IsIndexed = false;

			this.Tiles = new List<Tile>();
			this.TileDictionary = new MultiValueDictionary<int, TileEntry>();
		}

		public Tile this[int index] {
			get {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return this.Tiles[index];
			}
		}

		/// <summary>
		/// Adds the specified tile to the tileset, even if an equivalent tile is already in the tileset.
		/// </summary>
		/// <param name="tile">The tile to add.</param>
		/// <returns>The created tile entry for the tile.</returns>
		public TileEntry AddTile(Tile tile, bool hFlip, bool vFlip) {
			int tileNum = this.Count;
			this.Tiles.Add(tile);

			TileEntry entry = this.AddTileEntry(tile, tileNum, false, false);
			if (hFlip) {
				this.AddTileEntry(tile, tileNum, true, false);
			}
			if (vFlip) {
				this.AddTileEntry(tile, tileNum, false, true);
			}
			if (hFlip && vFlip) {
				this.AddTileEntry(tile, tileNum, true, true);
			}

			return entry;
		}
		private TileEntry AddTileEntry(Tile tile, int tileNumber, bool hFlip, bool vFlip) {
			int hash = tile.GetHashCode(hFlip, vFlip);
			TileEntry entry = new TileEntry(tileNumber, hFlip, vFlip);

			this.TileDictionary.Add(hash, entry);
			return entry;
		}

		/// <summary>
		/// Finds a suitable tile entry for the specified tile.
		/// </summary>
		/// <param name="tile">The tile to find a tile entry for.</param>
		/// <param name="entry">The found tile entry, or null if no suitable tile entry was found.</param>
		/// <returns>true if a tile entry was found; otherwise, false.</returns>
		public bool TryFindTileEntry(in Tile tile, out TileEntry entry) {
			foreach (TileEntry candidate in this.TileDictionary[tile.GetHashCode()]) {
				Tile candidateTile = this.Tiles[candidate.TileNumber];
				if (tile.Equals(candidateTile, candidate.HFlip, candidate.VFlip)) {
					entry = candidate;
					return true;
				}
			}
			entry = default;
			return false;
		}

		public IEnumerator<Tile> GetEnumerator() => this.Tiles.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.Tiles.GetEnumerator();

		/// <summary>
		/// Converts this tileset to a bitmap.
		/// </summary>
		/// <param name="maxTilesPerRow">The maximum number of tiles per row. If set to 0, no maximum is used.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmap(in int maxTilesPerRow) {
			if (maxTilesPerRow < 0)
				throw new ArgumentOutOfRangeException(nameof(maxTilesPerRow));

			int hTileCount = this.Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (this.Count + hTileCount - 1) / hTileCount;

			ColorFormat bgra8888 = ColorFormat.BGRA8888;

			Bitmap bmp = null;
			try {
				bmp = new Bitmap(
					this.TileWidth * hTileCount,
					this.TileHeight * vTileCount,
					PixelFormat.Format32bppArgb
				);
				BitmapData bmpData = bmp.LockBits(
					new Rectangle(0, 0, bmp.Width, bmp.Height),
					ImageLockMode.WriteOnly,
					bmp.PixelFormat
				);
				int[] buffer = new int[(bmpData.Stride * bmp.Height) / 4];

				// Draw all tiles in the tileset.
				for (int t = 0; t < this.Count; t++) {
					Tile tile = this[t];
					int ti = t % hTileCount;
					int tj = t / hTileCount;

					for (int ty = 0; ty < this.TileHeight; ty++) {
						for (int tx = 0; tx < this.TileWidth; tx++) {
							int px = ti * this.TileWidth + tx;
							int py = tj * this.TileHeight + ty;
							int ptr = (py * bmpData.Stride + px * 4) / 4;

							int c = tile[tx, ty];
							buffer[ptr] = bgra8888.Convert(c, this.ColorFormat);
						}
					}
				}

				Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
				bmp.UnlockBits(bmpData);

				return bmp;
			} catch {
				bmp?.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Converts this tileset to an indexed bitmap.
		/// </summary>
		/// <param name="maxTilesPerRow">The maximum number of tiles per row. If set to 0, no maximum is used.</param>
		/// <param name="palettes">The palettes to use.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmapIndexed(in int maxTilesPerRow, PaletteSet palettes) {
			if (maxTilesPerRow < 0)
				throw new ArgumentOutOfRangeException(nameof(maxTilesPerRow));
			if (palettes == null)
				throw new ArgumentNullException(nameof(palettes));

			int hTileCount = this.Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (this.Count + hTileCount - 1) / hTileCount;

			ColorFormat bgra8888 = ColorFormat.BGRA8888;

			Bitmap bmp = null;
			try {
				bmp = new Bitmap(
					this.TileWidth * hTileCount,
					this.TileHeight * vTileCount,
					PixelFormat.Format32bppArgb
				);
				BitmapData bmpData = bmp.LockBits(
					new Rectangle(0, 0, bmp.Width, bmp.Height),
					ImageLockMode.WriteOnly,
					bmp.PixelFormat
				);
				int[] buffer = new int[(bmpData.Stride * bmp.Height) / 4];

				// Draw all tiles in the tileset.
				for (int t = 0; t < this.Count; t++) {
					Tile tile = this[t];
					int ti = t % hTileCount;
					int tj = t / hTileCount;
					Palette pal = palettes.FindPalette(tile.PaletteNumber);

					for (int ty = 0; ty < this.TileHeight; ty++) {
						for (int tx = 0; tx < this.TileWidth; tx++) {
							int px = ti * this.TileWidth + tx;
							int py = tj * this.TileHeight + ty;
							int ptr = (py * bmpData.Stride + px * 4) / 4;

							int c = tile[tx, ty];
							if (pal != null && c < pal.Count) {
								c = pal[c];
							} else {
								c = 0;
							}
							buffer[ptr] = bgra8888.Convert(c, pal.Format);
						}
					}
				}

				Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
				bmp.UnlockBits(bmpData);

				return bmp;
			} catch {
				bmp?.Dispose();
				throw;
			}
		}
	}
}
