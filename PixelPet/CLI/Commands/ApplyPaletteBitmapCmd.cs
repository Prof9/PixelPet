using LibPixelPet;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace PixelPet.CLI.Commands {
	internal class ApplyPaletteBitmapCmd : CliCommand {
		public ApplyPaletteBitmapCmd()
			: base("Apply-Palette-Bitmap",
				new Parameter("palette-number", "pn", false, new ParameterValue("number"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			Parameter palNumP = FindNamedParameter("--palette-number");
			int palNum = palNumP.Values[0].ToInt32();

			// Get requested palette
			Palette pal = null;
			if (palNumP.IsPresent) {
				if (!workbench.PaletteSet.TryFindPalette(palNum, out pal)) {
					logger?.Log("Palette number " + palNum + " not loaded.", LogLevel.Error);
					return;
				}
			}

			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadWrite,
				workbench.Bitmap.PixelFormat
			);
			int[] buffer = new int[bmpData.Stride * workbench.Bitmap.Height];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length / 4);

			// Check if there is a suitable palette
			uint maxCol = buffer.Max(a => (uint)a);
			if (maxCol > int.MaxValue) {
				pal = null;
				logger?.Log("The current bitmap is not indexed.");
			} else if (pal == null) {
				if (workbench.PaletteSet.Count == 1) {
					// When there is only one palette, skip the check to improve speed
					pal = workbench.PaletteSet[0].Palette;
				} else {
					// Find first palette with enough colors
					foreach (PaletteEntry palEntry in workbench.PaletteSet) {
						if (palEntry.Palette.Count >= (int)maxCol) {
							pal = palEntry.Palette;
							palNum = palEntry.Number;
							break;
						}
					}
					if (pal == null) {
						logger?.Log("No suitable palette loaded.", LogLevel.Error);
					}
				}
			} else if ((int)maxCol >= pal.Count) {
				// Can't use this palette; too small
				pal = null;
				logger?.Log("Palette " + palNum + " contains " + pal.Count + " colors.", LogLevel.Error);
			}

			// Do we have a suitable palette?
			if (pal == null) {
				logger?.Log("The current bitmap requires a palette with at least " + (maxCol + 1) + " colors.", LogLevel.Error);
				workbench.Bitmap.UnlockBits(bmpData);
				return;
			}

			// Apply the palette
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = pal[buffer[i]];
			}

			workbench.BitmapFormat = pal.Format;
			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length / 4);
			workbench.Bitmap.UnlockBits(bmpData);

			logger?.Log("Applied palette " + palNum + " to " + workbench.Bitmap.Width + "x" + workbench.Bitmap.Height + " bitmap.");
		}
	}
}
