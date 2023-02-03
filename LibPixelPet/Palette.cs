using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace LibPixelPet {
	public class Palette : IEnumerable<int>, ICloneable {
		/// <summary>
		/// Gets the color values currently in the palette.
		/// </summary>
		private List<int> Colors { get; }

		/// <summary>
		/// Gets a dictionary mapping colors to indices. This is used for faster lookup.
		/// </summary>
		private Dictionary<int, int> ColorIndexMap { get; }

		/// <summary>
		/// Gets the number of colors currently in the palette.
		/// </summary>
		public int Count => Colors.Count;
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
			if (maxSize < -1)
				throw new ArgumentOutOfRangeException(nameof(maxSize));

			// Default capacity 16, but no greater than 256.
			int initialCapacity = maxSize;
			if (initialCapacity < 0) {
				initialCapacity = 16;
			}
			if (initialCapacity > 256) {
				initialCapacity = 256;
			}

			Colors = new List<int>(initialCapacity);
			ColorIndexMap = new Dictionary<int, int>(initialCapacity);
			Format = format;
			MaximumSize = maxSize;
		}

		public int this[int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return Colors[index];
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				// Remove old mapping, add new one.
				ColorIndexMap.Remove(Colors[index]);
				if (!ColorIndexMap.ContainsKey(value)) {
					ColorIndexMap[value] = index;
				}

				Colors[index] = value;
			}
		}

		/// <summary>
		/// Clears the palette.
		/// </summary>
		public void Clear() {
			Colors.Clear();
			ColorIndexMap.Clear();
		}

		/// <summary>
		/// Adds the specified color to the palette.
		/// </summary>
		/// <param name="color">The color value to add.</param>
		public void Add(int color) {
			if (MaximumSize >= 0 && Colors.Count >= MaximumSize)
				throw new InvalidOperationException("The maximum palette size has been reached.");

			Colors.Add(color);
			if (!ColorIndexMap.ContainsKey(color)) {
				ColorIndexMap[color] = Colors.Count - 1;
			}
		}

		/// <summary>
		/// Adds the specified color to the palette, converted from the specified color format.
		/// </summary>
		/// <param name="color">The color value to add.</param>
		/// <param name="format">The color format that the color value is currently in.</param>
		public void Add(int color, in ColorFormat format)
			=> Add(Format.Convert(color, format));

		/// <summary>
		/// Finds the index of the specified color in this palette.
		/// </summary>
		/// <param name="color">The color value to find.</param>
		/// <param name="from">The color format that the color value is currently in.</param>
		/// <returns>The index of the color, or -1 if it was not found.</returns>
		public int IndexOfColor(in int color, in ColorFormat from) {
			return IndexOfColor(Format.Convert(color, from));
		}

		/// <summary>
		/// Finds the index of the specified color in this palette.
		/// </summary>
		/// <param name="color">The color value to find.</param>
		/// <returns>The index of the color, or -1 if it was not found.</returns>
		public int IndexOfColor(in int color) {
			if (ColorIndexMap.TryGetValue(color, out int index)) {
				return index;
			} else {
				return -1;
			}
		}

		public IEnumerator<int> GetEnumerator() 
			=> ((IEnumerable<int>)Colors).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() 
			=> ((IEnumerable<int>)Colors).GetEnumerator();

		public Palette Clone() {
			Palette clone = new(Format, MaximumSize);

			foreach (int c in this) {
				clone.Add(c);
			}

			return clone;
		}
		object ICloneable.Clone() => Clone();
	}
}
