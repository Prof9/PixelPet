using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class DeserializeTilemapCmd : CliCommand {
		public DeserializeTilemapCmd()
			: base("Deserialize-Tilemap") { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Deserializing tilemap...");

			workbench.Stream.Position = 0;
			workbench.Tilemap = new Tilemap();

			int bpe = 2;		// bytes per tile entry

			byte[] buffer = new byte[bpe];
			while (workbench.Stream.Read(buffer, 0, 2) == bpe) {
				int scrn = buffer[0] | (buffer[1] << 8);

				TileEntry te = new TileEntry() {
					TileNumber = scrn & 0x3FF,
					HFlip = (scrn & (1 << 10)) != 0,
					VFlip = (scrn & (1 << 11)) != 0,
					PaletteNumber = (scrn >> 12) & 0xF
				};

				workbench.Tilemap.TileEntries.Add(te);
			}
		}
	}
}
