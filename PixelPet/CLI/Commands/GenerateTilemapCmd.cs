using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class GenerateTilemapCmd : CliCommand {
		public GenerateTilemapCmd()
			: base("Generate-Tilemap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("append", "a", false),
				new Parameter("no-reduce", "nr", false),
				new Parameter("x", "x", false, new ParameterValue("pixels", "0")),
				new Parameter("y", "y", false, new ParameterValue("pixels", "0")),
				new Parameter("width", "w", false, new ParameterValue("pixels", "-1")),
				new Parameter("height", "h", false, new ParameterValue("pixels", "-1")),
				new Parameter("tile-size", "s", false, new ParameterValue("width", "-1"), new ParameterValue("height", "-1"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].Value;
			bool append = FindNamedParameter("--append").IsPresent;
			bool noReduce = FindNamedParameter("--no-reduce").IsPresent;
			int x = FindNamedParameter("--x").Values[0].ToInt32();
			int y = FindNamedParameter("--y").Values[0].ToInt32();
			int w = FindNamedParameter("--width").Values[0].ToInt32();
			int h = FindNamedParameter("--height").Values[0].ToInt32();
			Parameter ts = FindNamedParameter("--tile-size");
			int tw = ts.Values[0].ToInt32();
			int th = ts.Values[1].ToInt32();

			if (!(TilemapFormat.GetFormat(fmtName) is TilemapFormat fmt)) {
				logger?.Log("Unknown tilemap format \"" + fmtName + "\".", LogLevel.Error);
				return false;
			}
			if (ts.IsPresent && tw <= 0) {
				logger?.Log("Invalid tile width.", LogLevel.Error);
				return false;
			}
			if (ts.IsPresent && th <= 0) {
				logger?.Log("Invalid tile height.", LogLevel.Error);
				return false;
			}

			if (!append) {
				workbench.Tileset.Clear();
				workbench.Tilemap.Clear();
			}

			if (workbench.Tileset.Count > 0) {
				if (ts.IsPresent && (tw != workbench.Tileset.TileWidth || th != workbench.Tileset.TileHeight)) {
					logger?.Log("Specified tile size " + tw + "x" + th + " does not match tile size " +
						workbench.Tileset.TileWidth + "x" + workbench.Tileset.TileHeight + " of nonempty tileset.", LogLevel.Error);
					return false;
				}
			}

			// Use existing tile size if not specified
			if (tw == -1 && th == -1) {
				tw = workbench.Tileset.TileWidth;
				th = workbench.Tileset.TileHeight;
			}
			// Set tileset to new tile size
			if (workbench.Tileset.Count == 0) {
				workbench.Tileset.TileWidth = tw;
				workbench.Tileset.TileHeight = th;
			}

			int beforeCount = workbench.Tilemap.Count;

			Bitmap bmp = workbench.Bitmap.GetCroppedBitmap(x, y, w, h);
			try {
				if (fmt.IsIndexed) {
					if (workbench.PaletteSet.Count <= 0) {
						logger?.Log("Cannot generate indexed tiles: no palettes loaded.", LogLevel.Error);
						return false;
					}
					workbench.Tilemap.AddBitmapIndexed(bmp, workbench.Tileset, workbench.PaletteSet, fmt, !noReduce);
				} else {
					workbench.Tilemap.AddBitmap(bmp, workbench.Tileset, fmt, !noReduce);
				}
			} catch (Exception ex) {
				logger?.Log(ex.Message, LogLevel.Error);
				return false;
			}

			workbench.Tilemap.TilemapFormat = fmt;

			int addedCount = workbench.Tilemap.Count - beforeCount;
			logger?.Log("Generated " + addedCount + " tilemap entries.");
			return true;
		}
	}
}
