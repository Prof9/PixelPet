using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class RenderTilesetCmd : CliCommand {
		public RenderTilesetCmd()
			: base("Render-Tileset") { }

		public override void Run(Workbench workbench, Cli cli) {
			int maxTilesPerRow = 0;

			workbench.SetBitmap(workbench.Tileset.ToBitmap(maxTilesPerRow));
		}
	}
}
