using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class GenerateTilemapCmd : CliCommand {
		protected struct TilemapEntry {
			int TileNum { get; set; }
			int Palette { get; set; }
			bool HFlip { get; set; }
			bool VFlip { get; set; }
		}

		public GenerateTilemapCmd()
			: base("Generate-Tilemap", new Parameter[] {
				new Parameter("palette",    "p", false,	new ParameterValue("index",          "0")),
				new Parameter("base-tile",  "b", false,	new ParameterValue("index",          "0")),
				new Parameter("first-tile", "f", false,	new ParameterValue("tilemap-entry", "-1")),
				new Parameter("tile-size",  "t", false,	new ParameterValue("width",          "8"),
														new ParameterValue("height",         "8")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			int palette    = FindNamedParameter("--palette"   ).Values[0].ToInt32();
			int baseTile   = FindNamedParameter("--base-tile" ).Values[0].ToInt32();
			int firstTile  = FindNamedParameter("--first-tile").Values[0].ToInt32();
			int tileWidth  = FindNamedParameter("--tile-size" ).Values[0].ToInt32();
			int tileHeight = FindNamedParameter("--tile-size" ).Values[1].ToInt32();

			cli.Log("Generating tilemap...");

			Tilemap tm = new Tilemap(workbench.Bitmap, tileWidth, tileHeight);
			tm.Reduce(true);

			workbench.SetBitmap(tm.GetTileset());

			workbench.Stream.SetLength(0);

			foreach (Tilemap.Entry entry in tm.TileEntries) {
				int scrn = 0;
				if (firstTile >= 0 && entry.TileNumber == 0) {
					scrn = firstTile;
				} else {
					scrn |= (entry.TileNumber + baseTile) & 0x1FF;
					scrn |= entry.FlipHorizontal ? 1 << 10 : 0;
					scrn |= entry.FlipVertical ? 1 << 11 : 0;
					scrn |= palette << 12;
				}

				workbench.Stream.WriteByte((byte)(scrn >> 0));
				workbench.Stream.WriteByte((byte)(scrn >> 8));
			}
		}
	}
}
