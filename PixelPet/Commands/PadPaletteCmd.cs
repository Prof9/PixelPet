using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class PadPaletteCmd : CliCommand {
		public PadPaletteCmd()
			: base("Pad-Palette", new Parameter[] {
				new Parameter(true, new ParameterValue("width", "0")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			int width = FindUnnamedParameter(0).Values[0].ToInt32();

			cli.Log("Padding palette...");

			if (width <= 0) {
				return;
			}

			while (workbench.Palette.Count % width != 0) {
				workbench.Palette.Add(Color.Black);
			}
		}
	}
}
