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
			: base("Generate-Tilemap",
				new Parameter("palette-size", "ps", false, new ParameterValue("count",  "16")),
				new Parameter("no-reduce",    "nr", false),
				new Parameter("x",            "x",  false, new ParameterValue("pixels", "0")),
				new Parameter("y",            "y",  false, new ParameterValue("pixels", "0")),
				new Parameter("width",        "w",  false, new ParameterValue("pixels", "-1")),
				new Parameter("height",       "h",  false, new ParameterValue("pixels", "-1"))
			) { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Generating tilemap...");

			int  palSize  = FindNamedParameter("--palette-size").Values[0].ToInt32();
			bool noReduce = FindNamedParameter("--no-reduce"   ).IsPresent;
			int  x        = FindNamedParameter("--x"           ).Values[0].ToInt32();
			int  y        = FindNamedParameter("--y"           ).Values[0].ToInt32();
			int  w        = FindNamedParameter("--width"       ).Values[0].ToInt32();
			int  h        = FindNamedParameter("--height"      ).Values[0].ToInt32();

			using (Bitmap bmp = workbench.GetCroppedBitmap(x, y, w, h, cli)) {
				workbench.Tilemap = new Tilemap(bmp, workbench.Tileset, workbench.Palette, palSize, !noReduce);
			}
		}
	}
}
