using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class SerializeTilemapCmd : CliCommand {
		public SerializeTilemapCmd()
			: base("Serialize-Tilemap", new Parameter[] {
				new Parameter("palette",    "p", false, new ParameterValue("index",          "0")),
				new Parameter("base-tile",  "b", false, new ParameterValue("index",          "0")),
				new Parameter("first-tile", "f", false, new ParameterValue("tilemap-entry", "-1")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			int palette = FindNamedParameter("--palette").Values[0].ToInt32();
			int baseTile = FindNamedParameter("--base-tile").Values[0].ToInt32();
			int firstTile = FindNamedParameter("--first-tile").Values[0].ToInt32();

			cli.Log("Serializing tilemap...");

			workbench.Stream.SetLength(0);

			foreach (TileEntry entry in workbench.Tilemap.TileEntries) {
				int scrn = 0;
				if (firstTile >= 0 && entry.TileNumber == 0) {
					scrn = firstTile;
				} else {
					scrn |= (entry.TileNumber + baseTile) & 0x3FF;
					scrn |= entry.HFlip ? 1 << 10 : 0;
					scrn |= entry.VFlip ? 1 << 11 : 0;
					scrn |= (palette + entry.PaletteNumber) << 12;
				}

				workbench.Stream.WriteByte((byte)(scrn >> 0));
				workbench.Stream.WriteByte((byte)(scrn >> 8));
			}
		}
	}
}
