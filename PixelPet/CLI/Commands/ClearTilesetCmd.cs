using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class ClearTilesetCmd : CliCommand {
		public ClearTilesetCmd()
			: base("Clear-Tileset") { }

		public override void Run(Workbench workbench, ILogger logger) {
			int w = 8;
			int h = 8;

			workbench.Tileset = new Tileset(w, h);

			logger?.Log("Created new " + w + 'x' + h + " tileset.");
		}
	}
}
