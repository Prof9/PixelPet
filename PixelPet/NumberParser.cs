using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelPet {
	public static class NumberParser {
		private static readonly List<char> _signChars = new List<char>(new char[] { '+', '−', '-' });

		/// <summary>
		/// Determines whether the specified number string contains a radix identifier.
		/// </summary>
		/// <param name="number">The number string to check.</param>
		/// <returns>true if the number string contains a radix identifier; otherwise, false.</returns>
		public static bool HasRadix(string number) {
			return GetRadix(ref number, -1) == -1;
		}

		/// <summary>
		/// Determines the most likely radix of a number string, removing radix identifiers from the number string in the process. Assumes a decimal number if no radix was found.
		/// </summary>
		/// <param name="s">A string representing the number.</param>
		/// <returns>The radix of the number.</returns>
		private static int GetRadix(ref string s) {
			return GetRadix(ref s, 10);
		}

		/// <summary>
		/// Determines the most likely radix of a number string, removing radix identifiers from the number string in the process.
		/// </summary>
		/// <param name="number">A string representing the number.</param>
		/// <param name="standard">The default radix to use, if no radix identifier was found.</param>
		/// <returns>The radix of the number, or -1 if the radix could not be determined.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static int GetRadix(ref string number, int standard) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");

			int radix = standard;
			number = number.Trim();

			if (number.Length <= 0) {
				return radix;
			}

			// Remove the sign for convenience.
			char sign = (char)0;
			if (_signChars.Contains(number[0])) {
				sign = number[0];
				number = number.Substring(1);
			}

			char? pos1 = number.Length > 1 ? (char?)char.ToUpperInvariant(number[1]) : (char?)null;
			char end1 = char.ToUpperInvariant(number[number.Length - 1]);
			string start3 = null;
			string end3 = null;
			if (number.Length >= 3) {
				start3 = number.Substring(0, 3).ToUpperInvariant();
				end3 = number.Substring(number.Length - 3).ToUpperInvariant();
			}

			if (number[0] == 'X' || number[0] == '$' || number[0] == '#') {
				radix = 16;
				number = number.Substring(1);
			} else if (pos1 == 'X') {
				radix = 16;
				number = number.Substring(2);
			} else if (start3 == "HEX") {
				radix = 16;
				number = number.Substring(3);
			} else if (end3 == "HEX") {
				radix = 16;
				number = number.Remove(number.Length - 3);
			} else if (start3 == "OCT") {
				radix = 8;
				number = number.Substring(3);
			} else if (end3 == "OCT") {
				radix = 8;
				number = number.Remove(number.Length - 3);
			} else if (number[0] == 'O') {
				radix = 8;
				number = number.Substring(1);
			} else if (pos1 == 'O') {
				radix = 8;
				number = number.Substring(2);
			} else if (end1 == 'O') {
				radix = 8;
				number = number.Remove(number.Length - 1);
			} else if (start3 == "BIN") {
				radix = 2;
				number = number.Substring(3);
			} else if (end3 == "BIN") {
				radix = 2;
				number = number.Remove(number.Length - 3);
			} else if (pos1 == 'H') {
				radix = 16;
				number = number.Substring(2);
			} else if (end1 == 'H') {
				radix = 16;
				number = number.Remove(number.Length - 1);
			} else if (start3 == "DEC") {
				radix = 10;
				number = number.Substring(3);
			} else if (end3 == "DEC") {
				radix = 10;
				number = number.Remove(number.Length - 3);
			} else if (number[0] == 'D') {
				radix = 10;
				number = number.Substring(1);
			} else if (pos1 == 'D') {
				radix = 10;
				number = number.Substring(2);
			} else if (end1 == 'D') {
				radix = 10;
				number = number.Remove(number.Length - 1);
			} else if (number[0] == 'B') {
				radix = 2;
				number = number.Substring(1);
			} else if (pos1 == 'B') {
				radix = 2;
				number = number.Substring(2);
			} else if (end1 == 'B') {
				radix = 2;
				number = number.Remove(number.Length - 1);
			}

			// Check if the number is a valid number for the detected radix.
			// This needs to be as fast as possible.
			int numberMax = '0' + radix - 1;
			int lowerMax = 'a' + radix - 1;
			int upperMax = 'A' + radix - 1;
			foreach (char c in number) {
				if (!((c >= '0' && c <= '9' && c <= numberMax)
					|| (radix > 10 && (c >= 'A' && c <= 'Z' && c <= upperMax) || (c >= 'a' && c <= 'z' && c <= lowerMax))
				)) {
					radix = -1;
					break;
				}
			}

			// Put the sign back.
			if (sign != (char)0) {
				number = sign + number;
			}

			return radix;
		}

		/// <summary>
		/// Converts the string representation of a number to its 16-bit signed integer equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>A 16-bit signed integer equivalent to the number contained in s.</returns>
		public static short ParseInt16(string number) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			int radix = GetRadix(ref number);
			return Convert.ToInt16(number, radix);
		}

		/// <summary>
		/// Converts the string representation of a number to its 32-bit signed integer equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>A 32-bit signed integer equivalent to the number contained in s.</returns>
		public static int ParseInt32(string number) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			int radix = GetRadix(ref number);
			return Convert.ToInt32(number, radix);
		}

		/// <summary>
		/// Converts the string representation of a number to its 64-bit signed integer equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>A 64-bit signed integer equivalent to the number contained in s.</returns>
		public static long ParseInt64(string number) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			int radix = GetRadix(ref number);
			return Convert.ToInt64(number, radix);
		}

		/// <summary>
		/// Converts the string representation of a number to its unsigned byte equivalent.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <returns>An unsigned byte equivalent to the number contained in s.</returns>
		public static byte ParseByte(string number) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			int radix = GetRadix(ref number);
			return Convert.ToByte(number, radix);
		}

		/// <summary>
		/// Converts the string representation of a number to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="number">A string containing a number to convert.</param>
		/// <param name="output">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in s, if the conversion succeeded, or zero if the conversion failed.</param>
		/// <returns>true if the number string was converted successfully; otherwise, false.</returns>
		public static bool TryParseInt16(string number, out short output) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			int radix = GetRadix(ref number);

			if (radix == -1) {
				return false;
			}

			try {
				output = Convert.ToInt16(number, radix);
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
		public static bool TryParseInt32(string number, out int output) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			int radix = GetRadix(ref number);

			if (radix == -1) {
				return false;
			}

			try {
				output = Convert.ToInt32(number, radix);
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
		public static bool TryParseInt64(string number, out long output) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			int radix = GetRadix(ref number);

			if (radix == -1) {
				return false;
			}

			try {
				output = Convert.ToInt64(number, radix);
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
		public static bool TryParseByte(string number, out byte output) {
			if (number == null)
				throw new ArgumentNullException(nameof(number), "The number string cannot be null.");
			if (number.Length <= 0)
				throw new ArgumentOutOfRangeException(nameof(number), number, "The number string cannot be empty.");

			output = 0;
			int radix = GetRadix(ref number);

			if (radix == -1) {
				return false;
			}

			try {
				output = Convert.ToByte(number, radix);
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
		public static IEnumerable<byte> ParseHexString(string hex) {
			if (hex == null)
				throw new ArgumentNullException(nameof(hex), "The hex string cannot be null.");

			hex = Regex.Replace(hex, @"[\s-]", "");
			if (hex.Length % 2 != 0)
				throw new ArgumentException("The length of the hex string is not a multiple of two.", nameof(hex));

			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; i++) {
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
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
			if (hex == null)
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
