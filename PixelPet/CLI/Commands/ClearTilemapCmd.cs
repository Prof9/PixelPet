using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class ClearTilemapCmd : CliCommand {
		public ClearTilemapCmd()
			: base("Clear-Tilemap") { }

		protected override void Run(Workbench workbench, ILogger logger) {
			workbench.Tilemap.Clear();

			logger?.Log("Cleared tilemap.");
		}
	}
}
