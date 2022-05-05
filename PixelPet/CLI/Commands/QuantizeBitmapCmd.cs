using LibPixelPet;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelPet.CLI.Commands {
	internal class QuantizeBitmapCmd : CliCommand {
		public QuantizeBitmapCmd()
			: base("Quantize-Bitmap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("palette-number", "pn", false, new ParameterValue("number"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].Value;
			Parameter palNumP = FindNamedParameter("--palette-number");
			int palNum = palNumP.Values[0].ToInt32();

			if (!(ColorFormat.GetFormat(fmtName) is ColorFormat fmt)) {
				logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
				return false;
			}

			Palette pal = null;
			if (palNumP.IsPresent) {
				// Get requested palette
				if (!workbench.PaletteSet.TryFindPalette(palNum, out pal)) {
					logger?.Log("Palette number " + palNum + " not loaded.", LogLevel.Error);
					return false;
				}
			}

			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadWrite,
				workbench.Bitmap.PixelFormat
			);
			int[] buffer = new int[bmpData.Stride * workbench.Bitmap.Height / 4];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);

			if (pal == null) {
				// Find first palette that contains all colors in the bitmap
				foreach (PaletteEntry palEntry in workbench.PaletteSet) {
					bool canUsePal = true;
					foreach (int c in buffer) {
						int ci = palEntry.Palette.IndexOfColor(c);
						if (ci < 0 || ci > fmt.MaxValue) {
							canUsePal = false;
							break;
						}
					}
					if (canUsePal) {
						pal    = palEntry.Palette;
						palNum = palEntry.Number;
					}
				}
			}

			// Do we have a suitable palette?
			if (pal == null) {
				logger?.Log("No suitable palette loaded.", LogLevel.Error);
				workbench.Bitmap.UnlockBits(bmpData);
				return false;
			}

			// Quantize using the palette
			for (int y = 0; y < workbench.Bitmap.Height; y++) {
				for (int x = 0; x < workbench.Bitmap.Width; x++) {
					int i = y * (bmpData.Stride / 4) + x;
					int ci = pal.IndexOfColor(buffer[i]);
					if (ci < 0) {
						logger?.Log("Color 0x" + buffer[i].ToString("X") + " at pixel (" + x + ", " + y + ") not found in palette " + palNum + ".", LogLevel.Error);
						break;
					}
					if (ci > fmt.MaxValue) {
						logger?.Log("Color index " + ci + " at pixel (" + x + ", " + y + ") cannot be represented in the given color format.", LogLevel.Error);
						break;
					}
					buffer[i] = ci;
				}
			}

			workbench.BitmapFormat = fmt;
			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);

			logger?.Log("Quantized " + workbench.Bitmap.Width + "x" + workbench.Bitmap.Height + " bitmap using palette " + palNum + ".");
			return true;
		}
	}
}
