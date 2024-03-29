﻿using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal sealed class RenderTilemapCmd : Command {
		public RenderTilemapCmd()
			: base("Render-Tilemap",
				new Parameter(true, new ParameterValue("tiles-per-row")),
				new Parameter(true, new ParameterValue("tiles-per-column"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			int tpr = GetUnnamedParameter(0).Values[0].ToInt32();
			int tpc = GetUnnamedParameter(1).Values[0].ToInt32();

			if (tpr <= 0) {
				logger?.Log("Invalid tile count per row.", LogLevel.Error);
				return false;
			}
			if (tpc <= 0) {
				logger?.Log("Invalid tile count per column.", LogLevel.Error);
				return false;
			}

			workbench.Bitmap = workbench.Tileset.IsIndexed
				? workbench.Tilemap.ToBitmapIndexed(workbench.Tileset, workbench.PaletteSet, tpr, tpc)
				: workbench.Tilemap.ToBitmap(workbench.Tileset, tpr, tpc);
			workbench.BitmapFormat = ColorFormat.ARGB8888;
			return true;
		}
	}
}
