﻿using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal sealed class RenderTilesetCmd : Command {
		public RenderTilesetCmd()
			: base("Render-Tileset",
				new Parameter("tiles-per-row", "tw", false, new ParameterValue("count", "32")),
				new Parameter("format", "f", false, new ParameterValue("format"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			Parameter format = GetNamedParameter("--format");
			int maxTilesPerRow = GetNamedParameter("--tiles-per-row").Values[0].ToInt32();

			ColorFormat fmt = ColorFormat.ARGB8888;
			if (format.IsPresent) {
				string fmtName = format.Values[0].ToString();
				ColorFormat? fmt2 = ColorFormat.GetFormat(fmtName);
				if (fmt2 is null) { 
					logger?.Log($"Unknown color format {fmtName}.", LogLevel.Error);
					return false;
				} else {
					fmt = (ColorFormat)fmt2;
				}
			}

			if (maxTilesPerRow < 0) {
				logger?.Log("Invalid tile count per row.", LogLevel.Error);
				return false;
			}

			if (workbench.Tileset.Count <= 0) {
				logger?.Log("Cannot render empty tileset.", LogLevel.Error);
				return false;
			}

			workbench.Bitmap = workbench.Tileset.IsIndexed
				? workbench.Tileset.ToBitmapIndexed(maxTilesPerRow, workbench.PaletteSet, fmt)
				: workbench.Tileset.ToBitmap(maxTilesPerRow, fmt);
			workbench.BitmapFormat = fmt;
			return true;
		}
	}
}
