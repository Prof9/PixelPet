using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class ExtractPaletteCmd : CliCommand {
		public ExtractPaletteCmd() 
			: base("Extract-Palette") { }

		public override void Run(Workbench workbench, Cli cli) {
			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadOnly,
				workbench.Bitmap.PixelFormat
			);
			byte[] buffer = new byte[bmpData.Stride * workbench.Bitmap.Height];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);

			// Load unique colors.
			List<uint> colors = new List<uint>();
			bool transparent = false;
			int x, y, ptr;
			for (y = 0; y < workbench.Bitmap.Height; y++) {
				for (x = 0; x < workbench.Bitmap.Width; x++) {
					ptr = y * bmpData.Stride + x * 4;
					uint rgba = BitConverter.ToUInt32(buffer, ptr);

					if ((rgba >> 24) == 0) {
						// Add only one transparent color, as the first color.
						if (!transparent) {
							colors.Insert(0, rgba);
							transparent = true;
						}
					} else {
						// Add only if a non-transparent version doesn't exist.
						if (colors.LastIndexOf(rgba) < (transparent ? 1 : 0)) {
							colors.Add(rgba);
						}
					}
				}
			}

			foreach (int c in colors) {
				workbench.Palette.Add(Color.FromArgb((int)c));
			}

			cli.Log("Extracted " + colors.Count + " colors from bitmap.");
		}
	}
}
