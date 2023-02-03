using System;

namespace LibPixelPet {
	public readonly struct TileEntry : IEquatable<TileEntry> {
		/// <summary>
		/// Gets or sets the number of the tile.
		/// </summary>
		public int TileNumber { get; }
		/// <summary>
		/// Gets or sets a boolean that indicates whether the tile is horizontally flipped.
		/// </summary>
		public bool HFlip { get; }
		/// <summary>
		/// Gets or sets a boolean that indicates whether the tile is vertically flipped.
		/// </summary>
		public bool VFlip { get; }
		/// <summary>
		/// Gets the palette number for this tile entry.
		/// </summary>
		public int PaletteNumber { get; }
		/// <summary>
		/// Gets the texture mode for this tile entry.
		/// </summary>
		public int TextureMode { get; }

		public TileEntry(int tileNumber, bool hFlip, bool vFlip)
			: this(tileNumber, hFlip, vFlip, 0) { }

		public TileEntry(int tileNumber, bool hFlip, bool vFlip, int paletteNumber)
			: this(tileNumber, hFlip, vFlip, paletteNumber, 0) { }

		public TileEntry(int tileNumber, bool hFlip, bool vFlip, int paletteNumber, int mode) {
			if (tileNumber < 0)
				throw new ArgumentOutOfRangeException(nameof(tileNumber));

			TileNumber = tileNumber;
			HFlip = hFlip;
			VFlip = vFlip;
			PaletteNumber = paletteNumber;
			TextureMode = mode;
		}

		public override int GetHashCode() {
			unchecked {
				// Pretty crappy hash.
				int hash = (TileNumber + PaletteNumber << 16);
				if (HFlip) {
					hash ^= 0x55555555;
				}
				if (VFlip) {
					hash ^= (int)0xAAAAAAAA;
				}
				return hash;
			}
		}

		public override bool Equals(object obj)
			=> obj is TileEntry entry
			&& Equals(entry);
		public bool Equals(TileEntry other)
			=> TileNumber    == other.TileNumber
			&& HFlip         == other.HFlip
			&& VFlip         == other.VFlip
			&& PaletteNumber == other.PaletteNumber;

		public static bool operator ==(TileEntry entry1, TileEntry entry2)
			=> entry1.Equals(entry2);
		public static bool operator !=(TileEntry entry1, TileEntry entry2)
			=> !entry1.Equals(entry2);
	}
}
