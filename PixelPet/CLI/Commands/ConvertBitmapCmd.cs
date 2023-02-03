using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal sealed class ConvertBitmapCmd : CliCommand {
		public ConvertBitmapCmd()
			: base("Convert-Bitmap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("sloppy", "s", false)
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].ToString();
			bool sloppy = FindNamedParameter("--sloppy").IsPresent;

			if (ColorFormat.GetFormat(fmtName) is not ColorFormat fmt) {
				logger?.Log($"Unknown color format {fmtName}.", LogLevel.Error);
				return false;
			}

			int mayNeedSloppy = 0;

			for (int i = 0; i < workbench.Bitmap.PixelCount; i++) {
				int cb = workbench.Bitmap.Pixels[i];
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
				workbench.Bitmap.Pixels[i] = ca;
			}

			workbench.BitmapFormat = fmt;

			if (mayNeedSloppy >= workbench.Bitmap.Width * workbench.Bitmap.Height) {
				logger?.Log("This bitmap appears to be improperly scaled. The --sloppy flag may be required.", LogLevel.Warning);
			}
			logger?.Log($"Converted bitmap to color format {fmtName}.");
			return true;
		}
	}
}
