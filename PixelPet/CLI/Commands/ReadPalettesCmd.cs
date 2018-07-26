﻿using LibPixelPet;
using System;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class ReadPalettesCmd : CliCommand {
		public ReadPalettesCmd()
			: base("Read-Palettes",
				new Parameter("format", "f", false, new ParameterValue("name", nameof(ColorFormat.BGRA8888))),
				new Parameter("palette-number", "pn", false, new ParameterValue("number", "-1")),
				new Parameter("palette-size", "ps", false, new ParameterValue("count", "" + int.MaxValue))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			string fmtName = FindNamedParameter("--format").Values[0].ToString();
			int palNum = FindNamedParameter("--palette-number").Values[0].ToInt32();
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();

			if (palNum < -1) {
				logger?.Log("Invalid palette number.", LogLevel.Error);
				return;
			}
			if (palSize < 1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return;
			}
			if (!(ColorFormat.GetFormat(fmtName) is ColorFormat fmt)) {
				logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
				return;
			}

			TileCutter cutter = new TileCutter(8, 8);
			int ti = 0;
			int addedColors = 0;
			int addedPalettes = 0;
			Palette pal = null;
			foreach (Tile tile in cutter.CutTiles(workbench.Bitmap)) {
				// Grab color from tile.
				int color = tile[0, 0];
				foreach (int otherColor in tile.EnumerateTile().Skip(1)) {
					if (otherColor != color) {
						logger?.Log("Palette tile " + ti + " is not a single color.", LogLevel.Error);
						return;
					}
				}

				// Create new palette if needed.
				if (pal == null) {
					pal = new Palette(fmt, palSize);
				}

				// Add finished palette to palette set.
				pal.Add(color, ColorFormat.BGRA8888);
				addedColors++;

				// Add finished palette to palette set.
				if (pal.Count >= palSize) {
					if (palNum < 0) {
						workbench.PaletteSet.Add(pal);
					} else {
						workbench.PaletteSet.Add(pal, palNum++);
					}
					addedPalettes++;
					pal = null;
				}
			}
			if (pal != null) {
				if (palNum < 0) {
					workbench.PaletteSet.Add(pal);
				} else {
					workbench.PaletteSet.Add(pal, palNum++);
				}
			}

			logger?.Log("Read " + addedPalettes + " palettes with " + addedColors + " colors total.");
		}
	}
}
