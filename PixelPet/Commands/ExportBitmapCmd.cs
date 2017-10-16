using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class ExportBitmapCmd : CliCommand {
		public ExportBitmapCmd()
			: base ("Export-Bitmap", new Parameter[] {
				new Parameter(true, new ParameterValue("path")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			string path = this.Parameters[0].Values[0].GetValue();
			cli.Log("Exporting bitmap " + Path.GetFileName(path) + "...");

			workbench.Bitmap.Save(path);
		}
	}
}
