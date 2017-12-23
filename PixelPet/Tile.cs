using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet {
	public class Tile : IEquatable<Tile> {
		private struct HashCodeTuple {
			public bool IsSet { get; set; }
			public uint Value { get; set; }
		}

		private int[] BitmapBuffer { get; }
		private short[] IndexBuffer { get; set; }
		/// <summary>
		/// Gets the width of the tile in pixels.
		/// </summary>
		public int Width { get; private set; }
		/// <summary>
		/// Gets the height of the tile in pixels.
		/// </summary>
		public int Height { get; private set; }
		/// <summary>
		/// Gets the amount of pixels in the tile.
		/// </summary>
		public int PixelCount {
			get {
				return this.Width * this.Height;
			}
		}

		/// <summary>
		/// Gets the palette for this tile, if it has been indexed.
		/// </summary>
		public IEnumerable<Color> Palette { get; private set; }
		/// <summary>
		/// Gets a boolean that indicated whether this tile has been indexed.
		/// </summary>
		public bool IsIndexed { get; private set; }
		/// <summary>
		/// Gets a boolean that indicates whether this tile is known to be an empty tile.
		/// </summary>
		public bool IsEmptyTile { get; private set; }

		/// <summary>
		/// Gets the previously computed hash codes.
		/// </summary>
		private HashCodeTuple[] HashCodes { get; }

		/// <summary>
		/// Creates a new tile with the specified width and height.
		/// </summary>
		/// <param name="width">The width of the tile in pixels.</param>
		/// <param name="height">The height of the tile in pixels.</param>
		public Tile(int width, int height) {
			if (width < 1)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height < 1)
				throw new ArgumentOutOfRangeException(nameof(height));

			this.Width = width;
			this.Height = height;

			this.BitmapBuffer = new int[this.PixelCount];
			this.HashCodes = new HashCodeTuple[4];
		}

		/// <summary>
		/// Clears the previously computed hash codes.
		/// </summary>
		private void ClearHashCodes() {
			for (int i = 0; i < this.HashCodes.Length; i++) {
				this.HashCodes[i].IsSet = false;
			}
		}

		/// <summary>
		/// Gets the pixel at the specified (unflipped) position in the tile.
		/// </summary>
		/// <param name="x">The x-component of the position.</param>
		/// <param name="y">The y-component of the position.</param>
		/// <returns>The color value of the pixel.</returns>
		public int GetPixel(int x, int y) {
			int i = y * this.Width + x;
			return this.BitmapBuffer[i];
		}
		/// <summary>
		/// Sets the pixel at the specified (unflipped) position in the tile.
		/// </summary>
		/// <param name="x">The x-component of the position.</param>
		/// <param name="y">The y-component of the position.</param>
		/// <param name="pixel">The new color value of the pixel.</param>
		public void SetPixel(int x, int y, int pixel) {
			int i = y * this.Width + x;
			this.BitmapBuffer[i] = pixel;
			this.ClearHashCodes();
		}
		/// <summary>
		/// Sets all pixels in the (unflipped) tile.
		/// </summary>
		/// <param name="pixels">The pixels to set, which must be the same amount as the number of pixels in the tile.</param>
		public void SetAllPixels(IList<int> pixels) {
			if (pixels == null)
				throw new ArgumentNullException(nameof(pixels));
			if (pixels.Count != this.PixelCount)
				throw new ArgumentException("Amount of new pixels must equal the amount of pixels in the tile.", nameof(pixels));

			pixels.CopyTo(this.BitmapBuffer, 0);
			this.ClearHashCodes();
		}

		/// <summary>
		/// Gets the hash code of the unflipped tile.
		/// </summary>
		/// <returns>The computed hash code.</returns>
		public override int GetHashCode() {
			return this.GetHashCode(false, false);
		}
		/// <summary>
		/// Gets the hash code of the tile with the specified flips applied.
		/// </summary>
		/// <param name="hflip">To flip horizontally.</param>
		/// <param name="vflip">To flip vertically.</param>
		/// <returns>The computed hash code.</returns>
		public int GetHashCode(bool hflip, bool vflip) {
			// Use previously computed hash, if available.
			int i = (hflip ? 1 : 0) + (vflip ? 2 : 0);
			if (this.HashCodes[i].IsSet) {
				return (int)this.HashCodes[i].Value;
			}

			// Calculate FNV-1a hash.
			uint hash = 0x811C9DC5;
			foreach (int pixel in this.EnumerateTile(hflip, vflip)) {
				foreach (byte b in BitConverter.GetBytes(pixel)) {
					hash = (hash ^ b) * 0x1000193;
				}
			}
			this.HashCodes[i].Value = hash;
			this.HashCodes[i].IsSet = true;

			return (int)hash;
		}

		/// <summary>
		/// Enumerates the unflipped tile.
		/// </summary>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTile() {
			return this.EnumerateTile(false, false);
		}
		/// <summary>
		/// Enumerates the tile with the specified flips applied.
		/// </summary>
		/// <param name="hflip">To flip horizontally.</param>
		/// <param name="vflip">To flip vertically.</param>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTile(bool hflip, bool vflip) {
			int x, xStart, xEnd, xStep;
			if (hflip) {
				xStart = this.Width - 1;
				xEnd = -1;
				xStep = -1;
			} else {
				xStart = 0;
				xEnd = this.Width;
				xStep = 1;
			}

			int y, yStart, yEnd, yStep;
			if (vflip) {
				yStart = this.Height - 1;
				yEnd = -1;
				yStep = -1;
			} else {
				yStart = 0;
				yEnd = this.Height;
				yStep = 1;
			}

			y = yStart;
			while (y != yEnd) {
				x = xStart;
				while (x != xEnd) {
					yield return this.GetPixel(x, y);
					x += xStep;
				}
				y += yStep;
			}
		}
		/// <summary>
		/// Enumerates the palette indices for the unflipped tile, if it has been indexed.
		/// </summary>
		/// <returns>The enumerated tile.</returns>
		public IEnumerable<int> EnumerateTileIndexed() {
			foreach (int idx in this.IndexBuffer) {
				if (!this.IsIndexed) {
					throw new InvalidOperationException("The tile is not indexed.");
				}

				yield return idx;
			}
		}

		/// <summary>
		/// Indexes this tile with the given palette.
		/// </summary>
		/// <param name="palette">The palette to use.</param>
		/// <param name="palStart">The offset in the palette at which to start checking.</param>
		/// <param name="palSize">The number of colors from the start of the palette to check.</param>
		/// <returns>true if the palette was successfully indexed; otherwise, falce.</returns>
		public bool IndexTile(IList<Color> palette, int palStart, int palSize) {
			short[] indexBuffer = new short[this.PixelCount];

			bool nonzero = false;

			for (int i = 0; i < this.PixelCount; i++) {
				// Try to match this pixel to the palette.
				int argb = this.BitmapBuffer[i];
				int idx = Tile.FindPaletteIndex(argb, palette, palStart, palSize);

				if (idx < 0) {
					// Tile does not match this palette.
					return false;
				}

				if (idx != 0) {
					nonzero = true;
				}

				indexBuffer[i] = (short)idx;
			}

			this.Palette = palette.Skip(palStart).Take(palSize);
			this.IndexBuffer = indexBuffer;
			this.IsIndexed = true;
			this.IsEmptyTile = !nonzero;
			return true;
		}
		/// <summary>
		/// Indexes this tile with its own palette.
		/// </summary>
		public void IndexTile() {
			IList<Color> palette = this.MakePalette();
			this.IndexTile(palette, 0, palette.Count);
		}
		/// <summary>
		/// Indexes this tile with any of the given palettes.
		/// </summary>
		/// <param name="palettes">The palettes to use.</param>
		/// <param name="palSize">The number of colors per palette.</param>
		/// <param name="palInitial">The index of the preferred palette, which will be matched first.</param>
		/// <returns>The index of the first matched palette, or -1 if the tile could not be indexed with any palette.</returns>
		public int IndexTileAny(IList<Color> palettes, int palSize, int palInitial) {
			int palCount = (palettes.Count + palSize - 1) / palSize;

			for (int i = 0; i < palCount; i++) {
				int curPal = (palInitial + i) % palCount;

				// Try to index this tile with the palette.
				if (this.IndexTile(palettes, curPal * palSize, palSize)) {
					if (this.IsEmptyTile) {
						return 0;
					} else {
						return i;
					}
				}
			}

			return -1;
		}
		/// <summary>
		/// Checks if this tile matches the specified palette.
		/// </summary>
		/// <param name="palette">The palette to match.</param>
		/// <param name="palStart">The offset in the palette at which to start checking.</param>
		/// <param name="palSize">The number of colors from the start of the palette to check.</param>
		/// <returns>true if this tile matches the palette; otherwise, false.</returns>
		public bool MatchesPalette(IList<Color> palette, int palStart, int palSize) {
			// Check if all own colors are present in the specified palette.
			return !this.MakePalette().Except(palette.Skip(palStart).Take(palSize)).Any();
		}
		/// <summary>
		/// Generates a palette for this tile containing all its unique colors.
		/// </summary>
		/// <returns>The generated palette.</returns>
		protected IList<Color> MakePalette() {
			return this.BitmapBuffer.Distinct().Select(argb => Color.FromArgb(argb)).ToList();
		}
		/// <summary>
		/// Finds the palette index for the specified color, using the specified palette.
		/// </summary>
		/// <param name="argb">The color to find.</param>
		/// <param name="palette">The palette to use.</param>
		/// <param name="palStart">The offset in the palette at which to start checking.</param>
		/// <param name="palSize">The number of colors from the start of the palette to check.</param>
		/// <returns>The pixel's palette index, or -1 if the color is not in the palette.</returns>
		protected static int FindPaletteIndex(int argb, IList<Color> palette, int palStart, int palSize) {
			// Set to totally transparent color, if it exists.
			int a = argb >> 24;
			if (a == 0) {
				return 0;
			}

			// Find matching color.
			for (int i = 0; i < palSize && (palStart + i) < palette.Count; i++) {
				if (palette[palStart + i].ToArgb() == argb) {
					return i;
				}
			}

			// No matching color found.
			return -1;
		}

		public override bool Equals(object obj) {
			return this.Equals(obj, false, false);
		}
		public bool Equals(object obj, bool hflip, bool vflip) {
			return obj is Tile other
				&& this.Equals(other, hflip, vflip);
		}
		public bool Equals(Tile other) {
			return this.Equals(other, false, false);
		}
		public bool Equals(Tile other, bool hflip, bool vflip) {
			if (other == null) {
				return false;
			}
			return ReferenceEquals(this, other) || (
				this.Width == other.Width &&
				this.Height == other.Height &&
				this.GetHashCode(hflip, vflip) == other.GetHashCode() &&
				this.EnumerateTile(hflip, vflip).SequenceEqual(other.EnumerateTile())
			);
		}
		public static bool operator ==(Tile a, Tile b) {
			return a?.Equals(b) ?? b?.Equals(a) ?? true;
		}
		public static bool operator !=(Tile a, Tile b) {
			return !a?.Equals(b) ?? b?.Equals(a) ?? true;
		}
	}
}
