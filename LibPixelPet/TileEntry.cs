using System;

namespace LibPixelPet {
	public struct TileEntry : IEquatable<TileEntry> {
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

		public TileEntry(int tileNumber, bool hFlip, bool vFlip)
			: this(tileNumber, hFlip, vFlip, 0) { }

		public TileEntry(int tileNumber, bool hFlip, bool vFlip, int paletteNumber) {
			this.TileNumber = tileNumber;
			this.HFlip = hFlip;
			this.VFlip = vFlip;
			this.PaletteNumber = paletteNumber;
		}

		public override int GetHashCode() {
			unchecked {
				// Pretty crappy hash.
				int hash = (this.TileNumber + this.PaletteNumber << 16);
				if (this.HFlip) {
					hash ^= 0x55555555;
				}
				if (this.VFlip) {
					hash ^= (int)0xAAAAAAAA;
				}
				return hash;
			}
		}

		public override bool Equals(object obj)
			=> obj is TileEntry entry
			&& this.Equals(entry);
		public bool Equals(TileEntry entry)
			=> this.TileNumber    == entry.TileNumber
			&& this.HFlip         == entry.HFlip
			&& this.VFlip         == entry.VFlip
			&& this.PaletteNumber == entry.PaletteNumber;

		public static bool operator ==(TileEntry entry1, TileEntry entry2)
			=> entry1.Equals(entry2);
		public static bool operator !=(TileEntry entry1, TileEntry entry2)
			=> !entry1.Equals(entry2);
	}
}
