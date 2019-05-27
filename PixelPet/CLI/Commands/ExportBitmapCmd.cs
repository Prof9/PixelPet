using LibPixelPet;
using System;
using System.IO;

namespace PixelPet.CLI.Commands {
	internal class ExportBitmapCmd : CliCommand {
		public ExportBitmapCmd()
			: base ("Export-Bitmap",
				new Parameter(true, new ParameterValue("path"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string path = this.FindUnnamedParameter(0).Values[0].ToString();

			try {
				workbench.Bitmap.Save(path);
			} catch (IOException) {
				logger?.Log("Could not save bitmap " + Path.GetFileName(path) + ".", LogLevel.Error);
				return;
			}

			logger?.Log("Exported bitmap " + Path.GetFileName(path) + '.');
		}
	}
}
