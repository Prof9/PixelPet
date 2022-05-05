using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class RenderTilemapCmd : CliCommand {
		public RenderTilemapCmd()
			: base("Render-Tilemap",
				new Parameter(true, new ParameterValue("tiles-per-row")),
				new Parameter(true, new ParameterValue("tiles-per-column"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			int tpr = FindUnnamedParameter(0).Values[0].ToInt32();
			int tpc = FindUnnamedParameter(1).Values[0].ToInt32();

			if (tpr <= 0) {
				logger?.Log("Invalid tile count per row.", LogLevel.Error);
				return false;
			}
			if (tpc <= 0) {
				logger?.Log("Invalid tile count per column.", LogLevel.Error);
				return false;
			}

			workbench.SetBitmap(workbench.Tileset.IsIndexed
				? workbench.Tilemap.ToBitmapIndexed(workbench.Tileset, workbench.PaletteSet, tpr, tpc)
				: workbench.Tilemap.ToBitmap(workbench.Tileset, tpr, tpc),
				ColorFormat.ARGB8888
			);
			return true;
		}
	}
}
