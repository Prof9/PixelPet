using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class ExportBytesCmd : CliCommand {
		public ExportBytesCmd()
			: base ("Export-Bytes", new Parameter[] {
				new Parameter(true, new ParameterValue("path")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			string path = this.Parameters[0].Values[0].GetValue();
			cli.Log("Exporting bytes to " + Path.GetFileName(path) + "...");

			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
				workbench.Stream.WriteTo(fs);
			}
		}
	}
}
