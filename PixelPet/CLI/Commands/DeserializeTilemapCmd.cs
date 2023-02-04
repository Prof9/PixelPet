using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal sealed class DeserializeTilemapCmd : CLICommand {
		public DeserializeTilemapCmd()
			: base("Deserialize-Tilemap",
				new Parameter(true, new ParameterValue("tilemap-format")),
				new Parameter("append", "a", false),
				new Parameter("base-tile", "bt", false, new ParameterValue("index", "0")),
				new Parameter("tile-count", "tc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string mapFmtName = FindUnnamedParameter(0).Values[0].ToString();
			bool append = FindNamedParameter("--append").IsPresent;
			int baseTile = FindNamedParameter("--base-tile").Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();
			int tileCount = FindNamedParameter("--tile-count").Values[0].ToInt32();

			if (TilemapFormat.GetFormat(mapFmtName) is not TilemapFormat mapFmt) {
				logger?.Log($"Unknown tilemap format {mapFmtName}.", LogLevel.Error);
				return false;
			}
			if (offset < 0) {
				logger?.Log("Invalid offset.", LogLevel.Error);
				return false;
			}
			if (tileCount < 0) {
				logger?.Log("Invalid tile count.", LogLevel.Error);
				return false;
			}

			if (!append) {
				workbench.Tilemap.Clear();
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

			// Bytes per tile entry.
			int bpe = (mapFmt.Bits + 7) / 8;

			byte[] buffer = new byte[bpe];
			int tn = 0;
			while (tileCount > 0 && workbench.Stream.Read(buffer, 0, bpe) == bpe) {
				int scrn = 0;
				for (int i = 0; i < bpe; i++) {
					scrn |= buffer[i] << i * 8;
				}

				int tileNum = mapFmt.BitmapEncoding switch {
					BitmapEncoding.NintendoDSTexture => tn,
					_ => ((scrn & mapFmt.TileNumberMask) >> mapFmt.TileNumberShift)
				} - baseTile;
				if (tileNum < 0) {
					tileNum = int.MaxValue;
				}

				TileEntry te = new(
					tileNumber: tileNum,
					hFlip: mapFmt.CanFlipHorizontal && (scrn & mapFmt.FlipHorizontalMask) != 0,
					vFlip: mapFmt.CanFlipVertical && (scrn & mapFmt.FlipVerticalMask) != 0,
					paletteNumber: (scrn & mapFmt.PaletteMask) >> mapFmt.PaletteShift,
					mode: (scrn & mapFmt.ModeMask) >> mapFmt.ModeShift
				);

				workbench.Tilemap.Add(te);
				
				tileCount--;
				tn++;
			}

			workbench.Tilemap.TilemapFormat = mapFmt;
			return true;
		}
	}
}
