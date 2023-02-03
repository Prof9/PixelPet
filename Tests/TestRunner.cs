using PixelPet;
using PixelPet.CLI;
using System;
using System.IO;
using Tests.Commands;

namespace Tests {
	internal sealed class TestRunner {
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
		private CommandRunner Cli { get; }

		/// <summary>
		/// Creates a new test runner with the given configuration.
		/// </summary>
		/// <param name="testConfig">Test configuration.</param>
		public TestRunner(TestConfig testConfig) {
			Config = testConfig;
			Passed = false;

			CapturedConsoleCombined = new StringWriter();
			CapturedConsoleOut = new PassThroughBufferTextWriter(CapturedConsoleCombined);
			CapturedConsoleError = new PassThroughBufferTextWriter(CapturedConsoleCombined);

			Workbench = new();
			Cli = new(Workbench) {
				ConsoleOut = CapturedConsoleOut,
				ConsoleError = CapturedConsoleError,
			};
			Cli.RegisterCommand(new CheckFileEqualCmd());
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		public void Run() {
			Passed = false;

			// Run test script
			Directory.SetCurrentDirectory(Config.Directory); ///TODO: Not thread safe
			try {
				Cli.Run(new string[] { "run-script", Config.ScriptToRun });

				if (Cli.MaximumLogLevel < LogLevel.Error) {
					Passed = true;
				}
#pragma warning disable CA1031 // Do not catch general exception types
			} catch (Exception ex) {
#pragma warning restore CA1031 // Do not catch general exception types
				CapturedConsoleError.WriteLine(ex.ToString());
			}

			CapturedConsoleOut.Flush();
			CapturedConsoleError.Flush();
			CapturedConsoleCombined.Flush();
		}
	}
}
