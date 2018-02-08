using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class DeserializeTilesetIndexedCmd : CliCommand {
		public DeserializeTilesetIndexedCmd()
			: base("Deserialize-Tileset-Indexed", new Parameter[] {
				new Parameter("palette",        "p",   false, new ParameterValue("index", "0")),
				new Parameter("palette-size",   "ps",  false, new ParameterValue("size",  "16")),
				new Parameter("bits-per-pixel", "bpp", false, new ParameterValue("count", "4")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Deserializing tileset...");

			if (workbench.Palette.Count == 0) {
				cli.Log("WARNING: No palette loaded.");
			}

			workbench.Stream.Position = 0;

			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();
			int palNum  = FindNamedParameter("--palette").Values[0].ToInt32();
			int bpp     = FindNamedParameter("--bits-per-pixel").Values[0].ToInt32();
			int tw = 8;					// tile width
			int th = 8;                 // tile height
			bool reduce = false;        // to remove duplicates from tileset
			
			int ppb = 8 / bpp;          // pixels per byte
			int bpt = (tw * th) / ppb;  // bytes per tile
			int pmask = (1 << bpp) - 1;	// mask per pixel
			int b;						// current byte
			int pi;						// pixel index in current byte
			int c;                      // current pixel
			int palStart = palSize * palNum;
			Color color;

			bool foundOutOfRangeColor = false;

			workbench.Tileset = new Tileset(8, 8);
			byte[] buffer = new byte[bpt];
			int[] pixels = new int[tw * th];
			while (workbench.Stream.Read(buffer, 0, bpt) == bpt) {
				// Process all bytes.
				for (int bi = 0; bi < bpt; bi++) {
					// Get current byte.
					b = buffer[bi];

					// Extract colors from byte.
					for (pi = 0; pi < ppb; pi++) {
						c = (b & pmask) + palStart;

						// Check if color in palette.
						if (c < workbench.Palette.Count) {
							color = workbench.Palette[c];
						} else {
							color = Color.Black;

							if (!foundOutOfRangeColor) {
								cli.Log("WARNING: Palette does not contain color " + c + ".");
								foundOutOfRangeColor = true;
							}
						}

						pixels[bi * ppb + pi] = color.ToArgb();
					}
				}

				// Add tile to the tileset.
				Tile tile = new Tile(tw, th);
				tile.SetAllPixels(pixels);
				if (reduce) {
					workbench.Tileset.FindOrAddTile(tile);
				} else {
					workbench.Tileset.AddTile(tile);
				}
			}
		}
	}
}
