using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class CropBitmapCmd : CliCommand {
		public CropBitmapCmd()
			: base("Crop-Bitmap",
				new Parameter("x",      "x", false, new ParameterValue("pixels", "0")),
				new Parameter("y",      "y", false, new ParameterValue("pixels", "0")),
				new Parameter("width",  "w", false, new ParameterValue("pixels", "-1")),
				new Parameter("height", "h", false, new ParameterValue("pixels", "-1"))
			) { }

		public override void Run(Workbench workbench, Cli cli) {
			int x = FindNamedParameter("--x"     ).Values[0].ToInt32();
			int y = FindNamedParameter("--y"     ).Values[0].ToInt32();
			int w = FindNamedParameter("--width" ).Values[0].ToInt32();
			int h = FindNamedParameter("--height").Values[0].ToInt32();

			cli.Log("Cropping bitmap...");

			workbench.SetBitmap(workbench.GetCroppedBitmap(x, y, w, h, cli));
		}
	}
}
