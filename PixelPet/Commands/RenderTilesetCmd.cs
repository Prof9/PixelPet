using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class RenderTilesetCmd : CliCommand {
		public RenderTilesetCmd()
			: base("Render-Tileset",
				new Parameter("tiles-per-row", "tw", false, new ParameterValue("count", "32"))
			) { }

		public override void Run(Workbench workbench, Cli cli) {
			int maxTilesPerRow = FindNamedParameter("--tiles-per-row").Values[0].ToInt32();

			workbench.SetBitmap(workbench.Tileset.ToBitmap(maxTilesPerRow));
		}
	}
}
