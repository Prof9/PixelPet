using System;
using System.Collections.Generic;
using System.Drawing;

namespace LibPixelPet {
	/// <summary>
	/// Cuts bitmaps into tiles.
	/// </summary>
	public class TileCutter {
		/// <summary>
		/// Gets the width of the produced tiles in pixels.
		/// </summary>
		public int TileWidth { get; }
		/// <summary>
		/// Gets the height of the produced tiles in pixels.
		/// </summary>
		public int TileHeight { get; }
		/// <summary>
		/// Gets the empty color used for out-of-bounds pixels.
		/// </summary>
		public Color EmptyColor { get; }

		/// <summary>
		/// Creates a new tile cutter that cuts tiles of 8 by 8 pixels.
		/// </summary>
		public TileCutter()
			: this(8, 8) { }
		/// <summary>
		/// Creates a new tile cutter that cuts tiles of 8 by 8 pixels.
		/// </summary>
		/// <param name="emptyColor">The empty color to use for out-of-bounds pixels.</param>
		public TileCutter(in Color emptyColor)
			: this(8, 8, emptyColor) { }
		/// <summary>
		/// Creates a new tile cutter that cuts tiles with the specified width and height.
		/// </summary>
		/// <param name="tileWidth">The width of the produced tiles in pixels.</param>
		/// <param name="tileHeight">The height of the produced tiles in pixels.</param>
		public TileCutter(in int tileWidth, in int tileHeight)
			: this(tileWidth, tileHeight, Color.Transparent) { }
		/// <summary>
		/// Creates a new tile cutter that cuts tiles with the specified width and height.
		/// </summary>
		/// <param name="tileWidth">The width of the produced tiles in pixels.</param>
		/// <param name="tileHeight">The height of the produced tiles in pixels.</param>
		/// <param name="emptyColor">The empty color to use for out-of-bounds pixels.</param>
		public TileCutter(in int tileWidth, in int tileHeight, in Color emptyColor) {
			ArgumentOutOfRangeException.ThrowIfLessThan(tileWidth, 1);
			ArgumentOutOfRangeException.ThrowIfLessThan(tileHeight, 1);

			TileWidth = tileWidth;
			TileHeight = tileHeight;
			EmptyColor = emptyColor;
		}

		/// <summary>
		/// Cuts out all tiles from the specified bitmap.
		/// </summary>
		/// <param name="bmp">The bitmap.</param>
		/// <returns>The tiles.</returns>
		public IEnumerable<Tile> CutTiles(Bitmap bmp) {
			ArgumentNullException.ThrowIfNull(bmp);

			int hTileCount = (bmp.Width + TileWidth - 1) / TileWidth;
			int vTileCount = (bmp.Height + TileHeight - 1) / TileHeight;

			return CutTiles(bmp, 0, 0, hTileCount, vTileCount);
		}

		/// <summary>
		/// Cuts out tiles from the specified bitmap in the specified range.
		/// </summary>
		/// <param name="bmp">The bitmap.</param>
		/// <param name="tx">The column of the first tile.</param>
		/// <param name="ty">The row of the first tile.</param>
		/// <param name="tw">The amount of horizontal tiles to cut.</param>
		/// <param name="th">The amount of vertical tiles to cut.</param>
		/// <returns>The tiles.</returns>
		public IEnumerable<Tile> CutTiles(Bitmap bmp, int tx, int ty, int tw, int th) {
			for (int tj = 0; tj < th; tj++) {
				for (int ti = 0; ti < tw; ti++) {
					yield return CutTile(bmp, tx + ti, ty + tj);
				}
			}
		}

		/// <summary>
		/// Cuts out one tile from the specified bitmap in the specified range.
		/// </summary>
		/// <param name="bmp">The bitmap.</param>
		/// <param name="tx">The column of the tile.</param>
		/// <param name="ty">The row of the tile.</param>
		/// <returns>The tile.</returns>
		public Tile CutTile(Bitmap bmp, in int tx, in int ty) {
			ArgumentNullException.ThrowIfNull(bmp);

			Tile tile = new(TileWidth, TileHeight, tx * TileWidth, ty * TileHeight);
			int[] pixels = new int[tile.Count];

			int p = 0;
			for (int y = 0; y < TileHeight; y++) {
				for (int x = 0; x < TileWidth; x++) {
					int px = tx * TileWidth + x;
					int py = ty * TileHeight + y;

					int pixel;
					if (px < 0 || py < 0 || px >= bmp.Width || py >= bmp.Height) {
						pixel = EmptyColor.ToArgb();
					} else {
						pixel = bmp[px, py];
					}

					pixels[p++] = pixel;
				}
			}

			tile.SetAllPixels(pixels);
			return tile;
		}
	}
}
