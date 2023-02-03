using System;
using System.Collections.Generic;
using System.Linq;

namespace LibPixelPet {
	public class Tile : ICloneable {
		private struct HashCodeTuple {
			public bool IsSet { get; set; }
			public int Value { get; set; }
		}

		/// <summary>
		/// Gets the pixels in the tile.
		/// </summary>
		private int[] Pixels { get; }

		/// <summary>
		/// Gets or sets the palette number that this tile uses.
		/// </summary>
		public int PaletteNumber { get; set; }

		/// <summary>
		/// Gets the width of the tile in pixels.
		/// </summary>
		public int Width { get; }
		/// <summary>
		/// Gets the height of the tile in pixels.
		/// </summary>
		public int Height { get; }
		/// <summary>
		/// Gets the amount of pixels in the tile.
		/// </summary>
		public int Count => this.Width * this.Height;

		/// <summary>
		/// Gets the x-position of the origin of the tile.
		/// </summary>
		public int OriginX { get; }
		/// <summary>
		/// Gets the y-position of the origin of the tile.
		/// </summary>
		public int OriginY { get; }

		/// <summary>
		/// Gets the previously computed hash codes.
		/// </summary>
		private readonly HashCodeTuple[] hashCodes;
		private bool hashCodesCleared;

		/// <summary>
		/// Creates a new tile with the specified width and height.
		/// </summary>
		/// <param name="width">The width of the tile in pixels.</param>
		/// <param name="height">The height of the tile in pixels.</param>
		public Tile(in int width, in int height)
			: this(width, height, 0, 0) { }

		/// <summary>
		/// Creates a new tile with the specified width and height and origin position.
		/// </summary>
		/// <param name="width">The width of the tile in pixels.</param>
		/// <param name="height">The height of the tile in pixels.</param>
		/// <param name="x">The x-position of the origin of the tile.</param>
		/// <param name="y">The y-position of the origin of the tile.</param>
		public Tile(in int width, in int height, in int x, in int y) {
			if (width < 1)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height < 1)
				throw new ArgumentOutOfRangeException(nameof(height));

			this.PaletteNumber = 0;

			this.Width   = width;
			this.Height  = height;
			this.OriginX = x;
			this.OriginY = y;
			this.Pixels  = new int[width * height];

			this.hashCodes = new HashCodeTuple[1 << 2];
			this.hashCodesCleared = true;
		}

		/// <summary>
		/// Clears the previously computed hash codes.
		/// </summary>
		private void ClearHashCodes() {
			for (int i = 0; i < hashCodes.Length; i++) {
				this.hashCodes[i].IsSet = false;
			}
			this.hashCodesCleared = true;
		}

		/// <summary>
		/// Gets or sets the pixel at the specified (unflipped) position in the tile.
		/// </summary>
		/// <param name="x">The x-component of the position.</param>
		/// <param name="y">The y-component of the position.</param>
		/// <returns>The color value of the pixel.</returns>
		public int this[in int x, in int y] {
			get {
				if (x < 0 || x >= this.Width)
					throw new ArgumentOutOfRangeException(nameof(x));
				if (y < 0 || y >= this.Height)
					throw new ArgumentOutOfRangeException(nameof(y));

				return this.Pixels[y * this.Width + x];
			}
			set {
				if (x < 0 || x >= this.Width)
					throw new ArgumentOutOfRangeException(nameof(x));
				if (y < 0 || y >= this.Height)
					throw new ArgumentOutOfRangeException(nameof(y));

				this.Pixels[y * this.Width + x] = value;

				if (!this.hashCodesCleared) {
					this.ClearHashCodes();
				}
			}
		}

		/// <summary>
		/// Sets all pixels in the tile.
		/// This is faster than setting them one by one.
		/// </summary>
		/// <param name="pixels">The new pixels. The length of the list must match the number of pixels exactly.</param>
		public void SetAllPixels(in IList<int> pixels) {
			if (pixels is null)
				throw new ArgumentNullException(nameof(pixels));
			if (pixels.Count != this.Count)
				throw new ArgumentException("Amount of new pixels must equal the amount of pixels in the tile.", nameof(pixels));

			pixels.CopyTo(this.Pixels, 0);
			this.ClearHashCodes();
		}

		/// <summary>
		/// Gets the hash code of the unflipped tile.
		/// </summary>
		/// <returns>The computed hash code.</returns>
		public override int GetHashCode()
			=> this.GetHashCode(false, false);
		/// <summary>
		/// Gets the hash code of the tile with the specified flips applied.
		/// </summary>
		/// <param name="hFlip">To flip horizontally.</param>
		/// <param name="vFlip">To flip vertically.</param>
		/// <returns>The computed hash code.</returns>
		public int GetHashCode(in bool hFlip, in bool vFlip) {
			int i = (hFlip ? 1 : 0) + (vFlip ? 2 : 0);

			// Use previously computed hash, if available.
			if (this.hashCodes[i].IsSet) {
				return (int)this.hashCodes[i].Value;
			}

			// Calculate hash.
			int hash = -490236692;
			unchecked {
				foreach (int px in (hFlip || vFlip ? this.EnumerateTile(hFlip, vFlip) : this.Pixels)) {
					hash = hash * -1521134295 + px;
				}
			}
			this.hashCodes[i].Value = hash;
			this.hashCodes[i].IsSet = true;

			this.hashCodesCleared = false;

			return (int)hash;
		}

		/// <summary>
		/// Enumerates the unflipped tile.
		/// </summary>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTile()
			=> this.EnumerateTile(false, false);
		/// <summary>
		/// Enumerates the tile with the specified flips applied.
		/// </summary>
		/// <param name="hflip">To flip horizontally.</param>
		/// <param name="vflip">To flip vertically.</param>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTile(bool hFlip, bool vFlip) {
			int x, y;
			for (
				y = (vFlip ? this.Height - 1 : 0);
				y != (vFlip ? -1 : this.Height);
				y += (vFlip ? -1 : 1)
			) {
				for (
					x = (hFlip ? this.Width - 1 : 0);
					x != (hFlip ? -1 : this.Width);
					x += (hFlip ? -1 : 1)
				) {
					yield return this[x, y];
				}
			}
		}

		/// <summary>
		/// Generates indexed versions of this tile for each of the palettes in the specified palette set.
		/// </summary>
		/// <param name="palettes">The palette set to use.</param>
		/// <param name="indexedTiles">The list where the resulting indexed tiles will be written to.</param>
		/// <returns>The number of successfully indexed tiles.</returns>
		public int GenerateIndexedTiles(PaletteSet palettes, IList<Tile> indexedTiles) {
			if (palettes is null)
				throw new ArgumentNullException(nameof(palettes));
			if (indexedTiles.Count < palettes.Count)
				throw new ArgumentException("Indexed tiles list is not large enough to hold all possible indexed tiles", nameof(indexedTiles));

			int count = 0;
			int maxFailPixel = -1;
			Tile indexedTile = indexedTiles[0];
			foreach (PaletteEntry pe in palettes) {
				if (indexedTile is null) {
					indexedTile = new Tile(this.Width, this.Height, this.OriginX, this.OriginY);
				}
				int failPixel = this.TryIndexTile(pe.Palette, ref indexedTile);
				if (failPixel < 0) {
					indexedTile.PaletteNumber = pe.Number;
					indexedTiles[count] = indexedTile;

					// Move to next tile
					if (++count >= palettes.Count) {
						break;
					}
					indexedTile = indexedTiles[count];
				} else if (failPixel > maxFailPixel) {
					maxFailPixel = failPixel;
				}
			}

			if (count == 0) {
				throw new InvalidOperationException("Could not index tile at " +
					"(" + this.OriginX + ", " + this.OriginY + ")" +
					"; color 0x" + this.Pixels[maxFailPixel].ToString("X") + " at " +
					"(" + (this.OriginX + maxFailPixel % this.Width) + ", " + (this.OriginY + maxFailPixel / this.Width) + ")" +
					" not found in any palette.");
			}

			return count;
		}

		private int TryIndexTile(Palette pal, ref Tile indexedTile) {
			for (int i = 0; i < this.Count; i++) {
				int c = pal.IndexOfColor(this.Pixels[i]);
				if (c < 0) {
					return i;
				}
				indexedTile.Pixels[i] = c;
			}
			indexedTile.ClearHashCodes();
			return -1;
		}

		public override bool Equals(object obj)
			=> obj is Tile other
			&& this.Equals(other, false, false);
		public bool Equals(in Tile other)
			=> this.Equals(other, false, false);
		public bool Equals(in Tile other, in bool hFlip, in bool vFlip)
			=> ReferenceEquals(this, other) || (
				// Do not include origin in equality check.
				this.Width == other.Width &&
				this.Height == other.Height &&
				this.GetHashCode(hFlip, vFlip) == other.GetHashCode() &&
				this.EnumerateTile(hFlip, vFlip).SequenceEqual(other.Pixels)
			);

		public Tile Clone() {
			Tile clone = new Tile(this.Width, this.Height, this.OriginX, this.OriginY);
			clone.SetAllPixels(this.Pixels);
			clone.PaletteNumber = this.PaletteNumber;
			return clone;
		}
		object ICloneable.Clone() => this.Clone();
	}
}
