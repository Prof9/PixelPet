using System;
using System.IO;
using System.Linq;

namespace Tests {
	internal sealed record TestConfig {
		/// <summary>
		/// Gets directory where the test is stored.
		/// </summary>
		public string Directory { get; }
		/// <summary>
		/// Gets path of PixelPet script file to run.
		/// </summary>
		public string ScriptToRun { get; init; }
		/// <summary>
		/// Gets name of test.
		/// </summary>
		public string Name { get; init; }

		public TestConfig(string directory) {
			ArgumentNullException.ThrowIfNull(directory);

			Directory = Path.GetFullPath(directory);
			Name = directory.Split(Path.DirectorySeparatorChar).Last();
			ScriptToRun = Path.Combine(Directory, "script.txt");
		}
	}
}
