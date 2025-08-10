using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LibPixelPet {
	public class PaletteSet : IEnumerable<PaletteEntry> {
		private List<PaletteEntry> PaletteEntries { get; }

		/// <summary>
		/// Gets the amount of palettes in this palette set.
		/// </summary>
		public int Count => PaletteEntries.Count;

		public PaletteSet() {
			PaletteEntries = new();
		}

		public PaletteEntry this[int index] {
			get {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return PaletteEntries[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				if (!value.IsValid())
					throw new ArgumentException("The palette entry is invalid.", nameof(value));
				if (ContainsPalette(value.Number))
					throw new ArgumentException("A palette with this palette number already exists in the palette set.", nameof(value));

				PaletteEntries[index] = value;
			}
		}

		/// <summary>
		/// Adds a palette to the palette set. The palette number is chosen to be the current highest palette number in the palette set incremented by one.
		/// </summary>
		/// <param name="palette"></param>
		public void Add(Palette palette)
			=> Add(palette, this
				.PaletteEntries.Select(p => p.Number)
				.DefaultIfEmpty(-1)
				.Max() + 1
			);
		/// <summary>
		/// Adds a palette to the palette set with the specified palette number.
		/// </summary>
		/// <param name="palette">The palette to add.</param>
		/// <param name="number">The palette number to use.</param>
		public void Add(Palette palette, in int number) {
			ArgumentNullException.ThrowIfNull(palette);
			ArgumentOutOfRangeException.ThrowIfNegative(number);
			if (ContainsPalette(number))
				throw new ArgumentException("A palette with this palette number already exists in the palette set.", nameof(number));

			PaletteEntries.Add(new PaletteEntry(number, palette));
			PaletteEntries.Sort((a, b) => a.Number.CompareTo(b.Number));
		}

		/// <summary>
		/// Clears the palette set.
		/// </summary>
		public void Clear() {
			PaletteEntries.Clear();
		}

		/// <summary>
		/// Removes the palette entry at the specified index in the palette set.
		/// </summary>
		/// <param name="index">The index of the palette entry to remove.</param>
		public void RemoveAt(in int index) {
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			PaletteEntries.RemoveAt(index);
		}

		/// <summary>
		/// Removes a palette with the specified palette number from the palette set.
		/// </summary>
		/// <param name="number">The number of the palette to remove.</param>
		/// <returns>true if a palette was removed; otherwise, false.</returns>
		public bool RemovePalette(in int number) {
			int index = IndexOfPalette(number);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Gets the palette associated with the specified palette number.
		/// </summary>
		/// <param name="number">The number of the palette to find.</param>
		/// <returns>The palette, or null if no palettet was found.</returns>
		public Palette? FindPalette(in int number) {
			TryFindPalette(number, out Palette? palette);
			return palette;
		}
		/// <summary>
		/// Checks if the palette set contains a palette with the specified palette number.
		/// </summary>
		/// <param name="number">The number of the palette to find.</param>
		/// <returns>true if a palette was found; otherwise, false.</returns>
		public bool ContainsPalette(in int number)
			=> TryFindPalette(number, out _);
		/// <summary>
		/// Gets the palette associated with the specified palette number.
		/// </summary>
		/// <param name="number">The number of the palette to find.</param>
		/// <param name="palette">The palette, if it was found.</param>
		/// <returns>true if a palette was found; otherwise, false.</returns>
		public bool TryFindPalette(in int number, [MaybeNullWhen(false)] out Palette palette) {
			int i = IndexOfPalette(number);
			if (i >= 0) {
				palette = PaletteEntries[i].Palette;
				return true;
			} else {
				palette = null;
				return false;
			}
		}
		/// <summary>
		/// Finds the index of the palette entry with the specified palette number.
		/// </summary>
		/// <param name="number">The number of the palette to find.</param>
		/// <returns>The index of the palette entry, or -1 if it was not found.</returns>
		public int IndexOfPalette(in int number) {
			for (int i = 0; i < Count; i++) {
				if (this[i].Number == number) {
					return i;
				}
			}
			return -1;
		}

		public IEnumerator<PaletteEntry> GetEnumerator()
			=> PaletteEntries.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> PaletteEntries.GetEnumerator();
	}
}
