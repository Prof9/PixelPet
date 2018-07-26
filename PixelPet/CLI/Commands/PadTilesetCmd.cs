using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class PadTilesetCmd : CliCommand {
		public PadTilesetCmd()
			: base("Pad-Tileset",
				new Parameter(true, new ParameterValue("width"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();

			if (width < 1) {
				logger?.Log("Invalid tileset width.", LogLevel.Error);
				return;
			}

			int tw = workbench.Tileset.TileWidth;
			int th = workbench.Tileset.TileHeight;

			int addedTiles = 0;
			while (workbench.Tileset.Count % width != 0) {
				workbench.Tileset.AddTile(new Tile(tw, th), false, false);
				addedTiles++;
			}

			logger?.Log("Padded tileset to width " + width + " (added " + addedTiles + " tiles).", LogLevel.Information);
		}
	}
}
