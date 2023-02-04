using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal sealed class DeserializeTilesetCmd : Command {
		public DeserializeTilesetCmd()
			: base("Deserialize-Tileset",
				new Parameter(true, new ParameterValue("tilemap-format")),
				new Parameter("append", "a", false),
				new Parameter("tile-count", "tc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0")),
				new Parameter("tile-size", "s", false, new ParameterValue("width", "-1"), new ParameterValue("height", "-1"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string mapFmtName = GetUnnamedParameter(0).Values[0].ToString();
			bool append = GetNamedParameter("--append").IsPresent;
			int tc = GetNamedParameter("--tile-count").Values[0].ToInt32();
			long offset = GetNamedParameter("--offset").Values[0].ToInt64();
			Parameter ts = GetNamedParameter("--tile-size");
			int tw = ts.Values[0].ToInt32();
			int th = ts.Values[1].ToInt32();

			if (TilemapFormat.GetFormat(mapFmtName) is not TilemapFormat mapFmt) {
				logger?.Log($"Unknown tilemap format {mapFmtName}.", LogLevel.Error);
				return false;
			}
			if (tc < 0) {
				logger?.Log("Invalid tile count.", LogLevel.Error);
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

			if (!append) {
				workbench.Tileset.Clear();
			}

			if (workbench.Tileset.Count > 0) {
				if (ts.IsPresent && (tw != workbench.Tileset.TileWidth || th != workbench.Tileset.TileHeight)) {
					logger?.Log($"Specified tile size {tw}x{th} does not match tile size {workbench.Tileset.TileWidth}x{workbench.Tileset.TileHeight} of nonempty tileset.", LogLevel.Error);
					return false;
				}
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

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
			int added = 0;
			using (PixelReader pixelReader = new(workbench.Stream, mapFmt.ColorFormat, true)) {
				while (tc-- > 0) {
					if (pixelReader.ReadPixels(pixels, 0, pixels.Length) != pixels.Length) {
						break;
					}

					// Add tile to the tileset.
					Tile tile = new(tw, th);
					tile.SetAllPixels(pixels);
					workbench.Tileset.AddTile(tile);
					added++;
				}
			}

			workbench.Tileset.ColorFormat = mapFmt.ColorFormat;
			workbench.Tileset.IsIndexed = mapFmt.IsIndexed;

			logger?.Log($"Deserialized {added} tiles.", LogLevel.Information);
			return true;
		}
	}
}
