using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class DeserializeTilemapCmd : CliCommand {
		public DeserializeTilemapCmd()
			: base("Deserialize-Tilemap",
				new Parameter("base-tile", "bt", false, new ParameterValue("index", "0")),
				new Parameter("tile-count", "tc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			int baseTile = FindNamedParameter("--base-tile").Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();
			int tileCount = FindNamedParameter("--tile-count").Values[0].ToInt32();

			if (offset < 0) {
				logger?.Log("Invalid offset.", LogLevel.Error);
				return;
			}
			if (tileCount < 0) {
				logger?.Log("Invalid tile count.", LogLevel.Error);
				return;
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

			// Bytes per tile entry.
			int bpe = 2;

			byte[] buffer = new byte[bpe];
			while (tileCount > 0 && workbench.Stream.Read(buffer, 0, bpe) == bpe) {
				int scrn = buffer[0] | (buffer[1] << 8);

				TileEntry te = new TileEntry(
					tileNumber: (scrn & 0x3FF) - baseTile,
					hFlip: (scrn & (1 << 10)) != 0,
					vFlip: (scrn & (1 << 11)) != 0,
					paletteNumber: (scrn >> 12) & 0xF
				);

				workbench.Tilemap.Add(te);
				
				tileCount--;
			}
		}
	}
}
