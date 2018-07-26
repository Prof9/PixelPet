using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class SerializeTilesetCmd : CliCommand {
		public SerializeTilesetCmd()
			: base("Serialize-Tileset") { }

		public override void Run(Workbench workbench, ILogger logger) {
			ColorFormat fmt = workbench.Tileset.ColorFormat;
			int bpp = fmt.Bits;		// bits per pixel
			int ppb = 8 / bpp;      // pixels per byte

			workbench.Stream.SetLength(0);

			foreach (Tile tile in workbench.Tileset) {
				// pixel index in current byte
				int pi = 0;
				// current byte
				int b = 0;
				foreach (int c in tile.EnumerateTile()) {
					// Add pixel to current byte.
					b |= (c << (pi * bpp));
					if (++pi >= ppb) {
						// Write byte to stream if it's finished.
						workbench.Stream.WriteByte((byte)b);
						b = 0;
						pi = 0;
					}
				}
				if (pi > 0) {
					// Write remainder byte to stream.
					workbench.Stream.WriteByte((byte)b);
				}
			}

			logger?.Log("Serialized " + workbench.Tileset.Count + " tiles.", LogLevel.Information);
		}
	}
}
