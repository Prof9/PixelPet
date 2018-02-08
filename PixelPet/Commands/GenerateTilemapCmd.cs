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
				new Parameter("palette-size", "ps", false, new ParameterValue("count",  "16")),
				new Parameter("no-reduce",    "nr", false),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Generating tilemap...");

			int palSize   = FindNamedParameter("--palette-size").Values[0].ToInt32();
			bool noReduce = FindNamedParameter("--no-reduce").IsPresent;

			workbench.Tilemap = new Tilemap(workbench.Bitmap, workbench.Tileset, workbench.Palette, palSize, !noReduce);
		}
	}
}
