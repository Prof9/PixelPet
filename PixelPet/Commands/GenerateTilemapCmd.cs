using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class GenerateTilemapCmd : CliCommand {
		public GenerateTilemapCmd()
			: base("Generate-Tilemap") { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Generating tilemap...");

			workbench.Tilemap = new Tilemap(workbench.Bitmap, workbench.Tileset);
		}
	}
}
