using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class SerializeTilemapCmd : CliCommand {
		public SerializeTilemapCmd()
			: base("Serialize-Tilemap",
				new Parameter("append", "a", false),
				new Parameter("base-tile", "bt", false, new ParameterValue("index", "0")),
				new Parameter("first-tile", "ft", false, new ParameterValue("tilemap-entry", "-1"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			bool append = FindNamedParameter("--append").IsPresent;
			int baseTile = FindNamedParameter("--base-tile").Values[0].ToInt32();
			int firstTile = FindNamedParameter("--first-tile").Values[0].ToInt32();

			TilemapFormat fmt = workbench.Tilemap.TilemapFormat;

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			// Get byte count of single entry.
			int size = (fmt.Bits + 7) / 8;

			foreach (TileEntry te in workbench.Tilemap) {
				int scrn = 0;
				if (firstTile >= 0 && te.TileNumber == 0) {
					scrn = firstTile;
				} else {
					// Create entry.
					scrn |= ((te.TileNumber + baseTile) << fmt.TileNumberShift) & fmt.TileNumberMask;
					scrn |= te.HFlip && fmt.CanFlipHorizontal ? fmt.FlipHorizontalMask : 0;
					scrn |= te.VFlip && fmt.CanFlipVertical   ? fmt.FlipVerticalMask   : 0;
					scrn |= (te.PaletteNumber << fmt.PaletteShift) & fmt.PaletteMask;
				}

				// Write to stream.
				for (int i = 0; i < size; i++) {
					workbench.Stream.WriteByte((byte)scrn);
					scrn >>= 8;
				}
			}

			logger?.Log("Serialized tilemap of length " + workbench.Tilemap.Count + ".", LogLevel.Information);
			return true;
		}
	}
}
