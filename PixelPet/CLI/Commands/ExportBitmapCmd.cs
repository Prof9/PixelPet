using LibPixelPet;
using SkiaSharp;
using System;
using System.IO;

namespace PixelPet.CLI.Commands {
	internal class ExportBitmapCmd : CliCommand {
		public ExportBitmapCmd()
			: base ("Export-Bitmap",
				new Parameter(true, new ParameterValue("path")),
				new Parameter("format", "f", false, new ParameterValue("format"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string path = this.FindUnnamedParameter(0).Values[0].ToString();
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

			SKBitmap bmp = null;
			bool setAlpha = workbench.BitmapFormat.AlphaBits == 0;
			try {
				bmp = new SKBitmap(workbench.Bitmap.Width, workbench.Bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
				SKColor[] pixels = new SKColor[bmp.Width * bmp.Height];

				for (int i = 0; i < workbench.Bitmap.PixelCount; i++) {
					pixels[i] = new SKColor((uint)fmt.Convert(workbench.Bitmap[i], workbench.BitmapFormat));
				}

				if (setAlpha) {
					for (int i = 0; i < pixels.Length; i++) {
						pixels[i] = pixels[i].WithAlpha(0xFF);
					}
				}

				bmp.Pixels = pixels;

				Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
				using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write)) {
					if (!bmp.Encode(fs, SKEncodedImageFormat.Png, 100)) {
						throw new Exception("Could not encode bitmap");
					}
				}
			} catch (Exception) {
				logger?.Log("Could not save bitmap " + Path.GetFileName(path) + ".", LogLevel.Error);
				return false;
			} finally {
				bmp?.Dispose();
			}

			logger?.Log("Exported bitmap " + Path.GetFileName(path) + (setAlpha ? " (added alpha)" : "") + '.');
			return true;
		}
	}
}
