using System;

namespace LibPixelPet {
	public readonly struct TilemapFormat : IEquatable<TilemapFormat> {
		public static readonly TilemapFormat GB        = new(8, 8, 0,  8,  0, false,  0, false,  0,  0,  0, 0,  true, false, ColorFormat.GameBoy,       BitmapEncoding.GameBoy);
		public static readonly TilemapFormat GBA4BPP   = new(8, 8, 0, 10, 10,  true, 11,  true, 12,  4,  0, 0,  true,  true, ColorFormat.Grayscale4BPP, BitmapEncoding.GameBoyAdvance);
		public static readonly TilemapFormat GBA8BPP   = new(8, 8, 0, 10, 10,  true, 11,  true, 12,  4,  0, 0,  true,  true, ColorFormat.Grayscale8BPP, BitmapEncoding.GameBoyAdvance);
		public static readonly TilemapFormat GBAAffine = new(8, 8, 0,  8,  0, false,  0, false,  0,  0,  0, 0,  true,  true, ColorFormat.Grayscale8BPP, BitmapEncoding.GameBoyAdvance);
		public static readonly TilemapFormat NDSTEX5   = new(4, 4, 0,  0,  0, false,  0, false,  0, 14, 14, 2, false,  true, ColorFormat.Grayscale2BPP, BitmapEncoding.NintendoDSTexture);

		/// <summary>
		/// Gets a tilemap format with the specified name, or null if no such tilemap format exists.
		/// </summary>
		/// <param name="formatName">The name of the tilemap format.</param>
		/// <returns>The tilemap format matching the specified name.</returns>
		public static TilemapFormat? GetFormat(in string formatName) {
			return (formatName?.ToUpperInvariant()) switch {
				"GB"                     => GB,
				"GBA-4BPP" or "NDS-4BPP" => GBA4BPP,
				"GBA-8BPP" or "NDS-8BPP" => GBA8BPP,
				"GBA-AFFINE"             => GBAAffine,
				"NDS-TEX5"               => NDSTEX5,
				_                        => null,
			};
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
		/// Gets a boolean that indicates whether this tilemap format uses indexed colors.
		/// </summary>
		public bool IsIndexed => isIndexed;

		/// <summary>
		/// Gets the mask for a tilemap entry.
		/// </summary>
		public int Mask
			=> TileNumberMask | FlipHorizontalMask | FlipVerticalMask | PaletteMask | ModeMask;

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
			=> ((1 << Bits) - 1) & Mask;

		/// <summary>
		/// Checks that the specified tilemap entry is a valid tilemap entry for this tilemap format.
		/// </summary>
		/// <param name="entry">The tilemap entry to check.</param>
		/// <returns>true if the tilemap entry is valid; otherwise, false.</returns>
		public bool IsValid(int entry)
			=> (entry & Mask) == entry;

		private TilemapFormat(
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

		public bool Equals(TilemapFormat other)
			=>      tWidth   == other.     tWidth
			&&      tHeight  == other.     tHeight
			&&   tnumBits    == other.  tnumBits
			&&    palBits    == other.   palBits
			&&   modeBits    == other.  modeBits
			&&   tnumShift   == other.  tnumShift
			&&  hflipShift   == other. hflipShift
			&&  vflipShift   == other. vflipShift
			&&    palShift   == other.   palShift
			&&   modeShift   == other.  modeShift
			&&  hflipAllowed == other. hflipAllowed
			&&  vflipAllowed == other. vflipAllowed
			&& reduceAllowed == other.reduceAllowed
			&& isIndexed     == other.isIndexed
			&&  colorFmt     == other.colorFmt
			&& bmpEncoding   == other.bmpEncoding;

		public override bool Equals(object? obj)
			=> obj is TilemapFormat tf && Equals(tf);

		public override int GetHashCode() {
			HashCode hash = new();
			hash.Add(tWidth);
			hash.Add(tHeight);
			hash.Add(tnumBits);
			hash.Add(palBits);
			hash.Add(modeBits);
			hash.Add(tnumShift);
			hash.Add(hflipShift);
			hash.Add(vflipShift);
			hash.Add(palShift);
			hash.Add(modeShift);
			hash.Add(hflipAllowed);
			hash.Add(vflipAllowed);
			hash.Add(reduceAllowed);
			hash.Add(isIndexed);
			hash.Add(colorFmt);
			hash.Add(bmpEncoding);
			return hash.ToHashCode();
		}

		public static bool operator ==(TilemapFormat format1, TilemapFormat format2)
			=> format1.Equals(format2);
		public static bool operator !=(TilemapFormat format1, TilemapFormat format2)
			=> !format1.Equals(format2);
	}
}
