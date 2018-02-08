using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class DeserializePaletteCmd : CliCommand {
		private static List<byte[]> ColorTables;

		public DeserializePaletteCmd()
			: base("Deserialize-Palette") {
			if (ColorTables == null) {
				ColorTables = new List<byte[]>() {
					null, null, null, null, null, null, null, null
				};
			}
		}

		protected static byte[] GetColorTable(int bits) {
			if (bits < 0 || bits > 8)
				throw new ArgumentOutOfRangeException(nameof(bits));

			if (ColorTables[bits] == null) {
				// Generate new table for this bit count.
				byte[] table = new byte[1 << bits];
				int max = (1 << bits) - 1;

				for (int i = 0; i < table.Length; i++) {
					table[i] = (byte)Math.Round(((float)i * 255) / max);
				}

				ColorTables[bits] = table;
			}

			return ColorTables[bits];
		}

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Deserializing palette...");

			workbench.Stream.Position = 0;
			workbench.Palette.Clear();

			int rbits = 5;		// red   bits
			int gbits = 5;		// green bits
			int bbits = 5;      // blue  bits
			int abits = 0;      // alpha bits

			int rmask = (1 << rbits) - 1;
			int gmask = (1 << gbits) - 1;
			int bmask = (1 << bbits) - 1;
			int amask = (1 << abits) - 1;

			byte[] rtbl = GetColorTable(rbits);
			byte[] gtbl = GetColorTable(gbits);
			byte[] btbl = GetColorTable(bbits);
			byte[] atbl = GetColorTable(abits);

			// bytes per color
			int bpc = (rbits + gbits + bbits + abits + 7) / 8;		// bytes per color

			byte[] buffer = new byte[4];
			while (workbench.Stream.Read(buffer, 0, bpc) == bpc) {
				int c = BitConverter.ToInt32(buffer, 0);

				// Read RGBA
				int r = rtbl[c & rmask];
				c >>= rbits;
				int g = gtbl[c & gmask];
				c >>= gbits;
				int b = btbl[c & bmask];
				c >>= bbits;
				int a = abits > 0 ? atbl[c & amask] : 255;

				// Convert to 8-bit color
				Color color = Color.FromArgb(a, r, g, b);
				workbench.Palette.Add(color);
			}
		}
	}
}
