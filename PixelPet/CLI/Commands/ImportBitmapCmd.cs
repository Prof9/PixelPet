using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;

namespace PixelPet.CLI.Commands {
	internal class ImportBitmapCmd : CliCommand {
		public ImportBitmapCmd()
			: base("Import-Bitmap",
				new Parameter(true, new ParameterValue("path"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();

			try {
				if (!File.Exists(path)) {
					logger?.Log("File not found: " + Path.GetFileName(path), LogLevel.Error);
					return;
				}
				using (Bitmap bmp = new Bitmap(path)) {
					workbench.ClearBitmap(bmp.Width, bmp.Height);
					workbench.Graphics.CompositingMode = CompositingMode.SourceCopy;
					workbench.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
				}
			} catch {
				logger?.Log("Could not import " + Path.GetFileName(path), LogLevel.Error);
				return;
			}
			workbench.Graphics.Flush();

			logger?.Log("Imported bitmap " + Path.GetFileName(path) + ".", LogLevel.Information);
		}
	}
}
