using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class RenderPaletteCmd : CliCommand {
		public RenderPaletteCmd()
			: base("Render-Palette") { }

		public override void Run(Workbench workbench, Cli cli) {
			int count = workbench.Palette.Count;
			if (count < 1) {
				cli.Log("No palette to render...");
			}

			int maxWidth = 16;
			int size = 8;

			int width = Math.Min(count, maxWidth);
			int height = Math.Max(1, (count + width - 1) / width);

			workbench.ClearBitmap(width * size, height * size);
			workbench.Graphics.CompositingMode = CompositingMode.SourceCopy;
			for (int i = 0; i < count; i++) {
				int x = i % width;
				int y = i / width;
				using (SolidBrush brush = new SolidBrush(workbench.Palette[i])) {
					workbench.Graphics.FillRectangle(brush, x * size, y * size, size, size);
				}
			}
			workbench.Graphics.Flush();
			
			cli.Log("Rendered " + count + " colors.");
		}
	}
}
