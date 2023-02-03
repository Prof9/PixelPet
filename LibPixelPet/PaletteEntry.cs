using System;

namespace LibPixelPet {
	public readonly struct PaletteEntry : IEquatable<PaletteEntry> {
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
			if (palette is null)
				throw new ArgumentNullException(nameof(palette));

			Number = number;
			Palette = palette;
		}

		public bool IsValid()
			=> Number >= 0
			&& Palette is not null;

		public override bool Equals(object obj)
			=> obj is PaletteEntry pe
			&& Equals(pe);
		public bool Equals(PaletteEntry other)
			=> Number == other.Number
			&& Palette == other.Palette;

		public override int GetHashCode()
			=> HashCode.Combine(Number, Palette);

		public static bool operator ==(PaletteEntry entry1, PaletteEntry entry2)
			=> entry1.Equals(entry2);
		public static bool operator !=(PaletteEntry entry1, PaletteEntry entry2)
			=> !entry1.Equals(entry2);
	}
}
