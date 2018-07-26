using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class DeserializeTilesetCmd : CliCommand {
		public DeserializeTilesetCmd()
			: base("Deserialize-Tileset",
				new Parameter(true, new ParameterValue("tilemapFormat")),
				new Parameter(true, new ParameterValue("colorFormat")),
				new Parameter("tile-count", "tc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			string mapFmtName = FindUnnamedParameter(0).Values[0].ToString();
			string colFmtName = FindUnnamedParameter(1).Values[0].ToString();
			int tc = FindNamedParameter("--tile-count").Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();
			
			if (tc < 0) {
				logger?.Log("Invalid tile count.", LogLevel.Error);
				return;
			}
			if (!(TilemapFormat.GetFormat(mapFmtName) is TilemapFormat mapFmt)) {
				logger?.Log("Unknown tilemap format \"" + mapFmtName + "\".", LogLevel.Error);
				return;
			}
			if (!(ColorFormat.GetFormat(colFmtName) is ColorFormat colFmt)) {
				logger?.Log("Unknown color format \"" + colFmtName + "\".", LogLevel.Error);
				return;
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

			int bpp = colFmt.Bits;						// bits per pixel
			int tw = workbench.Tileset.TileWidth;	// tile width
			int th = workbench.Tileset.TileHeight;	// tile height

			int ppb = 8 / bpp;						// pixels per byte
			int bpt = (tw * th) / ppb;              // bytes per tile
			int pmask = colFmt.Mask;                   // mask per pixel
			int b;                                  // current byte
			int bi;									// byte index in current tile
			int pi;                                 // pixel index in current byte
			int c;                                  // current pixel

			byte[] buffer = new byte[bpt];
			int[] pixels = new int[tw * th];
			int added = 0;
			while (tc-- > 0 && workbench.Stream.Read(buffer, 0, bpt) == bpt) {
				// Process all bytes.
				for (bi = 0; bi < bpt; bi++) {
					// Get current byte.
					b = buffer[bi];

					// Extract pixels from byte.
					for (pi = 0; pi < ppb; pi++) {
						c = (b & pmask);
						b >>= bpp;

						pixels[bi * ppb + pi] = c;
					}
				}

				// Add tile to the tileset.
				Tile tile = new Tile(tw, th);
				tile.SetAllPixels(pixels);
				workbench.Tileset.AddTile(tile, mapFmt.CanFlipHorizontal, mapFmt.CanFlipVertical);
				added++;
			}

			logger?.Log("Deserialized " + added + " tiles.", LogLevel.Information);
		}
	}
}
