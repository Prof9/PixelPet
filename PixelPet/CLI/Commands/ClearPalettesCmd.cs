﻿using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class ClearPalettesCmd : CliCommand {
		public ClearPalettesCmd()
			: base("Clear-Palettes") { }

		public override void Run(Workbench workbench, ILogger logger) {
			workbench.PaletteSet.Clear();

			logger?.Log("Cleared palettes.");
		}
	}
}
