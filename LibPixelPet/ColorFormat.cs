using System;
using System.Drawing;

namespace LibPixelPet {
	public enum PixelEncoding {
		Normal,
		GameBoy,
	}

	public readonly struct ColorFormat : IEquatable<ColorFormat> {
		public static readonly ColorFormat RGB555        = SequentialARGB(5, 5, 5, 0, 1);
		public static readonly ColorFormat BGR555        = SequentialABGR(5, 5, 5, 0, 1);
		public static readonly ColorFormat RGB888        = SequentialARGB(8, 8, 8, 0, 0);
		public static readonly ColorFormat ABGR1555      = SequentialABGR(5, 5, 5, 1, 0);
		public static readonly ColorFormat ARGB8888      = SequentialARGB(8, 8, 8, 8, 0);
		public static readonly ColorFormat Grayscale2BPP = Grayscale(2);
		public static readonly ColorFormat Grayscale4BPP = Grayscale(4);
		public static readonly ColorFormat Grayscale8BPP = Grayscale(8);
		public static readonly ColorFormat GameBoy       = new(PixelEncoding.GameBoy, 0, 2, 0, 2, 0, 2, 0, 0, 0, true);

		public static ColorFormat SequentialABGR(in int rBits, in int gBits, in int bBits, in int aBits, in int padBits)
			=> new(PixelEncoding.Normal, 0, rBits, rBits, gBits, rBits + gBits, bBits, rBits + gBits + bBits, aBits, padBits, false);
		public static ColorFormat SequentialARGB(in int rBits, in int gBits, in int bBits, in int aBits, in int padBits)
			=> new(PixelEncoding.Normal, bBits + gBits, rBits, bBits, gBits, 0, bBits, bBits + gBits + rBits, aBits, padBits, false);
		public static ColorFormat Grayscale(in int bits)
			=> new(PixelEncoding.Normal, 0, bits, 0, bits, 0, bits, 0, 0, 0, false);

		/// <summary>
		/// Gets a color format with the specified name, or null if no such color format exists.
		/// </summary>
		/// <param name="formatName">The name of the color format.</param>
		/// <returns>The color format matching the specified name.</returns>
		public static ColorFormat? GetFormat(in string formatName) {
			return formatName?.ToUpperInvariant() switch {
				"2BPP"                => Grayscale2BPP,
				"4BPP"                => Grayscale4BPP,
				"8BPP"                => Grayscale8BPP,
				"RGB555"              => RGB555,
				"BGR555"   or "GBA"   => BGR555,
				"ABGR1555" or "NDS"   => ABGR1555,
				"RGB888"   or "24BPP" => RGB888,
				"ARGB8888" or "32BPP" => ARGB8888,
				"GB"                  => GameBoy,
				_                     => null,
			};
		}

		/// <summary>
		/// Converts a color value to this color format.
		/// </summary>
		/// <param name="color">The color value to convert.</param>
		/// <param name="from">The color format that the color value is currently in.</param>
		/// <returns>The converted color value.</returns>
		public int Convert(in int color, in ColorFormat from)
			=> Convert(color, from, false);

		/// <summary>
		/// Converts a color value to this color format.
		/// </summary>
		/// <param name="color">The color value to convert.</param>
		/// <param name="from">The color format that the color value is currently in.</param>
		/// <param name="sloppy">If true, uses 'sloppy' conversion consisting only of bit-shifts; otherwise, uses accurate conversion.</param>
		/// <returns>The converted color value.</returns>
		public int Convert(in int color, in ColorFormat from, bool sloppy)
			=> Convert(color, from, this, sloppy);

		/// <summary>
		/// Converts a color to this color format.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The converted color value.</returns>
		public int Convert(in Color color)
			=> Convert(color, false);

		/// <summary>
		/// Converts a color to this color format.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <param name="sloppy">If true, uses 'sloppy' conversion consisting only of bit-shifts; otherwise, uses accurate conversion.</param>
		/// <returns>The converted color value.</returns>
		public int Convert(in Color color, bool sloppy)
			=> Convert(color.ToArgb(), ColorFormat.ARGB8888, sloppy);

		private static int Convert(in int color, in ColorFormat from, in ColorFormat to, bool sloppy) {
			int r = (int)((uint)(color & from.  RedMask) >> from.rShift);
			int g = (int)((uint)(color & from.GreenMask) >> from.gShift);
			int b = (int)((uint)(color & from. BlueMask) >> from.bShift);
			int a = (int)((uint)(color & from.AlphaMask) >> from.aShift);

			ConvertComponent(ref r, from.rBits, to.rBits, to.  RedMax, from.  RedMax, 0);
			ConvertComponent(ref g, from.gBits, to.gBits, to.GreenMax, from.GreenMax, 0);
			ConvertComponent(ref b, from.bBits, to.bBits, to. BlueMax, from. BlueMax, 0);
			ConvertComponent(ref a, from.aBits, to.aBits, to.AlphaMax, from.AlphaMax, to.AlphaMax);

			if (from.Invert != to.Invert) {
				r = (to.RedMax   - r);
				g = (to.GreenMax - g);
				b = (to.BlueMax  - b);
			}

			return (r << to.rShift)
			     | (g << to.gShift)
			     | (b << to.bShift)
			     | (a << to.aShift);

			void ConvertComponent(ref int c, in int fromBits, in int toBits, in int toMax, in int fromMax, in int def) {
				if (toBits == fromBits) {
					return;
				} else if (fromBits == 0) {
					c = def; // default
				} else if (sloppy && toBits < fromBits) {
					c >>= fromBits - toBits;
				} else if (sloppy && toBits > fromBits) {
					c <<= toBits - fromBits;
				} else {
					c = (c * toMax + fromMax / 2) / fromMax;
				}
			}
		}

		private readonly PixelEncoding encoding;
		/// <summary>
		/// Gets the pixel encoding method.
		/// </summary>
		public PixelEncoding Encoding => encoding;

		private readonly byte rBits;
		private readonly byte rShift;
		/// <summary>
		/// Gets the number of bits for the red color channel.
		/// </summary>
		public int RedBits => rBits;
		/// <summary>
		/// Gets the shift count for the red color channel.
		/// </summary>
		public int RedShift => rShift;
		/// <summary>
		/// Gets the maximum value for the red color channel.
		/// </summary>
		public int RedMax => (1 << rBits) - 1;
		/// <summary>
		/// Gets the mask for the red color channel.
		/// </summary>
		public int RedMask => ((1 << rBits) - 1) << rShift;

		private readonly byte gBits;
		private readonly byte gShift;
		/// <summary>
		/// Gets the number of bits for the green color channel.
		/// </summary>
		public int GreenBits => gBits;
		/// <summary>
		/// Gets the shift count for the green color channel.
		/// </summary>
		public int GreenShift => gShift;
		/// <summary>
		/// Gets the maximum value for the green color channel.
		/// </summary>
		public int GreenMax => (1 << gBits) - 1;
		/// <summary>
		/// Gets the mask for the green color channel.
		/// </summary>
		public int GreenMask => ((1 << gBits) - 1) << gShift;

		private readonly byte bBits;
		private readonly byte bShift;
		/// <summary>
		/// Gets the number of bits for the blue color channel.
		/// </summary>
		public int BlueBits => bBits;
		/// <summary>
		/// Gets the shift count for the blue color channel.
		/// </summary>
		public int BlueShift => bShift;
		/// <summary>
		/// Gets the maximum value for the blue color channel.
		/// </summary>
		public int BlueMax => (1 << bBits) - 1;
		/// <summary>
		/// Gets the mask for the blue color channel.
		/// </summary>
		public int BlueMask => ((1 << bBits) - 1) << bShift;

		private readonly byte aBits;
		private readonly byte aShift;
		/// <summary>
		/// Gets the number of bits for the alpha channel.
		/// </summary>
		public int AlphaBits => aBits;
		/// <summary>
		/// Gets the shift count for the alpha channel.
		/// </summary>
		public int AlphaShift => aShift;
		/// <summary>
		/// Gets the maximum value for the alpha channel.
		/// </summary>
		public int AlphaMax => (1 << aBits) - 1;
		/// <summary>
		/// Gets the mask for the alpha channel.
		/// </summary>
		public int AlphaMask => ((1 << aBits) - 1) << aShift;

		private readonly bool invert;
		/// <summary>
		/// Gets a boolean indicating whether color values are inverted.
		/// </summary>
		public bool Invert => invert;

		/// <summary>
		/// Gets the mask for the color channel.
		/// </summary>
		public int ColorMask
			=> RedMask | GreenMask | BlueMask;

		/// <summary>
		/// Gets the mask for the color and alpha channels.
		/// </summary>
		public int Mask
			=> ColorMask | AlphaMask;

		private readonly byte padBits;
		/// <summary>
		/// Gets the number of padding bits.
		/// </summary>
		public int PadBits => padBits;

		/// <summary>
		/// Gets the total number of bits used.
		/// </summary>
		public int Bits
			=> Math.Max(Math.Max(rShift + rBits, gShift + gBits), Math.Max(bShift + bBits, aShift + aBits)) + padBits;

		/// <summary>
		/// Gets the minimum number of bytes needed to represent one pixel.
		/// Depending on the encoding, this may differ from the number of bits used.
		/// </summary>
		public int Bytes
			=> Encoding switch {
				PixelEncoding.GameBoy     => Bits,
				PixelEncoding.Normal or _ => (Bits + 7) / 8,
			};

		/// <summary>
		/// Gets the maximum value this color can take.
		/// </summary>
		public int MaxValue
			=> ((1 << Bits) - 1) & Mask;

		/// <summary>
		/// Checks that the specified color value is a valid color value for this color format.
		/// </summary>
		/// <param name="color">The color value to check.</param>
		/// <returns>true if the color value is valid; otherwise, false.</returns>
		public bool IsValid(int color)
			=> (color & Mask) == color;

		private ColorFormat(
			in PixelEncoding encoding,
			in int rShift, in int rBits,
			in int gShift, in int gBits,
			in int bShift, in int bBits,
			in int aShift, in int aBits,
			in int padBits, in bool invert
		) {
			this.encoding = encoding;
			this.rBits    = (byte)(rBits   > 0 ? rBits   : 0);
			this.gBits    = (byte)(gBits   > 0 ? gBits   : 0);
			this.bBits    = (byte)(bBits   > 0 ? bBits   : 0);
			this.aBits    = (byte)(aBits   > 0 ? aBits   : 0);
			this.rShift   = (byte)(rBits   > 0 ? rShift  : 0);
			this.gShift   = (byte)(gBits   > 0 ? gShift  : 0);
			this.bShift   = (byte)(bBits   > 0 ? bShift  : 0);
			this.aShift   = (byte)(aBits   > 0 ? aShift  : 0);
			this.padBits  = (byte)(padBits > 0 ? padBits : 0);
			this.invert   = invert;
		}

		public bool Equals(ColorFormat other)
			=> encoding == other.encoding
			&& rBits    == other.rBits
			&& gBits    == other.gBits
			&& bBits    == other.bBits
			&& aBits    == other.aBits
			&& rShift   == other.rShift
			&& gShift   == other.gShift
			&& bShift   == other.bShift
			&& aShift   == other.aShift
			&& padBits  == other.padBits
			&& invert   == other.invert;

		public override bool Equals(object? obj)
			=> obj is ColorFormat pf && Equals(pf);

		public override int GetHashCode() {
			HashCode hash = new();
			hash.Add(encoding);
			hash.Add(rBits);
			hash.Add(gBits);
			hash.Add(bBits);
			hash.Add(aBits);
			hash.Add(rShift);
			hash.Add(gShift);
			hash.Add(bShift);
			hash.Add(aShift);
			hash.Add(padBits);
			hash.Add(invert);
			return hash.ToHashCode();
		}

		public static bool operator ==(ColorFormat format1, ColorFormat format2)
			=> format1.Equals(format2);
		public static bool operator !=(ColorFormat format1, ColorFormat format2)
			=> !format1.Equals(format2);
	}
}
