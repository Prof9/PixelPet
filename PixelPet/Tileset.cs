using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet {
	public class Tileset {
		private IList<Tile> _Tiles { get; }
		public IEnumerable<Tile> Tiles {
			get {
				foreach (Tile tile in this._Tiles) {
					yield return tile;
				}
			}
		}
		private Dictionary<int, List<TileEntry>> TileDictionary { get; }

		public int TileWidth { get; private set; }
		public int TileHeight { get; private set; }
		public int TileCount => this._Tiles.Count;

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

			this._Tiles = new List<Tile>();
			this.TileDictionary = new Dictionary<int, List<TileEntry>>();
		}

		/// <summary>
		/// Adds the specified tiles to the tileset if no equivalents exist yet.
		/// </summary>
		/// <param name="tiles">The tiles to add.</param>
		public void AddTiles(IEnumerable<Tile> tiles) {
			if (tiles == null)
				throw new ArgumentNullException(nameof(tiles));

			foreach (Tile tile in tiles) {
				this.FindOrAddTile(tile);
			}
		}
		/// <summary>
		/// Finds the specified tile in the tileset, or adds it if it is not yet in the tileset.
		/// </summary>
		/// <param name="tile">The tile to find or add.</param>
		/// <returns>The found or created tile entry for the tile.</returns>
		public TileEntry FindOrAddTile(Tile tile) {
			if (tile == null)
				throw new ArgumentNullException(nameof(tile));

			// Check if tile already in dictionary.
			TileEntry? entry = this.FindTile(tile);
			if (entry != null) {
				return (TileEntry)entry;
			}

			// Add tile to the tileset.
			return this.AddTile(tile);
		}
		/// <summary>
		/// Adds the specified tile to the tileset, even if an equivalent tile is already in the tileset.
		/// </summary>
		/// <param name="tile">The tile to add.</param>
		/// <returns>The created tile entry for the tile.</returns>
		public TileEntry AddTile(Tile tile) {
			int tileNum = this.TileCount;

			this._Tiles.Add(tile);

			TileEntry entry = this.AddTileEntry(tile, tileNum, false, false);
			this.AddTileEntry(tile, tileNum, true, false);
			this.AddTileEntry(tile, tileNum, false, true);
			this.AddTileEntry(tile, tileNum, true, true);

			return entry;
		}
		private TileEntry AddTileEntry(Tile tile, int tileNum, bool hflip, bool vflip) {
			int hash = tile.GetHashCode(hflip, vflip);
			if (!this.TileDictionary.ContainsKey(hash)) {
				this.TileDictionary[hash] = new List<TileEntry>();
			}
			TileEntry entry = new TileEntry(tileNum, hflip, vflip);
			this.TileDictionary[hash].Add(entry);
			return entry;
		}
		/// <summary>
		/// Gets the tile with the specified tile number.
		/// </summary>
		/// <param name="tileNum">The tile number.</param>
		/// <returns>The tile.</returns>
		public Tile GetTile(int tileNum) {
			if (tileNum < 0 || tileNum >= this._Tiles.Count)
				throw new ArgumentOutOfRangeException(nameof(tileNum));

			return this._Tiles[tileNum];
		}

		/// <summary>
		/// Converts this tileset to a bitmap.
		/// </summary>
		/// <param name="maxTilesPerRow">The maximum number of tiles per row. If set to 0, no maximum is used.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmap(int maxTilesPerRow) {
			if (maxTilesPerRow < 0)
				throw new ArgumentOutOfRangeException(nameof(maxTilesPerRow));

			int hTileCount = this._Tiles.Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (this._Tiles.Count + hTileCount - 1) / hTileCount;

			Bitmap result = null;
			Bitmap bmp = null;
			try {
				bmp = new Bitmap(this.TileWidth * hTileCount, this.TileHeight * vTileCount, PixelFormat.Format32bppArgb);
				BitmapData bmpData = bmp.LockBits(
					new Rectangle(0, 0, bmp.Width, bmp.Height),
					ImageLockMode.WriteOnly,
					PixelFormat.Format32bppArgb
				);
				int[] buffer = new int[(bmpData.Stride * bmp.Height) / 4];

				// Draw all tiles in the tileset.
				for (int t = 0; t < this._Tiles.Count; t++) {
					Tile tile = this._Tiles[t];
					int ti = t % hTileCount;
					int tj = t / hTileCount;

					for (int ty = 0; ty < this.TileHeight; ty++) {
						for (int tx = 0; tx < this.TileWidth; tx++) {
							int px = ti * this.TileWidth + tx;
							int py = tj * this.TileHeight + ty;
							int ptr = (py * bmpData.Stride + px * 4) / 4;

							buffer[ptr] = tile.GetPixel(tx, ty);
						}
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

		/// <summary>
		/// Finds a suitable tile entry for the specified tile.
		/// </summary>
		/// <param name="tile">The tile.</param>
		/// <returns>The found tile entry, or null if no suitable tile entry was found.</returns>
		private TileEntry? FindTile(Tile tile) {
			int hash = tile.GetHashCode();
			if (this.TileDictionary.ContainsKey(hash)) {
				foreach (TileEntry candidate in this.TileDictionary[hash]) {
					Tile candidateTile = this._Tiles[candidate.TileNumber];
					if (tile.Equals(candidateTile, candidate.HFlip, candidate.VFlip)) {
						return candidate;
					}
				}
			}
			return null;
		}
	}
}
