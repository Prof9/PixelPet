using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

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
			if (tileWidth < 1)
				throw new ArgumentOutOfRangeException(nameof(tileWidth));
			if (tileHeight < 1)
				throw new ArgumentOutOfRangeException(nameof(tileHeight));

			this.TileWidth = tileWidth;
			this.TileHeight = tileHeight;
			this.EmptyColor = emptyColor;
		}

		/// <summary>
		/// Cuts out all tiles from the specified bitmap.
		/// </summary>
		/// <param name="bmp">The bitmap.</param>
		/// <returns>The tiles.</returns>
		public IEnumerable<Tile> CutTiles(Bitmap bmp) {
			if (bmp == null)
				throw new ArgumentNullException(nameof(bmp));

			int hTileCount = (bmp.Width + this.TileWidth - 1) / this.TileWidth;
			int vTileCount = (bmp.Height + this.TileHeight - 1) / this.TileHeight;

			return this.CutTiles(bmp, 0, 0, hTileCount, vTileCount);
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tw")]
		public IEnumerable<Tile> CutTiles(Bitmap bmp, int tx, int ty, int tw, int th) {
			BitmapData bmpData = bmp.LockBits(
				new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly,
				bmp.PixelFormat
			);
			int[] bmpBuffer = new int[(bmpData.Stride * bmp.Height) / 4];
			Marshal.Copy(bmpData.Scan0, bmpBuffer, 0, bmpBuffer.Length);
			bmp.UnlockBits(bmpData);

			for (int tj = 0; tj < th; tj++) {
				for (int ti = 0; ti < tw; ti++) {
					yield return CutTile(bmp, bmpData, bmpBuffer, tx + ti, ty + tj);
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
			return this.CutTiles(bmp, tx, ty, 1, 1).First();
		}

		private Tile CutTile(in Bitmap bmp, in BitmapData bmpData, in int[] bmpBuffer, in int ti, in int tj) {
			Tile tile = new Tile(this.TileWidth, this.TileHeight, ti * this.TileWidth, tj * this.TileHeight);
			int[] pixels = new int[tile.Count];

			int p = 0;
			for (int ty = 0; ty < this.TileHeight; ty++) {
				for (int tx = 0; tx < this.TileWidth; tx++) {
					int px = ti * this.TileWidth + tx;
					int py = tj * this.TileHeight + ty;
					int ptr = (py * bmpData.Stride + px * 4) / 4;

					int pixel;
					if (px < 0 || py < 0 || px >= bmp.Width || py >= bmp.Height) {
						pixel = this.EmptyColor.ToArgb();
					} else {
						pixel = bmpBuffer[ptr];
					}

					pixels[p++] = pixel;
				}
			}

			tile.SetAllPixels(pixels);
			return tile;
		}
	}
}
