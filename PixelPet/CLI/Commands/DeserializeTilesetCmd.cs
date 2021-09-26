using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class DeserializeTilesetCmd : CliCommand {
		public DeserializeTilesetCmd()
			: base("Deserialize-Tileset",
				new Parameter(true, new ParameterValue("image-format")),
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

			if (!(BitmapFormat.GetFormat(mapFmtName) is BitmapFormat mapFmt)) {
				logger?.Log("Unknown bitmap format \"" + mapFmtName + "\".", LogLevel.Error);
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

			int bpp = mapFmt.ColorFormat.Bits;      // bits per pixel
			int ppb = 8 / bpp;                      // pixels per byte
			int bpt = (tw * th) / ppb;              // bytes per tile
			int pmask = mapFmt.ColorFormat.Mask;    // mask per pixel
			int b;                                  // current byte
			int b2;                                 // current byte 2
			int bi;                                 // byte index in current tile
			int pi;                                 // pixel index in current byte
			int c;                                  // current pixel

			Palette pal = null;
			if (mapFmt.IsIndexed && workbench.PaletteSet.Count > 0) {
				pal = workbench.PaletteSet[0].Palette;
			}

			byte[] buffer = new byte[bpt];
			int[] pixels = new int[tw * th];
			int added = 0;
			while (tc-- > 0 && workbench.Stream.Read(buffer, 0, bpt) == bpt) {
				switch (mapFmt.BitmapEncoding) {
				case BitmapEncoding.GameBoyAdvance:
				case BitmapEncoding.NintendoDSTexture:
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
			workbench.Tileset.IsIndexed = !usePalette;

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
						if (usePalette) {
							c = pal?[c] ?? c;
						}

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
						if (usePalette) {
							c = pal?[c] ?? c;
						}

						pixels[bi * ppb + pi] = c;
					}
				}
			}
		}
	}
}
