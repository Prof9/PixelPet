﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace LibPixelPet {
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class Palette : IEnumerable<int> {
		/// <summary>
		/// Gets the color values currently in the palette.
		/// </summary>
		private List<int> Colors { get; }

		/// <summary>
		/// Gets the number of colors currently in the palette.
		/// </summary>
		public int Count => this.Colors.Count;
		/// <summary>
		/// Gets or sets the color format used by the palette.
		/// </summary>
		public ColorFormat Format { get; set; }
		/// <summary>
		/// Gets the maximum size of the palette, or -1 if there is no maximum.
		/// </summary>
		public int MaximumSize { get; }

		/// <summary>
		/// Creates a new palette with the specified format and maximum size.
		/// </summary>
		/// <param name="format">The color format.</param>
		/// <param name="maxSize">The maximum palette size, or -1 if there is no maximum.</param>
		public Palette(in ColorFormat format, in int maxSize) {
			if (format == null)
				throw new ArgumentNullException(nameof(format));
			if (maxSize < -1)
				throw new ArgumentOutOfRangeException(nameof(maxSize));

			this.Colors = new List<int>();
			this.Format = format;
			this.MaximumSize = maxSize;
		}

		public int this[int index] {
			get {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return this.Colors[index];
			}
			set {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				this.Colors[index] = value;
			}
		}

		/// <summary>
		/// Clears the palette.
		/// </summary>
		public void Clear()
			=> this.Colors.Clear();

		/// <summary>
		/// Adds the specified color to the palette.
		/// </summary>
		/// <param name="color">The color value to add.</param>
		public void Add(int color) {
			if (this.Colors.Count >= this.MaximumSize)
				throw new InvalidOperationException("The maximum palette size has been reached.");

			this.Colors.Add(color);
		}

		/// <summary>
		/// Adds the specified color to the palette, converted from the specified color format.
		/// </summary>
		/// <param name="color">The color value to add.</param>
		/// <param name="format">The color format that the color value is currently in.</param>
		public void Add(int color, in ColorFormat format)
			=> this.Add(this.Format.Convert(color, format));

		/// <summary>
		/// Finds the index of the specified color in this palette.
		/// </summary>
		/// <param name="color">The color value to find.</param>
		/// <param name="from">The color format that the color value is currently in.</param>
		/// <returns>The index of the color, or -1 if it was not found.</returns>
		public int IndexOfColor(in int color, in ColorFormat from) {
			if (from == null)
				throw new ArgumentNullException(nameof(from));

			return this.IndexOfColor(this.Format.Convert(color, from));
		}

		/// <summary>
		/// Finds the index of the specified color in this palette.
		/// </summary>
		/// <param name="color">The color value to find.</param>
		/// <returns>The index of the color, or -1 if it was not found.</returns>
		public int IndexOfColor(in int color) {
			for (int i = 0; i < this.Count; i++) {
				if (this[i] == color) {
					return i;
				}
			}
			return -1;
		}

		public IEnumerator<int> GetEnumerator() 
			=> ((IEnumerable<int>)this.Colors).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() 
			=> ((IEnumerable<int>)this.Colors).GetEnumerator();
	}
}
