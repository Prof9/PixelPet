namespace Tests {
	internal record TestConfig {
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

			this.Directory = Path.GetFullPath(directory);
			this.Name = directory.Split(Path.DirectorySeparatorChar).Last();
			this.ScriptToRun = Path.Combine(this.Directory, "script.txt");
		}
	}
}
