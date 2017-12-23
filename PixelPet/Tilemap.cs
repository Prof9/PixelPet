using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
		public Tilemap(Bitmap bmp, Tileset tileset)
			: this() {
			if (bmp == null)
				throw new ArgumentNullException(nameof(bmp));
			if (tileset == null)
				throw new ArgumentNullException(nameof(tileset));

			TileCutter cutter = new TileCutter(tileset.TileWidth, tileset.TileWidth);
			foreach (Tile tile in cutter.CutTiles(bmp)) {
				TileEntry entry = tileset.FindOrAddTile(tile);
				this.TileEntries.Add(entry);
			}
		}

		public Tilemap(Bitmap bmp, Tileset tileset, IList<Color> palettes, int palSize)
			: this(bmp, tileset) {
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
	}
}
