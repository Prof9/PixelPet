using System;

namespace LibPixelPet {
	public struct BitmapFormat : IEquatable<BitmapFormat> {
		public static readonly BitmapFormat GB      = new BitmapFormat(8, 8, 0,  8,  0, false,  0, false,  0,  0,  0, 0,  true, false, ColorFormat.GameBoy,       BitmapEncoding.GameBoy);
		public static readonly BitmapFormat GBA4BPP = new BitmapFormat(8, 8, 0, 10, 10,  true, 11,  true, 12,  4,  0, 0,  true,  true, ColorFormat.Grayscale4BPP, BitmapEncoding.GameBoyAdvance);
		public static readonly BitmapFormat GBA8BPP = new BitmapFormat(8, 8, 0, 10, 10,  true, 11,  true, 12,  4,  0, 0,  true,  true, ColorFormat.Grayscale8BPP, BitmapEncoding.GameBoyAdvance);
		public static readonly BitmapFormat NDSTEX5 = new BitmapFormat(4, 4, 0,  0,  0, false,  0, false,  0, 14, 14, 2, false,  true, ColorFormat.Grayscale2BPP, BitmapEncoding.NintendoDSTexture);

		/// <summary>
		/// Gets a bitmap format with the specified name, or null if no such bitmap format exists.
		/// </summary>
		/// <param name="formatName">The name of the bitmap format.</param>
		/// <returns>The bitmap format matching the specified name.</returns>
		public static BitmapFormat? GetFormat(in string formatName) {
			switch (formatName?.ToUpperInvariant()) {
			case "GB":
				return BitmapFormat.GB;
			case "GBA-4BPP":
			case "NDS-4BPP":
				return BitmapFormat.GBA4BPP;
			case "GBA-8BPP":
			case "NDS-8BPP":
				return BitmapFormat.GBA8BPP;
			case "NDS-TEX5":
				return BitmapFormat.NDSTEX5;
			default:
				return null;
			}
		}

		private readonly byte tWidth;
		private readonly byte tHeight;
		/// <summary>
		/// Gets the tile width in pixels.
		/// </summary>
		public int TileWidth => tWidth;
		/// <summary>
		/// Gets the tile height in pixels.
		/// </summary>
		public int TileHeight => tHeight;

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

		private readonly byte modeBits;
		private readonly byte modeShift;
		/// <summary>
		/// Gets the number of bits for the mode.
		/// </summary>
		public int ModeBits => modeBits;
		/// <summary>
		/// Gets the shift count for the mode.
		/// </summary>
		public int ModeShift => modeShift;
		/// <summary>
		/// Gets the maximum value for the mode.
		/// </summary>
		public int ModeMax => (1 << modeBits) - 1;
		/// <summary>
		/// Gets the mask for the mode.
		/// </summary>
		public int ModeMask => ((1 << modeBits) - 1) << modeShift;

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

		private readonly BitmapEncoding bmpEncoding;
		/// <summary>
		/// Gets the bitmap encoding.
		/// </summary>
		public BitmapEncoding BitmapEncoding => bmpEncoding;

		private readonly bool isIndexed;
		/// <summary>
		/// Gets a boolean that indicates whether this bitmap format uses indexed colors.
		/// </summary>
		public bool IsIndexed => isIndexed;

		/// <summary>
		/// Gets the mask for a tilemap entry.
		/// </summary>
		public int Mask
			=> this.TileNumberMask | this.FlipHorizontalMask | this.FlipVerticalMask | this.PaletteMask | this.ModeMask;

		/// <summary>
		/// Gets the total number of bits used.
		/// </summary>
		public int Bits
			=> Math.Max(
				Math.Max(
					Math.Max(tnumShift + tnumBits, palShift + palBits),
					(modeShift + modeBits)
				),
				Math.Max(hflipShift + 1, vflipShift + 1)
			);

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
			in int tWidth, in int tHeight,
			in int tnumShift, in int tnumBits,
			in int hflipShift, in bool hflipAllowed,
			in int vflipShift, in bool vflipAllowed,
			in int palShift, in int palBits,
			in int modeShift, in int modeBits,
			in bool reduceAllowed, in bool isIndexed,
			in ColorFormat colorFmt, in BitmapEncoding bmpEncoding
		) {
			this.     tWidth   = (byte)(  tWidth > 0 ?     tWidth  : 0);
			this.     tHeight  = (byte)( tHeight > 0 ?     tHeight : 0);
			this.  tnumBits    = (byte)(tnumBits > 0 ?  tnumBits   : 0);
			this.   palBits    = (byte)( palBits > 0 ?   palBits   : 0);
			this.  modeBits    = (byte)(modeBits > 0 ?  modeBits   : 0);
			this.  tnumShift   = (byte)(tnumBits > 0 ?  tnumShift  : 0);
			this.   palShift   = (byte)( palBits > 0 ?   palShift  : 0);
			this.  modeShift   = (byte)(modeBits > 0 ?  modeShift  : 0);
			this. hflipShift   = (byte)(hflipAllowed ? hflipShift  : 0);
			this. vflipShift   = (byte)(vflipAllowed ? vflipShift  : 0);
			this. hflipAllowed =  hflipAllowed;
			this. vflipAllowed =  vflipAllowed;
			this.reduceAllowed = reduceAllowed;
			this. colorFmt     =  colorFmt;
			this.bmpEncoding   = bmpEncoding;
			this.isIndexed     = isIndexed;
		}

		public bool Equals(BitmapFormat other)
			=> this.     tWidth   == other.     tWidth
			&& this.     tHeight  == other.     tHeight
			&& this.  tnumBits    == other.  tnumBits
			&& this.   palBits    == other.   palBits
			&& this.  modeBits    == other.  modeBits
			&& this.  tnumShift   == other.  tnumShift
			&& this. hflipShift   == other. hflipShift
			&& this. vflipShift   == other. vflipShift
			&& this.   palShift   == other.   palShift
			&& this.  modeShift   == other.  modeShift
			&& this. hflipAllowed == other. hflipAllowed
			&& this. vflipAllowed == other. vflipAllowed
			&& this.reduceAllowed == other.reduceAllowed
			&& this.isIndexed     == other.isIndexed
			&& this. colorFmt     == other.colorFmt
			&& this.bmpEncoding   == other.bmpEncoding;

		public override bool Equals(object obj)
			=> obj is BitmapFormat tf ? this.Equals(tf) : false;

		public override int GetHashCode() {
			unchecked {
				int hash = -490236692;
				hash = hash * -1521134295 + this.     tWidth;
				hash = hash * -1521134295 + this.     tHeight;
				hash = hash * -1521134295 + this.  tnumBits;
				hash = hash * -1521134295 + this.   palBits;
				hash = hash * -1521134295 + this.  modeBits;
				hash = hash * -1521134295 + this.  tnumShift;
				hash = hash * -1521134295 + this. hflipShift;
				hash = hash * -1521134295 + this. vflipShift;
				hash = hash * -1521134295 + this.   palShift;
				hash = hash * -1521134295 + this.  modeShift;
				hash = hash * -1521134295 + this. hflipAllowed.GetHashCode();
				hash = hash * -1521134295 + this. vflipAllowed.GetHashCode();
				hash = hash * -1521134295 + this.reduceAllowed.GetHashCode();
				hash = hash * -1521134295 + this.    isIndexed.GetHashCode();
				hash = hash * -1521134295 + this.     colorFmt.GetHashCode();
				hash = hash * -1521134295 + this.  bmpEncoding.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(BitmapFormat format1, BitmapFormat format2)
			=> format1.Equals(format2);
		public static bool operator !=(BitmapFormat format1, BitmapFormat format2)
			=> !format1.Equals(format2);
	}
}
