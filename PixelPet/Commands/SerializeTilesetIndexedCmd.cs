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

			int palSize = 16;
			int bpp = 4;        // bits per pixel

			int ppb = 8 / bpp;  // pixels per byte
			int pi = 0;         // pixel index in current byte
			int b = 0;          // current byte

			workbench.Stream.SetLength(0);

			bool foundUnmatchedColor = false;

			int t = 0;
			int lastPal = 0;
			foreach (Tile tile in workbench.Tileset.Tiles) {
				if (!tile.IsIndexed) {
					int palIndex = tile.IndexTileAny(workbench.Palette, palSize, lastPal);
					if (palIndex != -1) {
						lastPal = palIndex;
					} else {
						// Couldn't index this tile with any of the palettes.
						// Index it separately.
						lastPal = 0;
						tile.IndexTile();

						if (!foundUnmatchedColor) {
							cli.Log("WARNING: Could not match tile " + t + " to a palette.");
							foundUnmatchedColor = true;
						}
					}
				}

				foreach (int c in tile.EnumerateTileIndexed()) {
					// Add pixel to current byte.
					b |= (c << (pi * bpp));
					if (++pi == ppb) {
						// Write byte to stream if it's finished.
						workbench.Stream.WriteByte((byte)b);
						b = 0;
						pi = 0;
					}
				}
				t++;
			}
		}
	}
}
