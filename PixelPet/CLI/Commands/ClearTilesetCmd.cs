using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class ClearTilesetCmd : CliCommand {
		public ClearTilesetCmd()
			: base("Clear-Tileset",
				new Parameter("tile-size", "s", false, new ParameterValue("width", "8"), new ParameterValue("height", "8"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			int tw = FindNamedParameter("--tile-size").Values[0].ToInt32();
			int th = FindNamedParameter("--tile-size").Values[1].ToInt32();

			workbench.Tileset = new Tileset(tw, th);

			logger?.Log("Created new " + tw + 'x' + th + " tileset.");
		}
	}
}
