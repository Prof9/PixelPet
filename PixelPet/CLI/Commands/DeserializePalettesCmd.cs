using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal class DeserializePalettesCmd : CliCommand {
		public DeserializePalettesCmd()
			: base("Deserialize-Palettes",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("palette-number", "pn", false, new ParameterValue("number", "-1")),
				new Parameter("palette-size", "ps", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("palette-count", "pc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].Value;
			int palNum = FindNamedParameter("--palette-number").Values[0].ToInt32();
			int palSize = FindNamedParameter("--palette-size").Values[0].ToInt32();
			int palCount = FindNamedParameter("--palette-count").Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();

			if (palNum < -1) {
				logger?.Log("Invalid palette number.", LogLevel.Error);
				return;
			}
			if (palSize < 1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return;
			}
			if (palCount < 0) {
				logger?.Log("Invalid palette count.", LogLevel.Error);
				return;
			}
			if (offset < 0) {
				logger?.Log("Invalid offset.", LogLevel.Error);
				return;
			}
			if (!(ColorFormat.GetFormat(fmtName) is ColorFormat fmt)) {
				logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
				return;
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

			// Bytes per color.
			int bpc = (fmt.Bits + 7) / 8;

			int addedColors = 0;
			int addedPalettes = 0;
			byte[] buffer = new byte[4];
			Palette pal = null;
			while (palCount > 0 && workbench.Stream.Read(buffer, 0, bpc) == bpc) {
				// Create new palette if needed.
				if (pal == null) {
					pal = new Palette(fmt, palSize);
				}

				// Add color to palette.
				int c = BitConverter.ToInt32(buffer, 0);
				pal.Add(c);
				addedColors++;

				// Add finished palette to palette set.
				if (pal.Count >= palSize) {
					if (palNum < 0) {
						workbench.PaletteSet.Add(pal);
					} else {
						workbench.PaletteSet.Add(pal, palNum++);
					}
					addedPalettes++;
					pal = null;

					if (--palCount <= 0) {
						break;
					}
				}
			}
			if (pal != null) {
				if (palNum < 0) {
					workbench.PaletteSet.Add(pal);
				} else {
					workbench.PaletteSet.Add(pal, palNum++);
				}
				addedPalettes++;
			}

			if (FindNamedParameter("--palette-size").IsPresent) {
				logger?.Log("Deserialized " + addedPalettes + " palettes with " + palSize + " colors each (" + addedColors + " colors total).");
			} else {
				logger?.Log("Deserialized " + addedPalettes + " palettes with " + addedColors + " colors total.");
			}
		}
	}
}
