using LibPixelPet;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal sealed class ApplyPaletteBitmapCmd : CLICommand {
		public ApplyPaletteBitmapCmd()
			: base("Apply-Palette-Bitmap",
				new Parameter("palette-number", "pn", false, new ParameterValue("number"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			Parameter palNumP = FindNamedParameter("--palette-number");
			int palNum = palNumP.Values[0].ToInt32();

			// Get requested palette
			Palette pal = null;
			if (palNumP.IsPresent) {
				if (!workbench.PaletteSet.TryFindPalette(palNum, out pal)) {
					logger?.Log($"Palette number {palNum} not loaded.", LogLevel.Error);
					return false;
				}
			}

			// Check if there is a suitable palette
			uint maxCol = workbench.Bitmap.Pixels.Max(a => (uint)a);
			if (maxCol > int.MaxValue) {
				pal = null;
				logger?.Log("The current bitmap is not indexed.", LogLevel.Error);
				return false;
			} else if (pal is null) {
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
					if (pal is null) {
						logger?.Log("No suitable palette loaded.", LogLevel.Error);
					}
				}
			} else if ((int)maxCol >= pal.Count) {
				// Can't use this palette; too small
				pal = null;
				logger?.Log($"Palette {palNum} contains {pal.Count} colors.", LogLevel.Error);
			}

			// Do we have a suitable palette?
			if (pal is null) {
				logger?.Log($"The current bitmap requires a palette with at least {maxCol + 1} colors.", LogLevel.Error);
				return false;
			}

			// Apply the palette
			for (int i = 0; i < workbench.Bitmap.PixelCount; i++) {
				workbench.Bitmap[i] = pal[workbench.Bitmap[i]];
			}
			workbench.BitmapFormat = pal.Format;

			logger?.Log($"Applied palette {palNum} to {workbench.Bitmap.Width}x{workbench.Bitmap.Height} bitmap.");
			return true;
		}
	}
}
