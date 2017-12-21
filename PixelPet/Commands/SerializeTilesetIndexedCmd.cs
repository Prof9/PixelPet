using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class SerializeTilesetIndexed : CliCommand {
		public SerializeTilesetIndexed()
			: base("Serialize-Tileset-Indexed") { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Serializing tileset...");

			if (workbench.Palette.Count == 0) {
				cli.Log("WARNING: No palette loaded.");
			}

			int bpp = 4;        // bits per pixel

			int ppb = 8 / bpp;  // pixels per byte
			int pi = 0;         // pixel index in current byte
			int b = 0;          // current byte

			workbench.Stream.SetLength(0);

			bool foundUnmatchedColor = false;

			int t = 0;
			foreach (Tile tile in workbench.Tileset.Tiles) {
				for (int ty = 0; ty < tile.Height; ty++) {
					for (int tx = 0; tx < tile.Width; tx++) {
						int argb = tile.GetPixel(tx, ty);
						int c = FindPaletteIndex(workbench.Palette, argb);
						if (c < 0) {
							c = 0;
							if (!foundUnmatchedColor) {
								cli.Log("WARNING: Found unmatched color 0x" + argb.ToString("X8", CultureInfo.CurrentCulture) + " in tile " + t + " at (" + tx+ ", " + ty + ").");
								foundUnmatchedColor = true;
							}
						}

						// Add pixel to current byte.
						b |= (c << (pi * bpp));
						if (++pi == ppb) {
							// Write byte to stream if it's finished.
							workbench.Stream.WriteByte((byte)b);
							b = 0;
							pi = 0;
						}

					}
				}
				t++;
			}

			int FindPaletteIndex(IList<Color> palette, int argb) {
				// Set to totally transparent color, if it exists.
				int a = argb >> 24;
				if (a == 0 && palette[0].A == 0) {
					return 0;
				}

				// Find matching color.
				for (int i = 0; i < palette.Count; i++) {
					if (palette[i].ToArgb() == argb) {
						return i;
					}
				}

				// No matching color found.
				return -1;
			}
		}
	}
}
