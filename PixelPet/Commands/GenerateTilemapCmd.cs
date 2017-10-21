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
			: base("Generate-Tilemap") { }

		public override void Run(Workbench workbench, Cli cli) {
			Tilemap tm = new Tilemap(workbench.Bitmap, 8, 8);
			tm.Reduce(true);

			workbench.Stream.SetLength(0);

			foreach (Tilemap.Entry entry in tm.TileEntries) {
				int scrn = 0;
				scrn |= entry.TileNumber & 0x1FF;
				scrn |= entry.FlipHorizontal ? 1 << 10 : 0;
				scrn |= entry.FlipVertical   ? 1 << 11 : 0;
				workbench.Stream.WriteByte((byte)(scrn >> 0));
				workbench.Stream.WriteByte((byte)(scrn >> 8));
			}
		}
	}
}
