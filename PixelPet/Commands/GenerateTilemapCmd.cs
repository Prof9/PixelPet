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
			: base("Generate-Tilemap", new Parameter[] {
				new Parameter("palette-size", "p", false, new ParameterValue("count", "16")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Generating tilemap...");

			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();

			workbench.Tilemap = new Tilemap(workbench.Bitmap, workbench.Tileset, workbench.Palette, palSize);
		}
	}
}
