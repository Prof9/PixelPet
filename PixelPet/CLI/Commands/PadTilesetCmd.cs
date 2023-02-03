using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal sealed class PadTilesetCmd : CliCommand {
		public PadTilesetCmd()
			: base("Pad-Tileset",
				new Parameter(true, new ParameterValue("width")),
				new Parameter("color", "c", false, new ParameterValue("value", "0")),
				new Parameter("tile-size", "s", false, new ParameterValue("width", "-1"), new ParameterValue("height", "-1"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();
			int color = FindNamedParameter("--color").Values[0].ToInt32();
			Parameter ts = FindNamedParameter("--tile-size");
			int tw = ts.Values[0].ToInt32();
			int th = ts.Values[1].ToInt32();

			if (width < 1) {
				logger?.Log("Invalid tileset width.", LogLevel.Error);
				return false;
			}
			if (ts.IsPresent && tw <= 0) {
				logger?.Log("Invalid tile width.", LogLevel.Error);
				return false;
			}
			if (ts.IsPresent && th <= 0) {
				logger?.Log("Invalid tile height.", LogLevel.Error);
				return false;
			}
			if (ts.IsPresent && workbench.Tileset.Count > 0 &&
				(tw != workbench.Tileset.TileWidth || th != workbench.Tileset.TileHeight)) {
				logger?.Log($"Specified tile size {tw}x{th} does not match tile size {workbench.Tileset.TileWidth}x{workbench.Tileset.TileHeight} of nonempty tileset.", LogLevel.Error);
				return false;
			}

			// Use existing tile size if not specified
			if (tw == -1 && th == -1) {
				tw = workbench.Tileset.TileWidth;
				th = workbench.Tileset.TileHeight;
			}
			// Set tileset to new tile size
			if (workbench.Tileset.Count == 0) {
				workbench.Tileset.TileWidth = tw;
				workbench.Tileset.TileHeight = th;
			}

			int[] pixels = new int[tw * th];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = color;
			}

			int addedTiles = 0;
			while (workbench.Tileset.Count < width) {
				Tile tile = new(tw, th);
				tile.SetAllPixels(pixels);
				workbench.Tileset.AddTile(tile);
				addedTiles++;
			}

			logger?.Log($"Padded tileset to width {width} (added {addedTiles} tiles).", LogLevel.Information);
			return true;
		}
	}
}
