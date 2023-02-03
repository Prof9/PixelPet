using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LibPixelPet {
	public static class NumberParser {
		private static readonly IList<char> posSignChars = new char[] { '+' };
		private static readonly IList<char> negSignChars = new char[] { '−', '-' };

		private record struct NumberInfo(int ParseStart, int ParseEnd, int Radix, int Sign) {
			public int ParseLength => ParseEnd - ParseStart;
		}

		/// <summary>
		/// Determines the sign, most likely radix, and parse substring of a number string.
		/// </summary>
		/// <param name="number">A string representing the number.</param>
		/// <returns>NumberInfo containing the sign, radix and substring boundaries. If the radix is -1, the radix could not be determined.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static NumberInfo GetNumberInfo(in string number) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");

			NumberInfo info = new NumberInfo(0, number.Length, 10, 1);
			if (number.Length <= 0) {
				return info;
			}

			// Remove the sign for convenience.
			if (posSignChars.Contains(number[info.ParseStart])) {
				info.ParseStart += 1;
			} else if (negSignChars.Contains(number[info.ParseStart])) {
				info.ParseStart += 1;
				info.Sign = -1;
			}

			char pos0 = char.ToUpperInvariant(number[info.ParseStart]);
			char? pos1 = (info.ParseEnd - info.ParseStart) > 1 ? (char?)char.ToUpperInvariant(number[info.ParseStart + 1]) : (char?)null;
			char end1 = char.ToUpperInvariant(number[info.ParseEnd - 1]);
			string start3 = null;
			string end3 = null;
			if (info.ParseEnd - info.ParseStart >= 3) {
				start3 = number.Substring(info.ParseStart, 3).ToUpperInvariant();
				end3 = number.Substring(info.ParseEnd - 3).ToUpperInvariant();
			}

			if (pos0 == 'X' || pos0 == '$' || pos0 == '#') {
				info.Radix = 16;
				info.ParseStart += 1;
			} else if (pos1 == 'X') {
				info.Radix = 16;
				info.ParseStart += 2;
			} else if (start3 == "HEX") {
				info.Radix = 16;
				info.ParseStart += 3;
			} else if (end3 == "HEX") {
				info.Radix = 16;
				info.ParseEnd -= 3;
			} else if (start3 == "OCT") {
				info.Radix = 8;
				info.ParseStart += 3;
			} else if (end3 == "OCT") {
				info.Radix = 8;
				info.ParseEnd -= 3;
			} else if (pos0 == 'O') {
				info.Radix = 8;
				info.ParseStart += 1;
			} else if (pos0 == 'O') {
				info.Radix = 8;
				info.ParseStart += 2;
			} else if (end1 == 'O') {
				info.Radix = 8;
				info.ParseEnd -= 1;
			} else if (start3 == "BIN") {
				info.Radix = 2;
				info.ParseStart += 3;
			} else if (end3 == "BIN") {
				info.Radix = 2;
				info.ParseEnd -= 3;
			} else if (pos1 == 'H') {
				info.Radix = 16;
				info.ParseStart += 2;
			} else if (end1 == 'H') {
				info.Radix = 16;
				info.ParseEnd -= 2;
			} else if (start3 == "DEC") {
				info.Radix = 10;
				info.ParseStart += 3;
			} else if (end3 == "DEC") {
				info.Radix = 10;
				info.ParseEnd -= 3;
			} else if (pos0 == 'D') {
				info.Radix = 10;
				info.ParseStart += 1;
			} else if (pos1 == 'D') {
				info.Radix = 10;
				info.ParseStart += 2;
			} else if (end1 == 'D') {
				info.Radix = 10;
				info.ParseEnd -= 1;
			} else if (pos0 == 'B') {
				info.Radix = 2;
				info.ParseStart += 1;
			} else if (pos1 == 'B') {
				info.Radix = 2;
				info.ParseStart += 2;
			} else if (end1 == 'B') {
				info.Radix = 2;
				info.ParseEnd -= 1;
			}

			// Check if the number is a valid number for the detected radix.
			// This needs to be as fast as possible.
			int numberMax = '0' + info.Radix - 1;
			int lowerMax = 'a' + info.Radix - 1;
			int upperMax = 'A' + info.Radix - 1;
			int i = info.ParseStart;
			char c;
			while (i < info.ParseEnd) {
				c = number[i++];
				if (!((c >= '0' && c <= '9' && c <= numberMax)
					|| (info.Radix > 10 && (c >= 'A' && c <= 'Z' && c <= upperMax) || (c >= 'a' && c <= 'z' && c <= lowerMax))
				)) {
					info.Radix = -1;
					break;
				}
			}

			return info;
		}

		/// <summary>
		/// Converts the string representation of a number to its 16-bit signed integer equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>A 16-bit signed integer equivalent to the number contained in s.</returns>
		public static short ParseInt16(in string number) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			NumberInfo info = GetNumberInfo(number);
			return (short)(Convert.ToInt16(number.Substring(info.ParseStart, info.ParseLength), info.Radix) * info.Sign);
		}

		/// <summary>
		/// Converts the string representation of a number to its 32-bit signed integer equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>A 32-bit signed integer equivalent to the number contained in s.</returns>
		public static int ParseInt32(in string number) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			NumberInfo info = GetNumberInfo(number);
			return Convert.ToInt32(number.Substring(info.ParseStart, info.ParseLength), info.Radix) * info.Sign;
		}

		/// <summary>
		/// Converts the string representation of a number to its 64-bit signed integer equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>A 64-bit signed integer equivalent to the number contained in s.</returns>
		public static long ParseInt64(in string number) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			NumberInfo info = GetNumberInfo(number);
			return Convert.ToInt64(number.Substring(info.ParseStart, info.ParseLength), info.Radix) * info.Sign;
		}

		/// <summary>
		/// Converts the string representation of a number to its unsigned byte equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>An unsigned byte equivalent to the number contained in s.</returns>
		public static byte ParseByte(in string number) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			NumberInfo info = GetNumberInfo(number);
			if (info.Sign != 1) {
				throw new ArgumentOutOfRangeException(nameof(number), number, "Unsigned byte cannot be negative.");
			}
			return Convert.ToByte(number.Substring(info.ParseStart, info.ParseLength), info.Radix);
		}

		/// <summary>
		/// Converts the string representation of a number to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <param name="output">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in s, if the conversion succeeded, or zero if the conversion failed.</param>
		/// <returns>true if the number string was converted successfully; otherwise, false.</returns>
		public static bool TryParseInt16(in string number, out short output) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			NumberInfo info = GetNumberInfo(number);

			if (info.Radix == -1) {
				return false;
			}

			try {
				output = (short)(Convert.ToInt16(number.Substring(info.ParseStart, info.ParseLength), info.Radix) * info.Sign);
				return true;
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is FormatException || ex is OverflowException) {
					return false;
				} else {
					throw;
				}
			}
		}

		/// <summary>
		/// Converts the string representation of a number to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <param name="output">When this method returns, contains the 32-bit signed integer value equivalent to the number contained in s, if the conversion succeeded, or zero if the conversion failed.</param>
		/// <returns>true if the number string was converted successfully; otherwise, false.</returns>
		public static bool TryParseInt32(in string number, out int output) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			NumberInfo info = GetNumberInfo(number);

			if (info.Radix == -1) {
				return false;
			}

			try {
				output = Convert.ToInt32(number.Substring(info.ParseStart, info.ParseLength), info.Radix) * info.Sign;
				return true;
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is FormatException || ex is OverflowException) {
					return false;
				} else {
					throw;
				}
			}
		}

		/// <summary>
		/// Converts the string representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <param name="output">When this method returns, contains the 64-bit signed integer value equivalent to the number contained in s, if the conversion succeeded, or zero if the conversion failed.</param>
		/// <returns>true if the number string was converted successfully; otherwise, false.</returns>
		public static bool TryParseInt64(in string number, out long output) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			NumberInfo info = GetNumberInfo(number);

			if (info.Radix == -1) {
				return false;
			}

			try {
				output = Convert.ToInt64(number.Substring(info.ParseStart, info.ParseLength), info.Radix) * info.Sign;
				return true;
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is FormatException || ex is OverflowException) {
					return false;
				} else {
					throw;
				}
			}
		}

		/// <summary>
		/// Converts the string representation of a number to its unsigned byte equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <param name="output">When this method returns, contains the unsigned byte value equivalent to the number contained in s, if the conversion succeeded, or zero if the conversion failed.</param>
		/// <returns>true if the number string was converted successfully; otherwise, false.</returns>
		public static bool TryParseByte(in string number, out byte output) {
			if (number is null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			NumberInfo info = GetNumberInfo(number);

			if (info.Radix == -1 || info.Sign != 1) {
				return false;
			}

			try {
				output = Convert.ToByte(number.Substring(info.ParseStart, info.ParseLength), info.Radix);
				return true;
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is FormatException || ex is OverflowException) {
					return false;
				} else {
					throw;
				}
			}
		}

		/// <summary>
		/// Parses the given hex string as an enumerable of bytes.
		/// </summary>
		/// <param name="hex">The hex string to parse.</param>
		/// <returns></returns>
		public static IEnumerable<byte> ParseHexString(in string hex) {
			if (hex is null)
				throw new ArgumentNullException(nameof(hex), "The hex string cannot be null.");

			string trimmed = Regex.Replace(hex, @"[\s-]", "");
			if (trimmed.Length % 2 != 0)
				throw new ArgumentException("The length of the hex string is not a multiple of two.", nameof(hex));

			byte[] bytes = new byte[trimmed.Length / 2];
			for (int i = 0; i < bytes.Length; i++) {
				bytes[i] = Convert.ToByte(trimmed.Substring(i * 2, 2), 16);
			}

			return bytes;
		}

		/// <summary>
		/// Parses the given hex string as an enumerable of bytes. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="hex">The hex string to parse.</param>
		/// <param name="output">When this method returns, contains the enumerable of bytes parsed from the hex string.</param>
		/// <returns>true if the hex string was parsed successfully; otherwise, false.</returns>
		public static bool TryParseHexString(string hex, out IEnumerable<byte> output) {
			if (hex is null)
				throw new ArgumentNullException(nameof(hex), "The hex string cannot be null.");

			try {
				output = ParseHexString(hex);
				return true;
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is FormatException || ex is OverflowException) {
					output = new byte[0];
					return false;
				} else {
					throw;
				}
			}
		}
	}
}
