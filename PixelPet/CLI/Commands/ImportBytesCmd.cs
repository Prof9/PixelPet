using LibPixelPet;
using System;
using System.IO;

namespace PixelPet.CLI.Commands {
	internal class ImportBytesCmd : CliCommand {
		public ImportBytesCmd()
			: base("Import-Bytes",
				new Parameter(true, new ParameterValue("path")),
				new Parameter("offset", "o", false, new ParameterValue("count", "0")),
				new Parameter("length", "l", false, new ParameterValue("count", "" + long.MaxValue))
			) { }

		public override void Run(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();
			long offset = this.FindNamedParameter("--offset").Values[0].ToInt64();
			long length = this.FindNamedParameter("--length").Values[0].ToInt64();

			if (offset < 0) {
				logger?.Log("Invalid offset.", LogLevel.Error);
				return;
			}
			if (length < 0) {
				logger?.Log("Invalid length.", LogLevel.Error);
				return;
			}

			workbench.Stream.SetLength(0);

			try {
				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					if (offset >= fs.Length) {
						logger?.Log("Offset past the end of the file.", LogLevel.Warning);
					}

					length = Math.Max(Math.Min(length, fs.Length - offset), 0);

					if (offset == 0 && length >= fs.Length) {
						fs.CopyTo(workbench.Stream);
					} else {
						byte[] buffer = new byte[length];
						fs.Position = offset;
						int read = fs.Read(buffer, 0, buffer.Length);

						if (read < length) {
							logger?.Log("Error while reading " + Path.GetFileName(path), LogLevel.Error);
							return;
						}

						workbench.Stream.Write(buffer, 0, read);
					}
				}
			} catch (IOException) {
				logger?.Log("Could not read file " + Path.GetFileName(path), LogLevel.Error);
				return;
			}

			logger?.Log("Imported " + length + " bytes from binary " + Path.GetFileName(path), LogLevel.Information);
		}
	}
}
