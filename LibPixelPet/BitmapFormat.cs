using System;

namespace LibPixelPet {
	public struct BitmapFormat : IEquatable<BitmapFormat> {
		public static readonly BitmapFormat GBA4BPP = new BitmapFormat(0, 10, 10, true, 11, true, 12, 4, true, true, ColorFormat.Grayscale4BPP);
		public static readonly BitmapFormat GBA8BPP = new BitmapFormat(0, 10, 10, true, 11, true, 12, 4, true, true, ColorFormat.Grayscale8BPP);

		/// <summary>
		/// Gets a bitmap format with the specified name, or null if no such bitmap format exists.
		/// </summary>
		/// <param name="formatName">The name of the bitmap format.</param>
		/// <returns>The bitmap format matching the specified name.</returns>
		public static BitmapFormat? GetFormat(in string formatName) {
			switch (formatName?.ToUpperInvariant()) {
			case "GBA-4BPP":
			case "NDS-4BPP":
				return BitmapFormat.GBA4BPP;
			case "GBA-8BPP":
			case "NDS-8BPP":
				return BitmapFormat.GBA8BPP;
			default:
				return null;
			}
		}

		private readonly byte tnumBits;
		private readonly byte tnumShift;
		/// <summary>
		/// Gets the number of bits for the tile number.
		/// </summary>
		public int TileNumberBits => tnumBits;
		/// <summary>
		/// Gets the shift count for the tile number.
		/// </summary>
		public int TileNumberShift => tnumShift;
		/// <summary>
		/// Gets the maximum value for the tile number.
		/// </summary>
		public int TileNumberMax => (1 << tnumBits) - 1;
		/// <summary>
		/// Gets the mask for the tile number.
		/// </summary>
		public int TileNumberMask => ((1 << tnumBits) - 1) << tnumShift;

		private readonly bool hflipAllowed;
		private readonly byte hflipShift;
		/// <summary>
		/// Gets a boolean that indicates whether tiles can be horizontally flipped.
		/// </summary>
		public bool CanFlipHorizontal => hflipAllowed;
		/// <summary>
		/// Gets the shift count for the horizontal flip bit.
		/// </summary>
		public int FlipHorizontalShift => hflipShift;
		/// <summary>
		/// Gets the mask for the horizontal flip bit.
		/// </summary>
		public int FlipHorizontalMask => 1 << hflipShift;

		private readonly bool vflipAllowed;
		private readonly byte vflipShift;
		/// <summary>
		/// Gets a boolean that indicates whether tiles can be vertically flipped.
		/// </summary>
		public bool CanFlipVertical => vflipAllowed;
		/// <summary>
		/// Gets the shift count for the vertical flip bit.
		/// </summary>
		public int FlipVerticalShift => vflipShift;
		/// <summary>
		/// Gets the mask for the vertical flip bit.
		/// </summary>
		public int FlipVerticalMask => 1 << vflipShift;

		/// <summary>
		/// Gets a boolean that indicates whether tiles can be flipped.
		/// </summary>
		public bool CanFlip => hflipAllowed && vflipAllowed;

		private readonly byte palBits;
		private readonly byte palShift;
		/// <summary>
		/// Gets the number of bits for the palette number.
		/// </summary>
		public int PaletteBits => palBits;
		/// <summary>
		/// Gets the shift count for the palette number.
		/// </summary>
		public int PaletteShift => palShift;
		/// <summary>
		/// Gets the maximum value for the palette number.
		/// </summary>
		public int PaletteMax => (1 << palBits) - 1;
		/// <summary>
		/// Gets the mask for the palette number.
		/// </summary>
		public int PaletteMask => ((1 << palBits) - 1) << palShift;

		private readonly bool reduceAllowed;
		/// <summary>
		/// Gets a boolean that indicates whether the tileset can be reduced.
		/// </summary>
		public bool CanReduceTileset => reduceAllowed;

		private readonly ColorFormat colorFmt;
		/// <summary>
		/// Gets the color format used for tiles.
		/// </summary>
		public ColorFormat ColorFormat => colorFmt;

		private readonly bool isIndexed;
		/// <summary>
		/// Gets a boolean that indicates whether this bitmap format uses indexed colors.
		/// </summary>
		public bool IsIndexed => isIndexed;

		/// <summary>
		/// Gets the mask for a tilemap entry.
		/// </summary>
		public int Mask
			=> this.TileNumberMask | this.FlipHorizontalMask | this.FlipVerticalMask | this.PaletteMask;

		/// <summary>
		/// Gets the total number of bits used.
		/// </summary>
		public int Bits
			=> Math.Max(Math.Max(tnumShift + tnumBits, palShift + palBits), Math.Max(hflipShift + 1, vflipShift + 1));

		/// <summary>
		/// Gets the maximum value a tilemap entry can take.
		/// </summary>
		public int MaxValue
			=> ((1 << this.Bits) - 1) & this.Mask;

		/// <summary>
		/// Checks that the specified tilemap entry is a valid tilemap entry for this bitmap format.
		/// </summary>
		/// <param name="entry">The tilemap entry to check.</param>
		/// <returns>true if the tilemap entry is valid; otherwise, false.</returns>
		public bool IsValid(int entry)
			=> (entry & this.Mask) == entry;

		private BitmapFormat(
			in int tnumShift, in int tnumBits,
			in int hflipShift, in bool hflipAllowed,
			in int vflipShift, in bool vflipAllowed,
			in int palShift, in int palBits,
			in bool reduceAllowed, in bool isIndexed,
			in ColorFormat colorFmt
		) {
			this.  tnumBits    = (byte)(tnumBits > 0 ?  tnumBits  : 0);
			this.   palBits    = (byte)( palBits > 0 ?   palBits  : 0);
			this.  tnumShift   = (byte)(tnumBits > 0 ?  tnumShift : 0);
			this.   palShift   = (byte)( palBits > 0 ?   palShift : 0);
			this. hflipShift   = (byte)(hflipAllowed ? hflipShift : 0);
			this. vflipShift   = (byte)(vflipAllowed ? vflipShift : 0);
			this. hflipAllowed =  hflipAllowed;
			this. vflipAllowed =  vflipAllowed;
			this.reduceAllowed = reduceAllowed;
			this. colorFmt     =  colorFmt;
			this.IsIndexed     = isIndexed;
			this.isIndexed     = isIndexed;
		}

		public bool Equals(BitmapFormat other)
			=> this.  tnumBits    == other.  tnumBits
			&& this.   palBits    == other.   palBits
			&& this.  tnumShift   == other.  tnumShift
			&& this. hflipShift   == other. hflipShift
			&& this. vflipShift   == other. vflipShift
			&& this.   palShift   == other.   palShift
			&& this. hflipAllowed == other. hflipAllowed
			&& this. vflipAllowed == other. vflipAllowed
			&& this.reduceAllowed == other.reduceAllowed
			&& this.isIndexed     == other.isIndexed
			&& this. colorFmt     == other.colorFmt

		public override bool Equals(object obj)
			=> obj is BitmapFormat tf ? this.Equals(tf) : false;

		public override int GetHashCode() {
			unchecked {
				int hash = -490236692;
				hash = hash * -1521134295 + this.  tnumBits;
				hash = hash * -1521134295 + this.   palBits;
				hash = hash * -1521134295 + this.  tnumShift;
				hash = hash * -1521134295 + this. hflipShift;
				hash = hash * -1521134295 + this. vflipShift;
				hash = hash * -1521134295 + this.   palShift;
				hash = hash * -1521134295 + this. hflipAllowed.GetHashCode();
				hash = hash * -1521134295 + this. vflipAllowed.GetHashCode();
				hash = hash * -1521134295 + this.reduceAllowed.GetHashCode();
				hash = hash * -1521134295 + this.    isIndexed.GetHashCode();
				hash = hash * -1521134295 + this.     colorFmt.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(BitmapFormat format1, BitmapFormat format2)
			=> format1.Equals(format2);
		public static bool operator !=(BitmapFormat format1, BitmapFormat format2)
			=> !format1.Equals(format2);
	}
}
