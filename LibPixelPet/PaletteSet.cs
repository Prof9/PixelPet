using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LibPixelPet {
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class PaletteSet : IEnumerable<PaletteEntry> {
		private List<PaletteEntry> PaletteEntries { get; }

		/// <summary>
		/// Gets the amount of palettes in this palette set.
		/// </summary>
		public int Count => this.PaletteEntries.Count;

		public PaletteSet() {
			this.PaletteEntries = new List<PaletteEntry>();
		}

		public PaletteEntry this[int index] {
			get {
				if (index < 0 || index > this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return this.PaletteEntries[index];
			}
			set {
				if (index < 0 || index > this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				if (!value.IsValid())
					throw new ArgumentException("The palette entry is invalid.", nameof(value));
				if (this.ContainsPalette(value.Number))
					throw new ArgumentException("A palette with this palette number already exists in the palette set.", nameof(value));

				this.PaletteEntries[index] = value;
			}
		}

		/// <summary>
		/// Adds a palette to the palette set. The palette number is chosen to be the current highest palette number in the palette set incremented by one.
		/// </summary>
		/// <param name="palette"></param>
		public void Add(Palette palette)
			=> this.Add(palette, this
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
			if (palette == null)
				throw new ArgumentNullException(nameof(palette));
			if (number < 0)
				throw new ArgumentOutOfRangeException(nameof(number));
			if (this.ContainsPalette(number))
				throw new ArgumentException("A palette with this palette number already exists in the palette set.", nameof(number));

			this.PaletteEntries.Add(new PaletteEntry(number, palette));
			this.PaletteEntries.OrderBy(pe => pe.Number);
		}

		/// <summary>
		/// Clears the palette set.
		/// </summary>
		public void Clear() {
			this.PaletteEntries.Clear();
		}

		/// <summary>
		/// Removes the palette entry at the specified index in the palette set.
		/// </summary>
		/// <param name="index">The index of the palette entry to remove.</param>
		public void RemoveAt(in int index) {
			if (index < 0 || index > this.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			this.PaletteEntries.RemoveAt(index);
		}

		/// <summary>
		/// Removes a palette with the specified palette number from the palette set.
		/// </summary>
		/// <param name="number">The number of the palette to remove.</param>
		/// <returns>true if a palette was removed; otherwise, false.</returns>
		public bool RemovePalette(in int number) {
			int index = this.IndexOfPalette(number);
			if (index >= 0) {
				this.RemoveAt(index);
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
		public Palette FindPalette(in int number) {
			this.TryFindPalette(number, out Palette palette);
			return palette;
		}
		/// <summary>
		/// Checks if the palette set contains a palette with the specified palette number.
		/// </summary>
		/// <param name="number">The number of the palette to find.</param>
		/// <returns>true if a palette was found; otherwise, false.</returns>
		public bool ContainsPalette(in int number)
			=> this.TryFindPalette(number, out _);
		/// <summary>
		/// Gets the palette associated with the specified palette number.
		/// </summary>
		/// <param name="number">The number of the palette to find.</param>
		/// <param name="palette">The palette, if it was found.</param>
		/// <returns>true if a palette was found; otherwise, false.</returns>
		public bool TryFindPalette(in int number, out Palette palette) {
			int i = this.IndexOfPalette(number);
			if ((i = this.IndexOfPalette(number)) >= 0) {
				palette = this.PaletteEntries[i].Palette;
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
			for (int i = 0; i < this.Count; i++) {
				if (this[i].Number == number) {
					return i;
				}
			}
			return -1;
		}

		public IEnumerator<PaletteEntry> GetEnumerator()
			=> this.PaletteEntries.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> this.PaletteEntries.GetEnumerator();
	}
}
