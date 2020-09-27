using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class DeserializeTilesetCmd : CliCommand {
		public DeserializeTilesetCmd()
			: base("Deserialize-Tileset",
				new Parameter(true, new ParameterValue("image-format")),
				new Parameter("tile-count", "tc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string mapFmtName = FindUnnamedParameter(0).Values[0].ToString();
			int tc = FindNamedParameter("--tile-count").Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();
			
			if (tc < 0) {
				logger?.Log("Invalid tile count.", LogLevel.Error);
				return;
			}
			if (!(BitmapFormat.GetFormat(mapFmtName) is BitmapFormat mapFmt)) {
				logger?.Log("Unknown tilemap format \"" + mapFmtName + "\".", LogLevel.Error);
				return;
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

			int bpp = mapFmt.ColorFormat.Bits;		// bits per pixel
			int tw = workbench.Tileset.TileWidth;	// tile width
			int th = workbench.Tileset.TileHeight;	// tile height

			int ppb = 8 / bpp;						// pixels per byte
			int bpt = (tw * th) / ppb;				// bytes per tile
			int pmask = mapFmt.ColorFormat.Mask;	// mask per pixel
			int b;									// current byte
			int b2;									// current byte 2
			int bi;									// byte index in current tile
			int pi;									// pixel index in current byte
			int c;									// current pixel

			bool usePal = mapFmt.IsIndexed && workbench.PaletteSet.Count > 0;
			Palette pal = null;
			if (mapFmt.IsIndexed && workbench.PaletteSet.Count > 0) {
				pal = workbench.PaletteSet[0].Palette;
			}

			byte[] buffer = new byte[bpt];
			int[] pixels = new int[tw * th];
			int added = 0;
			while (tc-- > 0 && workbench.Stream.Read(buffer, 0, bpt) == bpt) {
				switch (mapFmt.BitmapEncoding) {
				case BitmapEncoding.Normal:
					DeserializeTileNormal();
					break;
				case BitmapEncoding.GameBoy:
					DeserializeTileGameBoy();
					break;
				}

				// Add tile to the tileset.
				Tile tile = new Tile(tw, th);
				tile.SetAllPixels(pixels);
				workbench.Tileset.AddTile(tile, mapFmt.CanFlipHorizontal, mapFmt.CanFlipVertical);
				added++;
			}

			workbench.Tileset.ColorFormat = pal?.Format ?? mapFmt.ColorFormat;

			logger?.Log("Deserialized " + added + " tiles.", LogLevel.Information);

			void DeserializeTileNormal() {
				// Process all bytes.
				for (bi = 0; bi < bpt; bi++) {
					// Get current byte.
					b = buffer[bi];

					// Extract pixels from byte.
					for (pi = 0; pi < ppb; pi++) {
						c = (b & pmask);
						b >>= bpp;

						// Apply palette.
						c = pal?[c] ?? c;

						pixels[bi * ppb + pi] = c;
					}
				}
			}

			void DeserializeTileGameBoy() {
				// Process all byte pairs.
				for (bi = 0; bi < bpt; bi += 2) {
					b  = buffer[bi];
					b2 = buffer[bi + 1];

					// Extract pixels from byte.
					for (pi = 0; pi < 2 * ppb; pi++) {
						c = ((b & 0x80) >> 7) | ((b2 & 0x80) >> 6);
						b  <<= 1;
						b2 <<= 1;

						// Apply palette.
						c = pal?[c] ?? c;

						pixels[bi * ppb + pi] = c;
					}
				}
			}
		}
	}
}
