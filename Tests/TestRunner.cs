
using PixelPet;
using PixelPet.CLI;
using Tests.Commands;

namespace Tests {
	internal class TestRunner {
		/// <summary>
		/// Gets the test configuration.
		/// </summary>
		public TestConfig Config { get; }
		/// <summary>
		/// Gets whether the test passed.
		/// </summary>
		public bool Passed { get; private set; }
		/// <summary>
		/// Gets the combined captured console.
		/// </summary>
		public TextWriter CapturedConsoleCombined { get; }
		/// <summary>
		/// Gets the captured Console.Out.
		/// </summary>
		public TextWriter CapturedConsoleOut { get; }
		/// <summary>
		/// Gets the captured Console.Error.
		/// </summary>
		public TextWriter CapturedConsoleError { get; }

		/// <summary>
		/// Gets the workbench being operated on.
		/// </summary>
		private Workbench Workbench { get; }
		/// <summary>
		/// Gets the CLI being run.
		/// </summary>
		private Cli Cli { get; }

		/// <summary>
		/// Creates a new test runner with the given configuration.
		/// </summary>
		/// <param name="testConfig">Test configuration.</param>
		public TestRunner(TestConfig testConfig) {
			this.Config = testConfig;
			this.Passed = false;

			this.CapturedConsoleCombined = new StringWriter();
			this.CapturedConsoleOut = new PassThroughBufferTextWriter(this.CapturedConsoleCombined);
			this.CapturedConsoleError = new PassThroughBufferTextWriter(this.CapturedConsoleCombined);

			this.Workbench = new();
			this.Cli = new(this.Workbench) {
				ConsoleOut = this.CapturedConsoleOut,
				ConsoleError = this.CapturedConsoleError,
			};
			this.Cli.RegisterCommand(new CheckFileEqualCmd());
		}

		/// <summary>
		/// Runs the 
		/// </summary>
		public void Run() {
			this.Passed = false;

			// Run test script
			Directory.SetCurrentDirectory(this.Config.Directory); ///TODO: Not thread safe
			try {
				this.Cli.Run(new string[] { "run-script", Config.ScriptToRun });

				if (this.Cli.MaximumLogLevel < LogLevel.Error) {
					this.Passed = true;
				}
			} catch (Exception ex) {
				this.CapturedConsoleError.WriteLine(ex.ToString());
			}

			this.CapturedConsoleOut.Flush();
			this.CapturedConsoleError.Flush();
			this.CapturedConsoleCombined.Flush();
		}
	}
}
