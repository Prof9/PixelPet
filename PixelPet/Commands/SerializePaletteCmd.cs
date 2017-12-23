using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class SerializePaletteCmd : CliCommand {
		public SerializePaletteCmd()
			: base("Serialize-Palette") { }

		public override void Run(Workbench workbench, Cli cli) {
			cli.Log("Serializing palette...");

			workbench.Stream.SetLength(0);

			foreach (Color color in workbench.Palette) {
				int c =
					(color.R >> 3 <<  0) |
					(color.G >> 3 <<  5) |
					(color.B >> 3 << 10);
				workbench.Stream.WriteByte((byte)(c >> 0));
				workbench.Stream.WriteByte((byte)(c >> 8));
			}
		}
	}
}
