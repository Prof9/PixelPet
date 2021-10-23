using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace PixelPet.CLI.Commands {
	internal class ExportBitmapCmd : CliCommand {
		public ExportBitmapCmd()
			: base ("Export-Bitmap",
				new Parameter(true, new ParameterValue("path")),
				new Parameter("format", "f", false, new ParameterValue("format"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string path = this.FindUnnamedParameter(0).Values[0].ToString();
			Parameter format = FindNamedParameter("--format");

			ColorFormat fmt = ColorFormat.ARGB8888;
			if (format.IsPresent) {
				string fmtName = format.Values[0].ToString();
				ColorFormat? fmt2 = ColorFormat.GetFormat(fmtName);
				if (fmt2 == null) {
					logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
					return;
				} else {
					fmt = (ColorFormat)fmt2;
				}
			}

			Bitmap bmp = null;
			bool setAlpha = workbench.BitmapFormat.AlphaBits == 0;
			try {
				bmp = workbench.GetCroppedBitmap(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height, logger);
				BitmapData bmpData = bmp.LockBits(
					new Rectangle(0, 0, bmp.Width, bmp.Height),
					ImageLockMode.ReadWrite,
					workbench.Bitmap.PixelFormat
				);
				int[] buffer = new int[bmpData.Stride * bmp.Height / 4];
				Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);

				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] = fmt.Convert(buffer[i], workbench.BitmapFormat);
				}

				if (workbench.BitmapFormat.AlphaBits == 0) {
					for (int i = 0; i < buffer.Length; i++) {
						buffer[i] = (int)(buffer[i] | 0xFF000000);
					}
					setAlpha = true;
				}

				Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
				bmp.UnlockBits(bmpData);

				Directory.CreateDirectory(Path.GetDirectoryName(path));
				bmp.Save(path);
			} catch (IOException) {
				logger?.Log("Could not save bitmap " + Path.GetFileName(path) + ".", LogLevel.Error);
				return;
			} finally {
				bmp?.Dispose();
			}

			logger?.Log("Exported bitmap " + Path.GetFileName(path) + (setAlpha ? " (added alpha)" : "") + '.');
		}
	}
}
