﻿using LibPixelPet;
using System;

namespace PixelPet.CLI.Commands {
	internal sealed class DeserializePalettesCmd : Command {
		public DeserializePalettesCmd()
			: base("Deserialize-Palettes",
				new Parameter(true, new ParameterValue("format")),
				new Parameter("append", "a", false),
				new Parameter("palette-number", "pn", false, new ParameterValue("number", "-1")),
				new Parameter("palette-size", "ps", false, new ParameterValue("count", "-1")),
				new Parameter("palette-count", "pc", false, new ParameterValue("count", "" + int.MaxValue)),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string fmtName = GetUnnamedParameter(0).Values[0].ToString();
			bool append = GetNamedParameter("--append").IsPresent;
			int palNum = GetNamedParameter("--palette-number").Values[0].ToInt32();
			int palSize = GetNamedParameter("--palette-size").Values[0].ToInt32();
			int palCount = GetNamedParameter("--palette-count").Values[0].ToInt32();
			long offset = GetNamedParameter("--offset").Values[0].ToInt64();

			if (palNum < -1) {
				logger?.Log("Invalid palette number.", LogLevel.Error);
				return false;
			}
			if (palSize == 0 || palSize < -1) {
				logger?.Log("Invalid palette size.", LogLevel.Error);
				return false;
			}
			if (palCount < 0) {
				logger?.Log("Invalid palette count.", LogLevel.Error);
				return false;
			}
			if (offset < 0) {
				logger?.Log("Invalid offset.", LogLevel.Error);
				return false;
			}
			if (ColorFormat.GetFormat(fmtName) is not ColorFormat fmt) {
				logger?.Log($"Unknown color format {fmtName}.", LogLevel.Error);
				return false;
			}

			if (!append) {
				workbench.PaletteSet.Clear();
			}

			workbench.Stream.Position = Math.Min(offset, workbench.Stream.Length);

			// Bytes per color.
			int bpc = (fmt.Bits + 7) / 8;

			int addedColors = 0;
			int addedPalettes = 0;
			byte[] buffer = new byte[4];
			Palette? pal = null;
			bool formatWarning = false;
			while (palCount > 0 && workbench.Stream.Read(buffer, 0, bpc) == bpc) {
				// Create new palette if needed.
				pal ??= new Palette(fmt, palSize);

				// Add color to palette.
				int c = BitConverter.ToInt32(buffer, 0);
				int c2 = fmt.Convert(c, fmt);
				if (c != c2 && !formatWarning) {
					logger?.Log($"Color {addedColors} in palette {addedPalettes} read as 0x{c:X} but parsed as 0x{c2:X}; color format may not be correct", LogLevel.Warning);
					formatWarning = true;
				}
				pal.Add(c2);
				addedColors++;

				// Add finished palette to palette set.
				if (palSize != -1 && pal.Count >= palSize) {
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
			if (pal is not null) {
				if (palNum < 0) {
					workbench.PaletteSet.Add(pal);
				} else {
					workbench.PaletteSet.Add(pal, palNum++);
				}
				addedPalettes++;
			}

			if (GetNamedParameter("--palette-size").IsPresent) {
				logger?.Log($"Deserialized {addedPalettes} palettes with {palSize} colors each ({addedColors} colors total).");
			} else {
				logger?.Log($"Deserialized {addedPalettes} palettes with {addedColors} colors total.");
			}
			return true;
		}
	}
}
