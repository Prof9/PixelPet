using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class RenderPalettesCmd : CliCommand {
		public RenderPalettesCmd()
			: base("Render-Palettes",
				new Parameter("colors-per-row", "cw", false, new ParameterValue("count", "0"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			int maxTilesPerRow = FindNamedParameter("--colors-per-row").Values[0].ToInt32();
			if (maxTilesPerRow < 0) {
				logger?.Log("Invalid colors per row.", LogLevel.Error);
				return;
			}

			int tw = 8;
			int th = 8;

			int maxColors = workbench.PaletteSet.Max(pe => pe.Palette.Count);
			int palCount  = workbench.PaletteSet.Count;

			if (maxTilesPerRow == 0) {
				maxTilesPerRow = maxColors;
			}

			int w = maxTilesPerRow;
			int h = workbench.PaletteSet.Sum(pe => (pe.Palette.Count + maxTilesPerRow - 1) / maxTilesPerRow);

			if (w <= 0 || h <= 0) {
				logger?.Log("Cannot render empty palette set.", LogLevel.Error);
				return;
			}

			int count = 0;

			ColorFormat fmt = ColorFormat.BGRA8888;

			workbench.ClearBitmap(w * tw, h * th);
			workbench.Graphics.CompositingMode = CompositingMode.SourceCopy;
			using (SolidBrush brush = new SolidBrush(Color.Black)) {
				int p = 0;
				int c = 0;
				for (int j = 0; j < h; j++) {
					Palette pal = workbench.PaletteSet[p].Palette;
					for (int i = 0; i < w; i++) {
						// Draw transparent if we ran out of colors.
						if (c >= pal.Count) {
							brush.Color = Color.Transparent;
						} else {
							brush.Color = Color.FromArgb(fmt.Convert(pal[c++], pal.Format));
						}
						workbench.Graphics.FillRectangle(brush, i * tw, j * th, tw, th);
						count++;
					}
					// Go to next palette.
					if (c >= pal.Count) {
						p++;
						c = 0;
					}
				}
			}
			workbench.Graphics.Flush();

			logger?.Log("Rendered " + w + "x" + h + " palette set containing " + count + " colors.", LogLevel.Information);
		}
	}
}
