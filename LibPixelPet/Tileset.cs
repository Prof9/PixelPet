using System;
using System.Collections;
using System.Collections.Generic;

namespace LibPixelPet {
	public class Tileset : IEnumerable<Tile>, ICloneable {
		private List<Tile> Tiles { get; }
		private MultiValueDictionary<int, TileEntry> TileDictionary { get; }

		private int tileWidth;
		private int tileHeight;

		/// <summary>
		/// Gets or sets the width of a tile in the tileset, in pixels.
		/// The width can only be modified if the tileset is empty.
		/// </summary>
		public int TileWidth {
			get => this.tileWidth;
			set {
				if (this.Tiles.Count > 0 && this.tileWidth != value) {
					throw new InvalidOperationException("Cannot change tile width for a nonempty tileset");
				}
				this.tileWidth = value;
			}
		}
		/// <summary>
		/// Gets or sets the height of a tile in the tileset, in pixels.
		/// The height can only be modified if the tileset is empty.
		/// </summary>
		public int TileHeight {
			get => this.tileHeight;
			set {
				if (this.Tiles.Count > 0 && this.tileHeight != value) {
					throw new InvalidOperationException("Cannot change tile height for a nonempty tileset");
				}
				this.tileHeight = value;
			}
		}
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

			this.tileWidth = tileWidth;
			this.tileHeight = tileHeight;
			this.ColorFormat = ColorFormat.ARGB8888;
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
		/// Clears the tileset.
		/// </summary>
		public void Clear() {
			this.Tiles.Clear();
			this.TileDictionary.Clear();
		}

		/// <summary>
		/// Adds the specified tile to the tileset, even if an equivalent tile is already in the tileset.
		/// </summary>
		/// <param name="tile">The tile to add.</param>
		/// <returns>The created tile entry for the tile.</returns>
		public TileEntry AddTile(Tile tile) {
			int tileNum = this.Count;
			this.Tiles.Add(tile);

			TileEntry entry = this.AddTileEntry(tile, tileNum, false, false);
			this.AddTileEntry(tile, tileNum, true,  false);
			this.AddTileEntry(tile, tileNum, false, true);
			this.AddTileEntry(tile, tileNum, true,  true);

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
		/// <param name="canHFlip">Whether the tile can be horizontally flipped.</param>
		/// <param name="canVFlip">Whether the tile can be vertically flipped.</param>
		/// <param name="entry">The found tile entry, or null if no suitable tile entry was found.</param>
		/// <returns>true if a tile entry was found; otherwise, false.</returns>
		public bool TryFindTileEntry(in Tile tile, bool canHFlip, bool canVFlip, out TileEntry entry) {
			foreach (TileEntry candidate in this.TileDictionary[tile.GetHashCode()]) {
				if ((candidate.HFlip && !canHFlip) || (candidate.VFlip && !canVFlip)) {
					continue;
				}

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
		/// <param name="targetFmt">The target color format.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmap(in int maxTilesPerRow, ColorFormat targetFmt) {
			if (maxTilesPerRow < 0)
				throw new ArgumentOutOfRangeException(nameof(maxTilesPerRow));

			int hTileCount = this.Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (this.Count + hTileCount - 1) / hTileCount;

			Bitmap bmp = new Bitmap(this.TileWidth * hTileCount, this.TileHeight * vTileCount);

			// Draw all tiles in the tileset.
			for (int t = 0; t < this.Count; t++) {
				Tile tile = this[t];
				int ti = t % hTileCount;
				int tj = t / hTileCount;

				for (int ty = 0; ty < this.TileHeight; ty++) {
					for (int tx = 0; tx < this.TileWidth; tx++) {
						int px = ti * this.TileWidth + tx;
						int py = tj * this.TileHeight + ty;

						int c = tile[tx, ty];
						bmp[px, py] = targetFmt.Convert(c, this.ColorFormat);
					}
				}
			}

			return bmp;
		}

		/// <summary>
		/// Converts this tileset to an indexed bitmap.
		/// </summary>
		/// <param name="maxTilesPerRow">The maximum number of tiles per row. If set to 0, no maximum is used.</param>
		/// <param name="palettes">The palettes to use.</param>
		/// <param name="targetFmt">The target color format.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmapIndexed(in int maxTilesPerRow, PaletteSet palettes, ColorFormat targetFmt) {
			if (maxTilesPerRow < 0)
				throw new ArgumentOutOfRangeException(nameof(maxTilesPerRow));
			if (palettes is null)
				throw new ArgumentNullException(nameof(palettes));

			int hTileCount = this.Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (this.Count + hTileCount - 1) / hTileCount;

			Bitmap bmp = new Bitmap(this.TileWidth * hTileCount, this.TileHeight * vTileCount);
				
			// Draw all tiles in the tileset.
			for (int t = 0; t < this.Count; t++) {
				Tile tile = this[t];
				int ti = t % hTileCount;
				int tj = t / hTileCount;
				Palette pal = palettes.FindPalette(tile.PaletteNumber);
				if (pal is null && palettes.Count > 0) {
					// Don't have the palette used to index this tile anymore.
					// Just get any palette.
					pal = palettes[0].Palette;
				}

				for (int ty = 0; ty < this.TileHeight; ty++) {
					for (int tx = 0; tx < this.TileWidth; tx++) {
						int px = ti * this.TileWidth + tx;
						int py = tj * this.TileHeight + ty;

						int c = tile[tx, ty];
						if (pal is not null && c < pal.Count) {
							c = targetFmt.Convert(pal[c], pal.Format);
						} else {
							c = targetFmt.Convert(c, this.ColorFormat);
						}
						bmp[px, py] = c;
					}
				}
			}

			return bmp;
		}

		public Tileset Clone() {
			Tileset clone = new Tileset(this.TileWidth, this.TileHeight);
			foreach (Tile tile in this.Tiles) {
				clone.Tiles.Add(tile.Clone());
			}
			foreach (KeyValuePair<int, IList<TileEntry>> kvp in this.TileDictionary) {
				foreach (TileEntry te in kvp.Value) {
					clone.TileDictionary.Add(kvp.Key, te);
				}
			}
			clone.ColorFormat = this.ColorFormat;
			clone.IsIndexed = this.IsIndexed;
			return clone;
		}
		object ICloneable.Clone() => this.Clone();
	}
}
