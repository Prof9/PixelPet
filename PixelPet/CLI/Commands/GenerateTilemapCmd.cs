﻿using LibPixelPet;
using System;
using System.Drawing;

namespace PixelPet.CLI.Commands {
	internal class GenerateTilemapCmd : CliCommand {
		public GenerateTilemapCmd()
			: base("Generate-Tilemap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("no-reduce", "nr", false),
				new Parameter("indexed", "i", false),
				new Parameter("x", "x", false, new ParameterValue("pixels", "0")),
				new Parameter("y", "y", false, new ParameterValue("pixels", "0")),
				new Parameter("width", "w", false, new ParameterValue("pixels", "-1")),
				new Parameter("height", "h", false, new ParameterValue("pixels", "-1"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].Value;
			bool noReduce = FindNamedParameter("--no-reduce").IsPresent;
			bool indexed = FindNamedParameter("--indexed").IsPresent;
			int x = FindNamedParameter("--x").Values[0].ToInt32();
			int y = FindNamedParameter("--y").Values[0].ToInt32();
			int w = FindNamedParameter("--width").Values[0].ToInt32();
			int h = FindNamedParameter("--height").Values[0].ToInt32();

			if (!(TilemapFormat.GetFormat(fmtName) is TilemapFormat fmt)) {
				logger?.Log("Unknown tilemap format \"" + fmtName + "\".", LogLevel.Error);
				return;
			}

			int beforeCount = workbench.Tilemap.Count;

			using (Bitmap bmp = workbench.GetCroppedBitmap(x, y, w, h, logger)) {
				if (indexed) {
					workbench.Tilemap.AddBitmapIndexed(bmp, workbench.Tileset, workbench.PaletteSet, fmt, !noReduce);
				} else {
					workbench.Tilemap.AddBitmap(bmp, workbench.Tileset, fmt, !noReduce);
				}
			}

			workbench.TilemapFormat = fmt;

			int addedCount = workbench.Tilemap.Count - beforeCount;
			logger?.Log("Generated " + addedCount + " tilemap entries.");
		}
	}
}
