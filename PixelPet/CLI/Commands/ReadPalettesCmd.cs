using LibPixelPet;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal sealed class ReadPalettesCmd : CliCommand {
		public ReadPalettesCmd()
			: base("Read-Palettes",
				new Parameter("append", "a", false),
				new Parameter("palette-number", "pn", false, new ParameterValue("number", "-1")),
				new Parameter("palette-size", "ps", false, new ParameterValue("count", "-1")),
				new Parameter("x", "x", false, new ParameterValue("pixels", "0")),
				new Parameter("y", "y", false, new ParameterValue("pixels", "0")),
				new Parameter("width", "w", false, new ParameterValue("pixels", "-1")),
				new Parameter("height", "h", false, new ParameterValue("pixels", "-1"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			bool append = FindNamedParameter("--append").IsPresent;
			int palNum = FindNamedParameter("--palette-number").Values[0].ToInt32();
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();
			int x = FindNamedParameter("--x").Values[0].ToInt32();
			int y = FindNamedParameter("--y").Values[0].ToInt32();
			int w = FindNamedParameter("--width").Values[0].ToInt32();
			int h = FindNamedParameter("--height").Values[0].ToInt32();

			if (palNum < -1) {
				logger?.Log("Invalid palette number.", LogLevel.Error);
				return false;
			}
			if (palSize == 0 || palSize < -1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return false;
			}

			if (!append) {
				workbench.PaletteSet.Clear();
			}

			TileCutter cutter = new(8, 8);
			int ti = 0;
			int addedColors = 0;
			int addedPalettes = 0;
			Palette pal = null;
			Bitmap bmp = workbench.Bitmap.GetCroppedBitmap(x, y, w, h);

			foreach (Tile tile in cutter.CutTiles(bmp)) {
				// Grab color from tile.
				int color = tile[0, 0];
				foreach (int otherColor in tile.EnumerateTile().Skip(1)) {
					if (otherColor != color) {
						logger?.Log($"Palette tile {ti} is not a single color.", LogLevel.Error);
						return false;
					}
				}

				// Create new palette if needed.
				pal ??= new Palette(workbench.BitmapFormat, palSize);

				// Add finished palette to palette set.
				pal.Add(color, workbench.BitmapFormat);
				addedColors++;

				// Add finished palette to palette set.
				if (palSize != -1 && pal.Count >= palSize) {
					if (palNum >= 0) {
						while (workbench.PaletteSet.ContainsPalette(palNum)) {
							palNum++;
						}
						workbench.PaletteSet.Add(pal, palNum++);
					} else {
						workbench.PaletteSet.Add(pal);
					}
					addedPalettes++;
					pal = null;
				}

				ti++;
			}

			// Finish up remaining palette
			if (pal is not null) {
				if (palNum >= 0) {
					while (workbench.PaletteSet.ContainsPalette(palNum)) {
						palNum++;
					}
					workbench.PaletteSet.Add(pal, palNum++);
				} else {
					workbench.PaletteSet.Add(pal);
				}
				addedPalettes++;
			}

			logger?.Log($"Read {addedPalettes} palettes with {addedColors} colors total.");
			return true;
		}
	}
}
