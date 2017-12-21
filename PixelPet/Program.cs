using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet {
	class Program {
		public static string Version => "v1.0-alpha";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
		static int Main(string[] args) {
			Console.Title = "PixelPet CLI";
			Console.WriteLine("PixelPet " + Version + " by Prof. 9");
			Console.WriteLine();

			Workbench workbench = new Workbench();

			int exitCode = new Cli(workbench).Run(((IEnumerable<string>)args).GetEnumerator());

			Console.WriteLine("Done.");
#if DEBUG
			Console.ReadKey();
#endif
			return exitCode;
		}
	}
}
