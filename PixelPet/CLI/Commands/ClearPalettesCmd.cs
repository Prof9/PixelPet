﻿namespace PixelPet.CLI.Commands {
	internal sealed class ClearPalettesCmd : CLICommand {
		public ClearPalettesCmd()
			: base("Clear-Palettes") { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			workbench.PaletteSet.Clear();

			logger?.Log("Cleared palettes.");
			return true;
		}
	}
}
