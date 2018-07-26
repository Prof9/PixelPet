using System;

namespace LibPixelPet {
	public struct PaletteEntry : IEquatable<PaletteEntry> {
		/// <summary>
		/// Gets the palette number.
		/// </summary>
		public int Number { get; }
		/// <summary>
		/// Gets the palette.
		/// </summary>
		public Palette Palette { get; }

		public PaletteEntry(int number, Palette palette) {
			if (number < 0)
				throw new ArgumentOutOfRangeException(nameof(number));
			if (palette == null)
				throw new ArgumentNullException(nameof(palette));

			this.Number = number;
			this.Palette = palette;
		}

		public bool IsValid()
			=> this.Number >= 0
			&& this.Palette != null;

		public override bool Equals(object obj)
			=> obj is PaletteEntry pe && this.Equals(pe);
		public bool Equals(PaletteEntry other)
			=> this.Number == other.Number
			&& this.Palette == other.Palette;

		public override int GetHashCode() {
			unchecked {
				int hash = -2093347853;
				hash = hash * -1521134295 + this.Number.GetHashCode();
				hash = hash * -1521134295 + this.Palette.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(PaletteEntry entry1, PaletteEntry entry2) => entry1.Equals(entry2);
		public static bool operator !=(PaletteEntry entry1, PaletteEntry entry2) => !entry1.Equals(entry2);
	}
}
