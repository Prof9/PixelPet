using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class RenderTilemapCmd : CliCommand {
		public RenderTilemapCmd()
			: base("Render-Tilemap", new Parameter[] {
				new Parameter(true, new ParameterValue("width")),
				new Parameter(true, new ParameterValue("height")),
			}) { }

		public override void Run(Workbench workbench, Cli cli) {
			int width  = FindUnnamedParameter(0).Values[0].ToInt32();
			int height = FindUnnamedParameter(1).Values[0].ToInt32();

			cli.Log("Rendering tilemap...");

			workbench.SetBitmap(
				workbench.Tilemap.ToBitmap(workbench.Tileset, width, height)
			);
		}
	}
}
