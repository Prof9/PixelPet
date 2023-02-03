using System;
using System.Collections.Generic;

namespace LibPixelPet {
	public class Bitmap {
		/// <summary>
		/// Gets the pixels in the bitmap.
		/// </summary>
		public IList<int> Pixels { get; private set; }
		/// <summary>
		/// Gets the width of the bitmap.
		/// </summary>
		public int Width { get; private set; }
		/// <summary>
		///  Gets the height of the bitmap.
		/// </summary>
		public int Height { get; private set; }
		/// <summary>
		/// Gets the number of pixels in the bitmap.
		/// </summary>
		public int PixelCount => Pixels.Count;

		/// <summary>
		/// Get or set a pixel by index.
		/// </summary>
		/// <param name="i">Pixel index.</param>
		/// <returns>Pixel at specified index.</returns>
		public int this[int i] {
			get {
				return Pixels[i];
			}
			set {
				Pixels[i] = value;
			}
		}
		/// <summary>
		/// Gets or sets a pixel by x and y-coordinates.
		/// </summary>
		/// <param name="x">Pixel x-coordinate.</param>
		/// <param name="y">Pixel y-coordinate.</param>
		/// <returns>Pixel at specified coordinates.</returns>
		public int this[int x, int y] {
			get {
				return Pixels[y * Width + x];
			}
			set {
				Pixels[y * Width + x] = value;
			}
		}

		/// <summary>
		/// Creates an empty bitmap with the given width and height.
		/// All pixels are initialized to zero.
		/// </summary>
		/// <param name="width">Bitwap width in number of pixels.</param>
		/// <param name="height">Bitmap height in number of pixels.</param>
		public Bitmap(int width, int height) {
			Pixels = new int[(uint)width * (uint)height];
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Creates a bitmap with the given pixels, width and height.
		/// </summary>
		/// <param name="pixels"></param>
		/// <param name="width">Bitwap width in number of pixels.</param>
		/// <param name="height">Bitmap height in number of pixels.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the width or height is negative.</exception>
		/// <exception cref="ArgumentException">If the number of pixels does not match the width and height.</exception>
		public Bitmap(int[] pixels, int width, int height) {
			if (pixels is null)
				throw new ArgumentNullException(nameof(pixels));
			if (width < 0)
				throw new ArgumentOutOfRangeException(nameof(width), width, "The width cannot be negative.");
			if (height < 0)
				throw new ArgumentOutOfRangeException(nameof(height), height, "The height cannot be negative.");
			if ((uint)width * (uint)height != pixels.Length)
				throw new ArgumentException("The specified width and height do not match the number of pixels.", nameof(pixels));

			Pixels = pixels;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Creates a copy of this bitmap cropped to the given window.
		/// </summary>
		/// <param name="x">X-coordinate of top left pixel of crop window.</param>
		/// <param name="y">Y-coordinate of top left pixel of crop window.</param>
		/// <param name="width">Width of crop window.</param>
		/// <param name="height">Height of crop window.</param>
		/// <returns></returns>
		public Bitmap GetCroppedBitmap(int x, int y, int width, int height) {
			// Range checks to make sure at least a 1x1 bitmap is cropped.
			if (x < 0) {
				x = 0;
			} else if (x >= Width) {
				x = Width - 1;
			}
			if (y < 0) {
				y = 0;
			} else if (y >= Height) {
				y = Height - 1;
			}
			if (width < 0) {
				width = Width - x;
			}
			if (height < 0) {
				height = Height - y;
			}
			if (width < 1) {
				width = 1;
			} else if (x + width > Width) {
				width = Width - x;
			}
			if (height < 1) {
				height = 1;
			} else if (y + height > Height) {
				height = Height - y;
			}

			Bitmap bmp = new(width, height);
			
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					bmp[i, j] = this[x + i, y + j];
				}
			}

			return bmp;
		}
	}
}
