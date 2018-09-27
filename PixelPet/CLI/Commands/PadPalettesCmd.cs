using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class PadPalettesCmd : CliCommand {
		public PadPalettesCmd()
			: base("Pad-Palettes",
				new Parameter(true, new ParameterValue("width", "0")),
				new Parameter("color", "c", false, new ParameterValue("value", "0"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();
			int color = FindNamedParameter("--color").Values[0].ToInt32();

			if (width < 1) {
				logger?.Log("Invalid palette width.", LogLevel.Error);
				return;
			}

			if (workbench.PaletteSet.Count == 0) {
				logger?.Log("No palettes to pad. Creating 1 palette based on current bitmap format.", LogLevel.Information);
				Palette pal = new Palette(workbench.BitmapFormat, -1);
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
