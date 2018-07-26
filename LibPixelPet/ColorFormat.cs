using System;
using System.Drawing;

namespace LibPixelPet {
	public struct ColorFormat : IEquatable<ColorFormat> {
		public static readonly ColorFormat RGB555 = new ColorFormat(0, 5, 5, 5, 10, 5, 0, 0);
		public static readonly ColorFormat BGR888 = new ColorFormat(16, 8, 8, 8, 0, 8, 0, 0);
		public static readonly ColorFormat RGBA5551 = new ColorFormat(0, 5, 5, 5, 10, 5, 15, 1);
		public static readonly ColorFormat BGRA8888 = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
		public static readonly ColorFormat Grayscale4BPP = new ColorFormat(0, 4, 0, 4, 0, 4, 0, 0);
		public static readonly ColorFormat Grayscale8BPP = new ColorFormat(0, 8, 0, 8, 0, 8, 0, 0);

		public static ColorFormat SequentialRGBA(in int rBits, in int gBits, in int bBits, in int aBits)
			=> new ColorFormat(0, rBits, rBits, gBits, rBits + gBits, bBits, rBits + gBits + bBits, aBits);
		public static ColorFormat SequentialBGRA(in int rBits, in int gBits, in int bBits, in int aBits)
			=> new ColorFormat(bBits + gBits, rBits, bBits, gBits, 0, bBits, bBits + gBits + rBits, aBits);
		public static ColorFormat Grayscale(in int bits)
			=> new ColorFormat(0, bits, 0, bits, 0, bits, 0, 0);

		/// <summary>
		/// Gets a color format with the specified name, or null if no such color format exists.
		/// </summary>
		/// <param name="formatName">The name of the color format.</param>
		/// <returns>The color format matching the specified name.</returns>
		public static ColorFormat? GetFormat(in string formatName) {
			switch (formatName?.ToUpperInvariant()) {
			case "4BPP":
				return ColorFormat.Grayscale4BPP;
			case "8BPP":
				return ColorFormat.Grayscale8BPP;
			case "RGB555":
			case "GBA":
				return ColorFormat.RGB555;
			case "RGBA5551":
			case "NDS":
				return ColorFormat.RGBA5551;
			case "BGR888":
			case "24BPP":
				return ColorFormat.BGR888;
			case "BGRA8888":
			case "32BPP":
				return ColorFormat.BGRA8888;
			default:
				return null;
			}
		}

		/// <summary>
		/// Converts a color value to this color format.
		/// </summary>
		/// <param name="color">The color value to convert.</param>
		/// <param name="from">The color format that the color value is currently in.</param>
		/// <returns>The converted color value.</returns>
		public int Convert(in int color, in ColorFormat from)
			=> this.Convert(color, from, false);

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
			=> this.Convert(color, false);

		/// <summary>
		/// Converts a color to this color format.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <param name="sloppy">If true, uses 'sloppy' conversion consisting only of bit-shifts; otherwise, uses accurate conversion.</param>
		/// <returns>The converted color value.</returns>
		public int Convert(in Color color, bool sloppy)
			=> this.Convert(color.ToArgb(), ColorFormat.BGRA8888, sloppy);

		private static int Convert(in int color, in ColorFormat from, in ColorFormat to, bool sloppy) {
			int r = (int)((uint)(color & from.  RedMask) >> from.rShift);
			int g = (int)((uint)(color & from.GreenMask) >> from.gShift);
			int b = (int)((uint)(color & from. BlueMask) >> from.bShift);
			int a = (int)((uint)(color & from.AlphaMask) >> from.aShift);

			ConvertComponent(ref r, from.rBits, to.rBits, to.  RedMax, from.  RedMax, 0);
			ConvertComponent(ref g, from.gBits, to.gBits, to.GreenMax, from.GreenMax, 0);
			ConvertComponent(ref b, from.bBits, to.bBits, to. BlueMax, from. BlueMax, 0);
			ConvertComponent(ref a, from.aBits, to.aBits, to.AlphaMax, from.AlphaMax, to.AlphaMax);

			return (r << to.rShift)
			     | (g << to.gShift)
			     | (b << to.bShift)
			     | (a << to.aShift);

			void ConvertComponent(ref int c, in int fromBits, in int toBits, in int toMax, in int fromMax, in int def) {
				if (toBits == fromBits) {
					return;
				} else if (fromBits == 0) {
					c = def;
				} else if (sloppy && toBits < fromBits) {
					c >>= fromBits - toBits;
				} else if (sloppy && toBits > fromBits) {
					c <<= toBits - fromBits;
				} else {
					c = (c * toMax + fromMax / 2) / fromMax;
				}
			}
		}

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

		/// <summary>
		/// Gets the mask for the color channel.
		/// </summary>
		public int ColorMask
			=> this.RedMask | this.GreenMask | this.BlueMask;

		/// <summary>
		/// Gets the mask for the color and alpha channels.
		/// </summary>
		public int Mask
			=> this.ColorMask | this.AlphaMask;

		/// <summary>
		/// Gets the total number of bits used.
		/// </summary>
		public int Bits
			=> Math.Max(Math.Max(rShift + rBits, gShift + gBits), Math.Max(bShift + bBits, aShift + aBits));

		/// <summary>
		/// Gets the maximum value this color can take.
		/// </summary>
		public int MaxValue
			=> ((1 << this.Bits) - 1) & this.Mask;

		/// <summary>
		/// Checks that the specified color value is a valid color value for this color format.
		/// </summary>
		/// <param name="color">The color value to check.</param>
		/// <returns>true if the color value is valid; otherwise, false.</returns>
		public bool IsValid(int color)
			=> (color & this.Mask) == color;

		private ColorFormat(
			in int rShift, in int rBits,
			in int gShift, in int gBits,
			in int bShift, in int bBits,
			in int aShift, in int aBits
		) {
			this.rBits  = (byte)(rBits > 0 ? rBits  : 0);
			this.gBits  = (byte)(gBits > 0 ? gBits  : 0);
			this.bBits  = (byte)(bBits > 0 ? bBits  : 0);
			this.aBits  = (byte)(aBits > 0 ? aBits  : 0);
			this.rShift = (byte)(rBits > 0 ? rShift : 0);
			this.gShift = (byte)(gBits > 0 ? gShift : 0);
			this.bShift = (byte)(bBits > 0 ? bShift : 0);
			this.aShift = (byte)(aBits > 0 ? aShift : 0);
		}

		public bool Equals(ColorFormat other)
			=> this.rBits  == other.rBits
			&& this.gBits  == other.gBits
			&& this.bBits  == other.bBits
			&& this.aBits  == other.aBits
			&& this.rShift == other.rShift
			&& this.gShift == other.gShift
			&& this.bShift == other.bShift
			&& this.aShift == other.aShift;

		public override bool Equals(object obj)
			=> obj is ColorFormat pf ? this.Equals(pf) : false;

		public override int GetHashCode() {
			unchecked {
				int hash = -490236692;
				hash = hash * -1521134295 + this.rBits;
				hash = hash * -1521134295 + this.gBits;
				hash = hash * -1521134295 + this.bBits;
				hash = hash * -1521134295 + this.aBits;
				hash = hash * -1521134295 + this.rShift;
				hash = hash * -1521134295 + this.gShift;
				hash = hash * -1521134295 + this.bShift;
				hash = hash * -1521134295 + this.aShift;
				return hash;
			}
		}

		public static bool operator ==(ColorFormat format1, ColorFormat format2)
			=> format1.Equals(format2);
		public static bool operator !=(ColorFormat format1, ColorFormat format2)
			=> !format1.Equals(format2);
	}
}
