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

		public override void Run(Workbench workbench, ILogger logger) {
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

			for (int i = 0; i < buffer.Length; i += 4) {
				int c = fmt.Convert(BitConverter.ToInt32(buffer, i), workbench.BitmapFormat, sloppy);
				buffer[i + 0] = (byte)(c >>  0);
				buffer[i + 1] = (byte)(c >>  8);
				buffer[i + 2] = (byte)(c >> 16);
				buffer[i + 3] = (byte)(c >> 24);
			}

			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);

			workbench.BitmapFormat = fmt;

			logger?.Log("Converted bitmap to color format " + fmtName + ".");
		}
	}
}
