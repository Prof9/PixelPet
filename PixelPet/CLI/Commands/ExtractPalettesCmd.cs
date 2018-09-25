using LibPixelPet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class ExtractPalettesCmd : CliCommand {
		public ExtractPalettesCmd()
			: base("Extract-Palettes",
				  new Parameter("palette-size", "ps", false, new ParameterValue("count", "16"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();

			if (palSize < 1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return;
			}

			TileCutter cutter = new TileCutter(8, 8);

			int ti = 0;
			int addedPalettes = 0;
			int addedColors = 0;
			Palette pal = null;
			foreach (Tile tile in cutter.CutTiles(workbench.Bitmap)) {
				// Get all the unique colors in the tile.
				List<int> tileColors = tile.EnumerateTile().Distinct().ToList();

				if (tileColors.Count > palSize) {
					logger?.Log("Palette tile " + ti + " has more than " + palSize + " colors.", LogLevel.Error);
					return;
				}

				int bestPal = -1;
				int bestAdd = int.MaxValue;

				// Find palettes we can add to.
				for (int i = 0; i < workbench.PaletteSet.Count; i++) {
					pal = workbench.PaletteSet[i].Palette;

					// See if we can add to this palette.
					int newSize = pal.Union(tileColors).Count();
					if ((pal.MaximumSize >= 0 && newSize > pal.MaximumSize) || newSize > palSize) {
						// Cannot add to this palette.
						continue;
					}

					// Add to palette that would result in least new colors.
					int add = newSize - pal.Count;
					if (add < bestAdd) {
						bestPal = i;
						bestAdd = add;
					}
				}

				// Could not find a suitable palette, so have to make a new one.
				if (bestPal < 0) {
					pal = new Palette(workbench.BitmapFormat, palSize);
					workbench.PaletteSet.Add(pal);

					addedPalettes++;

					bestPal = workbench.PaletteSet.Count - 1;
					bestAdd = tileColors.Count;
				} else {
					pal = workbench.PaletteSet[bestPal].Palette;
				}

				// Add the new colors to the palette.
				foreach (int color in tileColors.Where(c => !pal.Contains(c))) {
					pal.Add(color);
					addedColors++;
				}

				ti++;
			}

			logger?.Log("Added " + addedPalettes + " new palettes, " + addedColors + " colors total.");
		}
	}
}
