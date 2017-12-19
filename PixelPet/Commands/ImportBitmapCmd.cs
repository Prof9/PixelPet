using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class ImportBitmapCmd : CliCommand {
		public ImportBitmapCmd()
			: base ("Import-Bitmap", new Parameter[] {
				new Parameter(true, new ParameterValue("path")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			string path = this.FindUnnamedParameter(0).Values[0].GetValue();
			cli.Log("Importing bitmap " + Path.GetFileName(path) + "...");

			// Load bitmap.
			using (Bitmap bmp = new Bitmap(path)) {
				// Import bitmap to workbench.
				workbench.ClearBitmap(bmp.Width, bmp.Height);
				workbench.Graphics.CompositingMode = CompositingMode.SourceCopy;
				workbench.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
			}
		}
	}
}
