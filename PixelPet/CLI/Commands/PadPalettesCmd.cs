﻿using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class PadPalettesCmd : CliCommand {
		public PadPalettesCmd()
			: base("Pad-Palettes",
				new Parameter(true, new ParameterValue("width", "0")),
				new Parameter("color", "c", false, new ParameterValue("value", "0")),
				new Parameter("palette-size", "ps", false, new ParameterValue("count", "-1"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();
			int color = FindNamedParameter("--color").Values[0].ToInt32();
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();

			if (width < 1) {
				logger?.Log("Invalid palette width.", LogLevel.Error);
				return;
			}
			if (palSize == 0 || palSize < -1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return;
			}

			if (workbench.PaletteSet.Count == 0) {
				logger?.Log("No palettes to pad. Creating 1 " + (palSize == -1 ? "unbounded palette" : ("palette with max size " + palSize)) + " based on current bitmap format.", LogLevel.Information);
				Palette pal = new Palette(workbench.BitmapFormat, palSize);
				workbench.PaletteSet.Add(pal);
			}

			int addedColors = 0;
			foreach (PaletteEntry pe in workbench.PaletteSet) {
				while (pe.Palette.Count < width) {
					pe.Palette.Add(color);
					addedColors++;
				}
			}

			logger?.Log("Padded palettes to width " + width + " (added " + addedColors + " colors).", LogLevel.Information);
		}
	}
}
