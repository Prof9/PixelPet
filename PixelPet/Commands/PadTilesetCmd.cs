using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class PadTilesetCmd : CliCommand {
		public PadTilesetCmd()
			: base("Pad-Tileset", new Parameter[] {
				new Parameter(true, new ParameterValue("width", "0")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();

			cli.Log("Padding tileset...");

			if (width <= 0) {
				return;
			}

			int tw = workbench.Tileset.TileWidth;
			int th = workbench.Tileset.TileHeight;
			List<int> emptyTile = Enumerable.Repeat(Color.Transparent.ToArgb(), th * tw).ToList();

			while (workbench.Tileset.TileCount % width != 0) {
				Tile tile = new Tile(workbench.Tileset.TileWidth, workbench.Tileset.TileHeight);
				tile.SetAllPixels(emptyTile);
				workbench.Tileset.AddTile(tile);
			}
		}
	}
}
