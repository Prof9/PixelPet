using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class ClearTilesetCmd : CliCommand {
		public ClearTilesetCmd()
			: base("Clear-Tileset") { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Clearing tileset...");

			workbench.Tileset = new Tileset(8, 8);
		}
	}
}
