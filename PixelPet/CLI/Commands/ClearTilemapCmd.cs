﻿namespace PixelPet.CLI.Commands {
	internal sealed class ClearTilemapCmd : Command {
		public ClearTilemapCmd()
			: base("Clear-Tilemap") { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			workbench.Tilemap.Clear();

			logger?.Log("Cleared tilemap.");
			return true;
		}
	}
}
