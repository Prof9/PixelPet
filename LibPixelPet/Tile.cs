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
		public int Count => Width * Height;

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
			ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
			ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

			PaletteNumber = 0;

			Width   = width;
			Height  = height;
			OriginX = x;
			OriginY = y;
			Pixels  = new int[width * height];

			hashCodes = new HashCodeTuple[1 << 2];
			hashCodesCleared = true;
		}

		/// <summary>
		/// Clears the previously computed hash codes.
		/// </summary>
		private void ClearHashCodes() {
			for (int i = 0; i < hashCodes.Length; i++) {
				hashCodes[i].IsSet = false;
			}
			hashCodesCleared = true;
		}

		/// <summary>
		/// Gets or sets the pixel at the specified (unflipped) position in the tile.
		/// </summary>
		/// <param name="x">The x-component of the position.</param>
		/// <param name="y">The y-component of the position.</param>
		/// <returns>The color value of the pixel.</returns>
		public int this[in int x, in int y] {
			get {
				if (x < 0 || x >= Width)
					throw new ArgumentOutOfRangeException(nameof(x));
				if (y < 0 || y >= Height)
					throw new ArgumentOutOfRangeException(nameof(y));

				return Pixels[y * Width + x];
			}
			set {
				if (x < 0 || x >= Width)
					throw new ArgumentOutOfRangeException(nameof(x));
				if (y < 0 || y >= Height)
					throw new ArgumentOutOfRangeException(nameof(y));

				Pixels[y * Width + x] = value;

				if (!hashCodesCleared) {
					ClearHashCodes();
				}
			}
		}

		/// <summary>
		/// Sets all pixels in the tile.
		/// This is faster than setting them one by one.
		/// </summary>
		/// <param name="pixels">The new pixels. The length of the list must match the number of pixels exactly.</param>
		public void SetAllPixels(in IList<int> pixels) {
			ArgumentNullException.ThrowIfNull(pixels);
			if (pixels.Count != Count)
				throw new ArgumentException("Amount of new pixels must equal the amount of pixels in the tile.", nameof(pixels));

			pixels.CopyTo(Pixels, 0);
			ClearHashCodes();
		}

		/// <summary>
		/// Gets the hash code of the unflipped tile.
		/// </summary>
		/// <returns>The computed hash code.</returns>
		public override int GetHashCode()
			=> GetHashCode(false, false);
		/// <summary>
		/// Gets the hash code of the tile with the specified flips applied.
		/// </summary>
		/// <param name="hFlip">To flip horizontally.</param>
		/// <param name="vFlip">To flip vertically.</param>
		/// <returns>The computed hash code.</returns>
		public int GetHashCode(in bool hFlip, in bool vFlip) {
			int i = (hFlip ? 1 : 0) + (vFlip ? 2 : 0);

			// Use previously computed hash, if available.
			if (hashCodes[i].IsSet) {
				return (int)hashCodes[i].Value;
			}

			// Calculate hash.
			int hash = -490236692;
			unchecked {
				foreach (int px in (hFlip || vFlip ? EnumerateTile(hFlip, vFlip) : Pixels)) {
					hash = hash * -1521134295 + px;
				}
			}
			hashCodes[i].Value = hash;
			hashCodes[i].IsSet = true;

			hashCodesCleared = false;

			return (int)hash;
		}

		/// <summary>
		/// Enumerates the unflipped tile.
		/// </summary>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTile()
			=> EnumerateTile(false, false);
		/// <summary>
		/// Enumerates the tile with the specified flips applied.
		/// </summary>
		/// <param name="hflip">To flip horizontally.</param>
		/// <param name="vflip">To flip vertically.</param>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTile(bool hFlip, bool vFlip) {
			int x, y;
			for (
				y = (vFlip ? Height - 1 : 0);
				y != (vFlip ? -1 : Height);
				y += (vFlip ? -1 : 1)
			) {
				for (
					x = (hFlip ? Width - 1 : 0);
					x != (hFlip ? -1 : Width);
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
			ArgumentNullException.ThrowIfNull(palettes);
			ArgumentNullException.ThrowIfNull(indexedTiles);
			if (indexedTiles.Count < palettes.Count)
				throw new ArgumentException("Indexed tiles list is not large enough to hold all possible indexed tiles", nameof(indexedTiles));

			int count = 0;
			int maxFailPixel = -1;
			Tile indexedTile = indexedTiles[0];
			foreach (PaletteEntry pe in palettes) {
				indexedTile ??= new Tile(Width, Height, OriginX, OriginY);
				int failPixel = TryIndexTile(pe.Palette, ref indexedTile);
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
				int c  = Pixels[maxFailPixel];
				int px = OriginX + maxFailPixel % Width;
				int py = OriginY + maxFailPixel / Width;
				throw new InvalidOperationException($"Could not index tile at ({OriginX}, {OriginY}); color 0x{c:X} at ({px}, {py}) not found in any palette.");
			}

			return count;
		}

		private int TryIndexTile(Palette pal, ref Tile indexedTile) {
			for (int i = 0; i < Count; i++) {
				int c = pal.IndexOfColor(Pixels[i]);
				if (c < 0) {
					return i;
				}
				indexedTile.Pixels[i] = c;
			}
			indexedTile.ClearHashCodes();
			return -1;
		}

		public override bool Equals(object? obj)
			=> obj is Tile other
			&& Equals(other, false, false);
		public bool Equals(in Tile other)
			=> Equals(other, false, false);
		public bool Equals(in Tile other, in bool hFlip, in bool vFlip)
			=> ReferenceEquals(this, other) || (
				// Do not include origin in equality check.
				other is not null &&
				Width == other.Width &&
				Height == other.Height &&
				GetHashCode(hFlip, vFlip) == other.GetHashCode() &&
				EnumerateTile(hFlip, vFlip).SequenceEqual(other.Pixels)
			);

		public Tile Clone() {
			Tile clone = new(Width, Height, OriginX, OriginY);
			clone.SetAllPixels(Pixels);
			clone.PaletteNumber = PaletteNumber;
			return clone;
		}
		object ICloneable.Clone() => Clone();
	}
}
