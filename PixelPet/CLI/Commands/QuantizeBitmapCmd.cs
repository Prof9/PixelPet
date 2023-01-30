﻿using LibPixelPet;
using System.Drawing;

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

			if (pal == null) {
				// Find first palette that contains all colors in the bitmap
				foreach (PaletteEntry palEntry in workbench.PaletteSet) {
					bool canUsePal = true;
					foreach (int c in workbench.Bitmap.Pixels) {
						int ci = palEntry.Palette.IndexOfColor((int)(uint)c);
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
				return false;
			}

			// Quantize using the palette
			for (int y = 0; y < workbench.Bitmap.Height; y++) {
				for (int x = 0; x < workbench.Bitmap.Width; x++) {
					int i = y * workbench.Bitmap.Width + x;
					int ci = pal.IndexOfColor(workbench.Bitmap.Pixels[i]);
					if (ci < 0) {
						logger?.Log("Color 0x" + ((uint)workbench.Bitmap.Pixels[i]).ToString("X") + " at pixel (" + x + ", " + y + ") not found in palette " + palNum + ".", LogLevel.Error);
						break;
					}
					if (ci > fmt.MaxValue) {
						logger?.Log("Color index " + ci + " at pixel (" + x + ", " + y + ") cannot be represented in the given color format.", LogLevel.Error);
						break;
					}
					workbench.Bitmap.Pixels[i] = ci;
				}
			}

			workbench.BitmapFormat = fmt;

			logger?.Log("Quantized " + workbench.Bitmap.Width + "x" + workbench.Bitmap.Height + " bitmap using palette " + palNum + ".");
			return true;
		}
	}
}
