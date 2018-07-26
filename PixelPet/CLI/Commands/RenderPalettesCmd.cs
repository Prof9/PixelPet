using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class RenderPalettesCmd : CliCommand {
		public RenderPalettesCmd()
			: base("Render-Palettes") { }

		public override void Run(Workbench workbench, ILogger logger) {
			int tw = 8;
			int th = 8;

			int w = workbench.PaletteSet.Max(pe => pe.Palette.Count);
			int h = workbench.PaletteSet.Count;

			if (w <= 0 || h <= 0) {
				logger?.Log("Cannot render empty palette set.", LogLevel.Error);
				return;
			}

			int count = 0;

			ColorFormat fmt = ColorFormat.BGRA8888;

			workbench.ClearBitmap(w * tw, h * th);
			workbench.Graphics.CompositingMode = CompositingMode.SourceCopy;
			using (SolidBrush brush = new SolidBrush(Color.Black)) {
				for (int i = 0; i < workbench.PaletteSet.Count; i++) {
					Palette pal = workbench.PaletteSet[i].Palette;
					for (int j = 0; j < pal.Count; j++) {
						brush.Color = Color.FromArgb(fmt.Convert(pal[j], pal.Format));
						workbench.Graphics.FillRectangle(brush, j * tw, i * th, tw, th);
						count++;
					}
				}
			}
			workbench.Graphics.Flush();

			logger?.Log("Rendered " + w + "x" + h + " palette set containing " + count + " colors.", LogLevel.Information);
		}
	}
}
