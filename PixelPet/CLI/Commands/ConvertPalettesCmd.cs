using LibPixelPet;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal sealed class ConvertPalettesCmd : CLICommand {
		public ConvertPalettesCmd()
			: base("Convert-Palettes",
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

			foreach (Palette pal in workbench.PaletteSet.Select(pe => pe.Palette)) {
				for (int i = 0; i < pal.Count; i++) {
					pal[i] = fmt.Convert(pal[i], pal.Format, sloppy);
				}
				pal.Format = fmt;
			}

			logger?.Log($"Converted palettes to {fmtName}.", LogLevel.Information);
			return true;
		}
	}
}
