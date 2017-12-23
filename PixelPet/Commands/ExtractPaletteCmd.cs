using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class ExtractPaletteCmd : CliCommand {
		public ExtractPaletteCmd() 
			: base("Extract-Palette", new Parameter[] {
				new Parameter("palette-size", "p", false, new ParameterValue("count", "16")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();

			// Load unique colors per tile.
			List<int> colors = new List<int>();
			bool transparent = false;
			foreach (Tile tile in new TileCutter().CutTiles(workbench.Bitmap)) {
				tile.IndexTile();
				foreach (Color color in tile.Palette) {
					int argb = color.ToArgb();

					if ((argb >> 24) == 0) {
						// Add only one transparent color, as the first color.
						if (!transparent) {
							colors.Insert(0, argb);
							transparent = true;
						}
					} else {
						// Add only if a non-transparent version doesn't exist in the current palette.
						if (!colors.Skip(colors.Count / palSize * palSize + (transparent ? 1 : 0)).Take(palSize).Contains(argb)) {
							colors.Add(argb);
						}
					}

					// Reset transparent color for new palette.
					if (colors.Count % palSize == 0) {
						transparent = false;
					}
				}
			}

			foreach (int c in colors) {
				workbench.Palette.Add(Color.FromArgb(c));
			}

			cli.Log("Extracted " + colors.Count + " colors from bitmap.");
		}
	}
}
