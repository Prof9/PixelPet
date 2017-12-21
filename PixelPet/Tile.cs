using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet {
	public class Tile : IEquatable<Tile> {
		private struct HashCodeTuple {
			public bool IsSet { get; set; }
			public uint Value { get; set; }
		}

		private int[] BitmapBuffer { get; }
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
