using LibPixelPet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class ExtractPalettesCmd : CliCommand {
		public ExtractPalettesCmd()
			: base("Extract-Palettes",
				new Parameter("append", "a", false),
				new Parameter("palette-number", "pn", false, new ParameterValue("number", "-1")),
				new Parameter("palette-size", "ps", false, new ParameterValue("count", "-1")),
				new Parameter("x", "x", false, new ParameterValue("pixels", "0")),
				new Parameter("y", "y", false, new ParameterValue("pixels", "0")),
				new Parameter("width", "w", false, new ParameterValue("pixels", "-1")),
				new Parameter("height", "h", false, new ParameterValue("pixels", "-1")),
				new Parameter("tile-size", "s", false, new ParameterValue("width", "-1"), new ParameterValue("height", "-1"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			bool append = FindNamedParameter("--append").IsPresent;
			int palNum = FindNamedParameter("--palette-number").Values[0].ToInt32();
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();
			int x = FindNamedParameter("--x").Values[0].ToInt32();
			int y = FindNamedParameter("--y").Values[0].ToInt32();
			int w = FindNamedParameter("--width").Values[0].ToInt32();
			int h = FindNamedParameter("--height").Values[0].ToInt32();
			Parameter ts = FindNamedParameter("--tile-size");
			int tw = ts.Values[0].ToInt32();
			int th = ts.Values[1].ToInt32();

			if (palNum < -1) {
				logger?.Log("Invalid palette number.", LogLevel.Error);
				return;
			}
			if (palSize == 0 || palSize < -1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return;
			}
			if (ts.IsPresent && tw < 0) {
				logger?.Log("Invalid tile width.", LogLevel.Error);
				return;
			}
			if (ts.IsPresent && th < 0) {
				logger?.Log("Invalid tile height.", LogLevel.Error);
				return;
			}

			if (!append) {
				workbench.PaletteSet.Clear();
			}

			// Use existing tile size if not specified
			if (tw == -1 && th == -1) {
				tw = workbench.Tileset.TileWidth;
				th = workbench.Tileset.TileHeight;
			}

			int ti = 0;
			int addedPalettes = 0;
			int addedColors = 0;
			Palette pal = null;
			using (Bitmap bmp = workbench.GetCroppedBitmap(x, y, w, h, logger)) {
				// Special case to use entire bitmap
				if (tw == 0) {
					tw = bmp.Width;
				}
				if (th == 0) {
					th = bmp.Height;
				}
				TileCutter cutter = new TileCutter(tw, th);

				foreach (Tile tile in cutter.CutTiles(bmp)) {
					// Get all the unique colors in the tile.
					List<int> tileColors = tile.EnumerateTile().Distinct().ToList();

					if (palSize != -1 && tileColors.Count > palSize) {
						logger?.Log("Tile " + ti + " has " + tileColors.Count + " colors, which is more than the " + palSize + " colors allowed by the palette.", LogLevel.Error);
						return;
					}

					int bestPal = -1;
					int bestAdd = int.MaxValue;

					// Find palettes we can add to.
					for (int i = 0; i < workbench.PaletteSet.Count; i++) {
						pal = workbench.PaletteSet[i].Palette;

						// See if we can add to this palette.
						int newSize = pal.Union(tileColors).Count();
						if ((pal.MaximumSize >= 0 && newSize > pal.MaximumSize)) {
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
						if (palNum >= 0) {
							while (workbench.PaletteSet.ContainsPalette(palNum)) {
								palNum++;
							}
							workbench.PaletteSet.Add(pal, palNum++);
						} else {
							workbench.PaletteSet.Add(pal);
						}

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
			}

			logger?.Log("Added " + addedPalettes + " new palettes, " + addedColors + " colors total.");
		}
	}
}
