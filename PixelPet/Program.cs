using PixelPet.CLI;
using System;
using System.Collections.Generic;

namespace PixelPet {
	internal class Program {
		public static string Version => "v1.0-alpha2";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
		static int Main(string[] args) {
			Program program = new Program();
			program.Run(args);
			return program.ErrorLevel;
		}

		public int ErrorLevel { get; private set; }

		public void Run(string[] args)
			=> this.RunCli(args ?? new string[0]);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
		private void RunCli(string[] args) {
			string consoleTitle = Console.Title;
			Console.Title = "PixelPet";
			Console.WriteLine($"PixelPet " + Version + " by Prof. 9");
			Console.WriteLine();

			Workbench workbench = new Workbench();
			Cli cli = new Cli(workbench);

			cli.Run(((IEnumerable<string>)args).GetEnumerator());

			if ((int)cli.MaximumLogLevel > this.ErrorLevel) {
				this.ErrorLevel = (int)cli.MaximumLogLevel;
			}

			Console.WriteLine("Done.");
#if DEBUG
			Console.ReadKey();
#endif
			Console.Title = consoleTitle;
		}
	}
}
