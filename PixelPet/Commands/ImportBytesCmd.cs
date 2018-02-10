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
				new Parameter("offset", "o", false, new ParameterValue("count", "0")),
				new Parameter("length", "l", false, new ParameterValue("count", "-1")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			string path = this.FindUnnamedParameter(0).Values[0].GetValue();
			long offset = this.FindNamedParameter("--offset").Values[0].ToInt64();
			int  length = this.FindNamedParameter("--length").Values[0].ToInt32();

			cli.Log("Importing binary " + Path.GetFileName(path) + "...");

			workbench.Stream.SetLength(0);

			// Load binary.
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				if (offset < fs.Length) {
					fs.Position = offset;
				} else {
					fs.Position = fs.Length;
					cli.Log("WARNING: Could not seek to offset " + offset + "; file size is " + fs.Length + ".");
				}
				if (length < 0) {
					fs.CopyTo(workbench.Stream);
				} else {
					byte[] buffer = new byte[length];
					int read = fs.Read(buffer, 0, buffer.Length);
					workbench.Stream.Write(buffer, 0, read);

					if (read < length) {
						cli.Log("WARNING: Could not read " + length + " bytes from file; " + read + " bytes read.");
					}
				}
			}
		}
	}
}
