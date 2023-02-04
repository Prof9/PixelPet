using LibPixelPet;
using System;
using System.IO;

namespace PixelPet.CLI.Commands {
	internal sealed class ImportBytesCmd : CLICommand {
		public ImportBytesCmd()
			: base("Import-Bytes",
				new Parameter(true, new ParameterValue("path")),
				new Parameter("append", "a", false),
				new Parameter("offset", "o", false, new ParameterValue("count", "0")),
				new Parameter("length", "l", false, new ParameterValue("count", "" + long.MaxValue))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();
			bool append = FindNamedParameter("--append").IsPresent;
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();
			long length = FindNamedParameter("--length").Values[0].ToInt64();

			if (offset < 0) {
				logger?.Log("Invalid offset.", LogLevel.Error);
				return false;
			}
			if (length < 0) {
				logger?.Log("Invalid length.", LogLevel.Error);
				return false;
			}

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			try {
				if (!File.Exists(path)) {
					logger?.Log($"File not found: {Path.GetFileName(path)}", LogLevel.Error);
					return false;
				}
				using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
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
						logger?.Log($"Error while reading {Path.GetFileName(path)}", LogLevel.Error);
						return false;
					}

					workbench.Stream.Write(buffer, 0, read);
				}
#pragma warning disable CA1031 // Do not catch general exception types
			} catch {
#pragma warning restore CA1031 // Do not catch general exception types
				logger?.Log($"Could not import {Path.GetFileName(path)}", LogLevel.Error);
				return false;
			}

			logger?.Log($"Imported {length} bytes from binary {Path.GetFileName(path)}.", LogLevel.Information);
			return true;
		}
	}
}
