using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PixelPet.CLI.Commands {
	internal sealed class ViewLicensesCmd : Command {
		public ViewLicensesCmd()
			: base("View-Licenses") { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			Assembly assembly = Assembly.GetExecutingAssembly();

			// Find all embedded license files using regex
			// Sort them without file extension
			IEnumerable<string> licenseFiles = assembly.GetManifestResourceNames()
				.Where(x => x.StartsWith("PixelPet.Licenses.", StringComparison.OrdinalIgnoreCase))
				.OrderBy(x => x[x.LastIndexOf('.')..]);

			// Print each license to console
			bool first = true;
			foreach (string licenseFile in licenseFiles) {
				if (!first) {
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine();
				}
				using (StreamReader sr = new(assembly.GetManifestResourceStream(licenseFile)!)) {
					string? line;
					while ((line = sr.ReadLine()) is not null) {
						Console.WriteLine(line);
					}
				}
				first = false;
			}
			Console.WriteLine();

			return true;
		}
	}
}
