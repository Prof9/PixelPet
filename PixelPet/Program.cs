using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet {
	class Program {
		static int Main(string[] args) {
			Workbench workbench = new Workbench();
			Cli cli = new Cli(workbench);
			int r = cli.Run(((IEnumerable<string>)args).GetEnumerator());
			Console.WriteLine("Done.");
#if DEBUG
			Console.ReadKey();
#endif
			return r;
		}
	}
}
