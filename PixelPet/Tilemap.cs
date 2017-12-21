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
		public IList<TilesetEntry> TileEntries { get; private set; }

		/// <summary>
		/// Creates a new empty tilemap.
		/// </summary>
		public Tilemap() {
			this.TileEntries = new List<TilesetEntry>();
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
				TilesetEntry entry = tileset.FindOrAddTile(tile);
				this.TileEntries.Add(entry);
			}
		}
	}
}
