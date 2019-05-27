using LibPixelPet;
using System;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class SerializePalettesCmd : CliCommand {
		public SerializePalettesCmd()
			: base("Serialize-Palettes") { }

		protected override void Run(Workbench workbench, ILogger logger) {
			int width = workbench.PaletteSet.Max(pe => pe.Palette.Count);

			workbench.Stream.SetLength(0);

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

			logger?.Log("Serialized " + width + "x" + workbench.PaletteSet.Count + " palette set (" + colors + " colors).", LogLevel.Information);
		}
	}
}
