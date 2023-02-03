using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tests;

internal sealed class Program {
	private static int Main(string[] args) {
		Console.WriteLine("Starting PixelPet test runner...");
		ConsoleColor consoleColor = Console.ForegroundColor;

		// Set working directory
		string exePath = Assembly.GetExecutingAssembly().Location;
		if (!string.IsNullOrEmpty(exePath)) {
			string? exeDir = Path.GetDirectoryName(exePath);
			if (!string.IsNullOrEmpty(exeDir)) {
				Directory.SetCurrentDirectory(exeDir);
			}
		}
		string baseDir = Directory.GetCurrentDirectory();

		// Delete existing run tests
		if (Directory.Exists("TestsOut")) {
			Directory.Delete("TestsOut", true);
		}
		FileOps.CopyDirectory("TestsIn", "TestsOut", true);

		// Create test runners for every test
		List<TestRunner> tests = new();
		foreach (string testDir in Directory.GetDirectories("TestsOut")) {
			tests.Add(new(new(testDir)));
		}
		int testNameLen = Math.Max(tests.Max(t => t.Config.Name.Length) + 5, 70);

		// Run tests
		Console.WriteLine();
		foreach (TestRunner test in tests) {
			Console.Write((test.Config.Name + ' ').PadRight(testNameLen, '.') + ' ');

			///TODO: Could be done in parallel if TestRunner didn't change working directory
			test.Run();

			if (test.Passed) {
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("PASSED");
				Console.ForegroundColor = consoleColor;
				Console.WriteLine();
			} else {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("FAILED");
				Console.ForegroundColor = consoleColor;
				Console.WriteLine();
			}

#if DEBUG
	Console.WriteLine(test.CapturedConsoleCombined.ToString());
#else
			if (!test.Passed) {
				Console.WriteLine(test.CapturedConsoleCombined.ToString());
			} else {
				Console.WriteLine(test.CapturedConsoleError.ToString());
			}
#endif

			File.WriteAllText(Path.Combine(test.Config.Directory, "log_all.txt"), test.CapturedConsoleCombined.ToString());
			File.WriteAllText(Path.Combine(test.Config.Directory, "log_out.txt"), test.CapturedConsoleOut.ToString());
			File.WriteAllText(Path.Combine(test.Config.Directory, "log_err.txt"), test.CapturedConsoleError.ToString());
		}

		// Print test results
		int numPassed = tests.Where(t => t.Passed).Count();
		int numFailed = tests.Count - numPassed;
		Console.Write($"{numPassed} tests passed, {numFailed} tests failed ");
		if (numFailed == 0) {
			Console.ForegroundColor = ConsoleColor.Green;
		} else {
			Console.ForegroundColor = ConsoleColor.Red;
		}
		Console.WriteLine($"({(float)numPassed * 100 / tests.Count:0.#}%)");
		Console.ForegroundColor = consoleColor;

		// Return zero if all tests passed, else nonzero
		return numFailed;
	}
}