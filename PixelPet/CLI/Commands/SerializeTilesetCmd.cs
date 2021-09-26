using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal class SerializeTilesetCmd : CliCommand {
		public SerializeTilesetCmd()
			: base("Serialize-Tileset",
				new Parameter("append", "a", false),
				new Parameter("color-offset", "o", false, new ParameterValue("value", "0"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			bool append = FindNamedParameter("--append").IsPresent;
			long colorOffset = FindNamedParameter("--color-offset").Values[0].ToInt64();

			ColorFormat fmt = workbench.Tileset.ColorFormat;
			int bpp = fmt.Bits;     // bits per pixel
			int ppb = 8 / bpp;      // pixels per byte

			int pi;                 // pixel index in current byte
			int b;                  // current byte
			int b2;                 // current byte 2
			long co;                // color + offset

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			foreach (Tile tile in workbench.Tileset) {
				switch (workbench.TilemapFormat.BitmapEncoding) {
				case BitmapEncoding.GameBoyAdvance:
					SerializeNormal(tile);
					break;
				case BitmapEncoding.GameBoy:
					SerializeGameBoy(tile);
					break;
				}
			}

			logger?.Log("Serialized " + workbench.Tileset.Count + " tiles.", LogLevel.Information);

			void SerializeNormal(Tile tile) {
				pi = 0;
				b  = 0;
				foreach (int c in tile.EnumerateTile()) {
					co = c + colorOffset;

					if (bpp > 4) {
						for (int bi = 0; bi < bpp; bi += 8) {
							workbench.Stream.WriteByte((byte)(co >> bi));
						}
					} else {
						// Add pixel to current byte.
						b |= (int)(co << (pi * bpp));

						if (++pi >= ppb) {
							// Write byte to stream if it's finished.
							workbench.Stream.WriteByte((byte)b);
							b = 0;
							pi = 0;
						}
					}
				}
				if (pi > 0) {
					// Write remainder byte to stream.
					workbench.Stream.WriteByte((byte)b);
				}
			}

			void SerializeGameBoy(Tile tile) {
				pi = 0;
				b  = 0;
				b2 = 0;
				foreach (int c in tile.EnumerateTile()) {
					co = c + colorOffset;
					// Add pixel to current byte.
					b  |= (int)((co & 0x1) >> 0 << (7 - pi));
					b2 |= (int)((co & 0x2) >> 1 << (7 - pi));
					
					if (++pi >= 2 * ppb) {
						// Write byte to stream if it's finished.
						workbench.Stream.WriteByte((byte)b);
						workbench.Stream.WriteByte((byte)b2);
						b  = 0;
						b2 = 0;
						pi = 0;
					}
				}
				if (pi > 0) {
					// Write remainder bytes to stream.
					workbench.Stream.WriteByte((byte)b);
					workbench.Stream.WriteByte((byte)b2);
				}
			}
		}
	}
}
