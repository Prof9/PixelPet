using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class RenderTilesetCmd : CliCommand {
		public RenderTilesetCmd()
			: base("Render-Tileset",
				new Parameter("tiles-per-row", "tw", false, new ParameterValue("count", "32"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			int maxTilesPerRow = FindNamedParameter("--tiles-per-row").Values[0].ToInt32();

			if (maxTilesPerRow < 0) {
				logger?.Log("Invalid tile count per row.", LogLevel.Error);
				return;
			}

			workbench.SetBitmap(workbench.Tileset.IsIndexed
				? workbench.Tileset.ToBitmapIndexed(maxTilesPerRow, workbench.PaletteSet)
				: workbench.Tileset.ToBitmap(maxTilesPerRow)
			);
		}
	}
}
