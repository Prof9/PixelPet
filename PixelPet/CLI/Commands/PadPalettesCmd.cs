using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class PadPalettesCmd : CliCommand {
		public PadPalettesCmd()
			: base("Pad-Palettes",
				new Parameter(true, new ParameterValue("width", "0"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();

			if (width < 1) {
				logger?.Log("Invalid palette width.", LogLevel.Error);
				return;
			}

			int addedColors = 0;
			foreach (PaletteEntry pe in workbench.PaletteSet) {
				while (pe.Palette.Count % width != 0) {
					pe.Palette.Add(0);
					addedColors++;
				}
			}

			logger?.Log("Padded palettes to width " + width + " (added " + addedColors + " colors).", LogLevel.Information);
		}
	}
}
