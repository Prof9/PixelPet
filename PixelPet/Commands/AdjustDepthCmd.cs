using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class AdjustDepthCmd : CliCommand {
		public AdjustDepthCmd()
			: base("Adjust-Depth") { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Adjusting color depth...");

			// Generate lookup table.
			byte[] lut = new byte[32];
			for (int i = 0; i < 32; i++) {
				lut[i] = (byte)Math.Round((float)i / 31 * 255);
			}

			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadOnly,
				workbench.Bitmap.PixelFormat
			);
			byte[] buffer = new byte[bmpData.Stride * workbench.Bitmap.Height];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);

			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = lut[buffer[i] / 8];
			}

			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);
		}
	}
}
