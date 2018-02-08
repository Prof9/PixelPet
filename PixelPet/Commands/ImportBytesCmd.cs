using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class ImportBytesCmd : CliCommand {
		public ImportBytesCmd()
			: base("Import-Bytes", new Parameter[] {
				new Parameter(true, new ParameterValue("path")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			string path = this.FindUnnamedParameter(0).Values[0].GetValue();
			cli.Log("Importing binary " + Path.GetFileName(path) + "...");

			workbench.Stream.SetLength(0);

			// Load binary.
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				fs.Position = 0;
				fs.CopyTo(workbench.Stream);
			}
		}
	}
}
