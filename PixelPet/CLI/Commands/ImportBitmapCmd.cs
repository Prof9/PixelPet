using LibPixelPet;
using SkiaSharp;
using System.Buffers;
using System.IO;
using System.Text;

namespace PixelPet.CLI.Commands {
	internal class ImportBitmapCmd : CliCommand {
		public ImportBitmapCmd()
			: base("Import-Bitmap",
				new Parameter(true, new ParameterValue("path")),
				new Parameter("format", "f", false, new ParameterValue("format"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();
			Parameter format = FindNamedParameter("--format");

			ColorFormat fmt = ColorFormat.ARGB8888;
			if (format.IsPresent) {
				string fmtName = format.Values[0].ToString();
				ColorFormat? fmt2 = ColorFormat.GetFormat(fmtName);
				if (fmt2 == null) {
					logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
					return false;
				} else {
					fmt = (ColorFormat)fmt2;
				}
			}

			try {
				if (!File.Exists(path)) {
					logger?.Log("File not found: " + Path.GetFileName(path), LogLevel.Error);
					return false;
				}
				using (SKBitmap bmp = SKBitmap.Decode(path)) {
					workbench.Bitmap = new Bitmap(bmp.Width, bmp.Height);
					SKColor[] pixels = bmp.Pixels;

					for (int i = 0; i < pixels.Length; i++) {
						workbench.Bitmap[i] = (int)(uint)pixels[i];
					}
				}
				workbench.BitmapFormat = fmt;
			} catch {
				logger?.Log("Could not import " + Path.GetFileName(path), LogLevel.Error);
				return false;
			}

			logger?.Log("Imported bitmap " + Path.GetFileName(path) + ".", LogLevel.Information);
			return true;
		}
	}
}
