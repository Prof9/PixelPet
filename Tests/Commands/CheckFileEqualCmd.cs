﻿using PixelPet;
using PixelPet.CLI;

namespace Tests.Commands {
	internal class CheckFileEqualCmd : CliCommand {
		public CheckFileEqualCmd()
			: base("Check-File-Equal",
				new Parameter(true, new ParameterValue("file1")),
				new Parameter(true, new ParameterValue("file2"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			string file1 = this.FindUnnamedParameter(0).Values[0].ToString();
			string file2 = this.FindUnnamedParameter(1).Values[0].ToString();

			using FileStream fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
			using FileStream fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (fs1.Length != fs2.Length) {
				logger?.Log($"{file1} size {fs1.Length} != {file2} size {fs2.Length}", LogLevel.Error);
				return;
			}

			Span<byte> buffer1 = stackalloc byte[8];
			Span<byte> buffer2 = stackalloc byte[8];

			while (fs1.Position < fs1.Length) {
				fs1.Read(buffer1);
				fs2.Read(buffer2);

				ulong val1 = BitConverter.ToUInt64(buffer1);
				ulong val2 = BitConverter.ToUInt64(buffer2);
				if (val1 != val2) {
					logger?.Log($"{file1} {val1:0xX8} != {file2} {val2:0xX8}", LogLevel.Error);
					return;
				}
			}

			logger?.Log($"{file1} equals {file2}");
		}
	}
}