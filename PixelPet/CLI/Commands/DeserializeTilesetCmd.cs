using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class DeserializeTilesetCmd : CliCommand {
		public DeserializeTilesetCmd()
			: base("Deserialize-Tileset",
				new Parameter(true, new ParameterValue("tilemap-format")),
				new Parameter("append", "a", false),
				new Parameter("ignore-palette", "ip", false),
				new Parameter("tile-count", "tc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0")),
				new Parameter("tile-size", "s", false, new ParameterValue("width", "-1"), new ParameterValue("height", "-1"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string mapFmtName = FindUnnamedParameter(0).Values[0].ToString();
			bool append = FindNamedParameter("--append").IsPresent;
			int tc = FindNamedParameter("--tile-count").Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();
			bool usePalette = !FindNamedParameter("--ignore-palette").IsPresent;
			Parameter ts = FindNamedParameter("--tile-size");
			int tw = ts.Values[0].ToInt32();
			int th = ts.Values[1].ToInt32();

			if (!(TilemapFormat.GetFormat(mapFmtName) is TilemapFormat mapFmt)) {
				logger?.Log("Unknown tilemap format \"" + mapFmtName + "\".", LogLevel.Error);
				return;
			}
			if (tc < 0) {
				logger?.Log("Invalid tile count.", LogLevel.Error);
				return;
			}
			if (ts.IsPresent && tw <= 0) {
				logger?.Log("Invalid tile width.", LogLevel.Error);
				return;
			}
			if (ts.IsPresent && th <= 0) {
				logger?.Log("Invalid tile height.", LogLevel.Error);
				return;
			}
			if (ts.IsPresent && workbench.Tileset.Count > 0 &&
				(tw != workbench.Tileset.TileWidth || th != workbench.Tileset.TileHeight)) {
					logger?.Log("Specified tile size " + tw + "x" + th + " does not match tile size " +
						workbench.Tileset.TileWidth + "x" + workbench.Tileset.TileHeight + " of nonempty tileset.", LogLevel.Error);
					return;
				}

			if (!append) {
				workbench.Tileset.Clear();
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

			Palette pal = null;
			if (mapFmt.IsIndexed && workbench.PaletteSet.Count > 0) {
				pal = workbench.PaletteSet[0].Palette;
			}

			int[] pixels = new int[tw * th];
			int added = 0;
			using (PixelReader pixelReader = new PixelReader(workbench.Stream, mapFmt.ColorFormat, true)) {
				while (tc-- > 0) {
					if (pixelReader.ReadPixels(pixels, 0, pixels.Length) != pixels.Length) {
						break;
					}

					if (usePalette) {
						for (int i = 0; i < pixels.Length; i++) {
							int c = pixels[i];
							pixels[i] = pal?[c] ?? c;
						}
					}
					
					// Add tile to the tileset.
					Tile tile = new Tile(tw, th);
					tile.SetAllPixels(pixels);
					workbench.Tileset.AddTile(tile);
					added++;
				}
			}

			if (usePalette && pal != null) {
				workbench.Tileset.ColorFormat = pal.Format;
			} else {
				workbench.Tileset.ColorFormat = mapFmt.ColorFormat;
			}
			workbench.Tileset.IsIndexed = !usePalette;

			logger?.Log("Deserialized " + added + " tiles.", LogLevel.Information);
		}
	}
}
