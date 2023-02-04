using LibPixelPet;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal sealed class SerializePalettesCmd : Command {
		public SerializePalettesCmd()
			: base("Serialize-Palettes",
				new Parameter("append", "a", false)
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			bool append = GetNamedParameter("--append").IsPresent;
			int width = workbench.PaletteSet.Max(pe => pe.Palette.Count);

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			int colors = 0;
			foreach (PaletteEntry pe in workbench.PaletteSet) {
				Palette pal = pe.Palette;
				int bpc = (pal.Format.Bits + 7) / 8;

				for (int i = 0; i < width; i++) {
					int c = i < pal.Count ? pal[i] : 0;

					for (int j = 0; j < bpc; j++) {
						workbench.Stream.WriteByte((byte)(c & 0xFF));
						c >>= 8;
					}

					colors++;
				}
			}

			logger?.Log($"Serialized {width}x{workbench.PaletteSet.Count} palette set ({colors} colors).", LogLevel.Information);
			return true;
		}
	}
}
