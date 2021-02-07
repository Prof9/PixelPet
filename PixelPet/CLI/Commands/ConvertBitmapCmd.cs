using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelPet.CLI.Commands {
	internal class ConvertBitmapCmd : CliCommand {
		public ConvertBitmapCmd()
			: base("Convert-Bitmap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("sloppy", "s", false)
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].ToString();
			bool sloppy = FindNamedParameter("--sloppy").IsPresent;

			if (!(ColorFormat.GetFormat(fmtName) is ColorFormat fmt)) {
				logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
				return;
			}

			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadWrite,
				workbench.Bitmap.PixelFormat
			);
			byte[] buffer = new byte[bmpData.Stride * workbench.Bitmap.Height];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);

			int mayNeedSloppy = 0;

			for (int i = 0; i < buffer.Length; i += 4) {
				int cb = BitConverter.ToInt32(buffer, i);
				int ca = fmt.Convert(cb, workbench.BitmapFormat, sloppy);
				if (!sloppy && fmt.Bits < workbench.BitmapFormat.Bits) {
					// Convert sloppy and back.
					int csa = fmt.Convert(cb, workbench.BitmapFormat, true);
					int csb = workbench.BitmapFormat.Convert(csa, fmt, true);
					// Check if original color may be sloppy, and this would change the output.
					if (cb == csb && ca != csa) {
						mayNeedSloppy++;
					}
				}
				buffer[i + 0] = (byte)(ca >>  0);
				buffer[i + 1] = (byte)(ca >>  8);
				buffer[i + 2] = (byte)(ca >> 16);
				buffer[i + 3] = (byte)(ca >> 24);
			}

			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);

			workbench.BitmapFormat = fmt;

			if (mayNeedSloppy >= workbench.Bitmap.Width * workbench.Bitmap.Height) {
				logger?.Log("This bitmap appears to be improperly scaled. The --sloppy flag may be required.", LogLevel.Warning);
			}
			logger?.Log("Converted bitmap to color format " + fmtName + ".");
		}
	}
}
