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
			get => tileWidth;
			set {
				if (Tiles.Count > 0 && tileWidth != value) {
					throw new InvalidOperationException("Cannot change tile width for a nonempty tileset");
				}
				tileWidth = value;
			}
		}
		/// <summary>
		/// Gets or sets the height of a tile in the tileset, in pixels.
		/// The height can only be modified if the tileset is empty.
		/// </summary>
		public int TileHeight {
			get => tileHeight;
			set {
				if (Tiles.Count > 0 && tileHeight != value) {
					throw new InvalidOperationException("Cannot change tile height for a nonempty tileset");
				}
				tileHeight = value;
			}
		}
		/// <summary>
		/// Gets the number of tiles in the tileset.
		/// </summary>
		public int Count => Tiles.Count;
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
			ColorFormat = ColorFormat.ARGB8888;
			IsIndexed = false;

			Tiles = new List<Tile>();
			TileDictionary = new MultiValueDictionary<int, TileEntry>();
		}

		public Tile this[int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return Tiles[index];
			}
		}

		/// <summary>
		/// Clears the tileset.
		/// </summary>
		public void Clear() {
			Tiles.Clear();
			TileDictionary.Clear();
		}

		/// <summary>
		/// Adds the specified tile to the tileset, even if an equivalent tile is already in the tileset.
		/// </summary>
		/// <param name="tile">The tile to add.</param>
		/// <returns>The created tile entry for the tile.</returns>
		public TileEntry AddTile(Tile tile) {
			ArgumentNullException.ThrowIfNull(tile);

			int tileNum = Count;
			Tiles.Add(tile);

			TileEntry entry = AddTileEntry(tile, tileNum, false, false);
			AddTileEntry(tile, tileNum, true,  false);
			AddTileEntry(tile, tileNum, false, true);
			AddTileEntry(tile, tileNum, true,  true);

			return entry;
		}
		private TileEntry AddTileEntry(Tile tile, int tileNumber, bool hFlip, bool vFlip) {
			int hash = tile.GetHashCode(hFlip, vFlip);
			TileEntry entry = new(tileNumber, hFlip, vFlip);

			TileDictionary.Add(hash, entry);
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
			ArgumentNullException.ThrowIfNull(tile);

			foreach (TileEntry candidate in TileDictionary[tile.GetHashCode()]) {
				if ((candidate.HFlip && !canHFlip) || (candidate.VFlip && !canVFlip)) {
					continue;
				}

				Tile candidateTile = Tiles[candidate.TileNumber];
				if (tile.Equals(candidateTile, candidate.HFlip, candidate.VFlip)) {
					entry = candidate;
					return true;
				}
			}
			entry = default;
			return false;
		}

		public IEnumerator<Tile> GetEnumerator() => Tiles.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Tiles.GetEnumerator();

		/// <summary>
		/// Converts this tileset to a bitmap.
		/// </summary>
		/// <param name="maxTilesPerRow">The maximum number of tiles per row. If set to 0, no maximum is used.</param>
		/// <param name="targetFmt">The target color format.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmap(in int maxTilesPerRow, ColorFormat targetFmt) {
			if (maxTilesPerRow < 0)
				throw new ArgumentOutOfRangeException(nameof(maxTilesPerRow));

			int hTileCount = Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (Count + hTileCount - 1) / hTileCount;

			Bitmap bmp = new(TileWidth * hTileCount, TileHeight * vTileCount);

			// Draw all tiles in the tileset.
			for (int t = 0; t < Count; t++) {
				Tile tile = this[t];
				int ti = t % hTileCount;
				int tj = t / hTileCount;

				for (int ty = 0; ty < TileHeight; ty++) {
					for (int tx = 0; tx < TileWidth; tx++) {
						int px = ti * TileWidth + tx;
						int py = tj * TileHeight + ty;

						int c = tile[tx, ty];
						bmp[px, py] = targetFmt.Convert(c, ColorFormat);
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
			ArgumentNullException.ThrowIfNull(palettes);

			int hTileCount = Count;
			if (maxTilesPerRow > 0 && hTileCount > maxTilesPerRow) {
				hTileCount = maxTilesPerRow;
			}
			int vTileCount = (Count + hTileCount - 1) / hTileCount;

			Bitmap bmp = new(TileWidth * hTileCount, TileHeight * vTileCount);
				
			// Draw all tiles in the tileset.
			for (int t = 0; t < Count; t++) {
				Tile tile = this[t];
				int ti = t % hTileCount;
				int tj = t / hTileCount;
				Palette? pal = palettes.FindPalette(tile.PaletteNumber);
				if (pal is null && palettes.Count > 0) {
					// Don't have the palette used to index this tile anymore.
					// Just get any palette.
					pal = palettes[0].Palette;
				}

				for (int ty = 0; ty < TileHeight; ty++) {
					for (int tx = 0; tx < TileWidth; tx++) {
						int px = ti * TileWidth + tx;
						int py = tj * TileHeight + ty;

						int c = tile[tx, ty];
						if (pal is not null && c < pal.Count) {
							c = targetFmt.Convert(pal[c], pal.Format);
						} else {
							c = targetFmt.Convert(c, ColorFormat);
						}
						bmp[px, py] = c;
					}
				}
			}

			return bmp;
		}

		public Tileset Clone() {
			Tileset clone = new(TileWidth, TileHeight);
			foreach (Tile tile in Tiles) {
				clone.Tiles.Add(tile.Clone());
			}
			foreach (KeyValuePair<int, IList<TileEntry>> kvp in TileDictionary) {
				foreach (TileEntry te in kvp.Value) {
					clone.TileDictionary.Add(kvp.Key, te);
				}
			}
			clone.ColorFormat = ColorFormat;
			clone.IsIndexed = IsIndexed;
			return clone;
		}
		object ICloneable.Clone() => Clone();
	}
}
