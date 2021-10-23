using LibPixelPet;
using System;
using System.IO;

namespace PixelPet.CLI.Commands {
	internal class ExportBytesCmd : CliCommand {
		public ExportBytesCmd()
			: base("Export-Bytes",
				new Parameter(true, new ParameterValue("path"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string path = this.FindUnnamedParameter(0).Values[0].ToString();

			try {
				Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
				using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
					workbench.Stream.WriteTo(fs);
				}
			} catch (IOException) {
				logger?.Log("Could not save binary file " + Path.GetFileName(path) + ".", LogLevel.Error);
				return;
			}

			logger?.Log("Exported " + workbench.Stream.Length + " bytes to " + Path.GetFileName(path) + '.', LogLevel.Information);
		}
	}
}
