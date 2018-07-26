﻿using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;

namespace PixelPet.CLI.Commands {
	internal class ImportBitmapCmd : CliCommand {
		public ImportBitmapCmd()
			: base("Import-Bitmap",
				new Parameter(true, new ParameterValue("path"))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();

			try {
				using (Bitmap bmp = new Bitmap(path)) {
					workbench.ClearBitmap(bmp.Width, bmp.Height);
					workbench.Graphics.CompositingMode = CompositingMode.SourceCopy;
					workbench.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
				}
			} catch (IOException) {
				logger?.Log("Could not read file " + Path.GetFileName(path), LogLevel.Error);
				return;
			}
			workbench.Graphics.Flush();

			logger?.Log("Imported bitmap " + Path.GetFileName(path), LogLevel.Information);
		}
	}
}