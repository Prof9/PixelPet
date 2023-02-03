using PixelPet.CLI;
using System;
using System.Reflection;

namespace PixelPet {
	[AttributeUsage(AttributeTargets.Assembly)]
	internal sealed class AssemblyBuildInfoAttribute : Attribute
	{
		public string Timestamp { get; }
		public string CommitID { get; }
		public AssemblyBuildInfoAttribute(string timestamp, string commitID)
		{
			Timestamp = timestamp;
			CommitID = commitID;
		}
	}

	internal sealed class Program {
		public static string Version => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		public static AssemblyBuildInfoAttribute BuildInfo => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyBuildInfoAttribute>();

		static int Main(string[] args) {
			Program program = new();
			program.Run(args);
			return program.ErrorLevel;
		}

		public int ErrorLevel { get; private set; }

		public void Run(string[] args)
			=> RunCli(args ?? Array.Empty<string>());

		private void RunCli(string[] args) {
			string consoleTitle = null;
			if (OperatingSystem.IsWindows()) {
				consoleTitle = Console.Title;
				Console.Title = "PixelPet";
			}
			Console.WriteLine($"PixelPet {Version} by Prof. 9 ({BuildInfo.CommitID} @ {BuildInfo.Timestamp})");
			Console.WriteLine();

			Workbench workbench = new();
			CommandRunner cli = new(workbench);

			cli.Run(args);

			if ((int)cli.MaximumLogLevel > ErrorLevel) {
				ErrorLevel = (int)cli.MaximumLogLevel;
			}

			if (ErrorLevel <= (int)LogLevel.Warning) {
				Console.WriteLine("Done.");
			} else {
				Console.WriteLine("Aborted.");
			}
#if DEBUG
			Console.ReadKey();
#endif
			if (consoleTitle is not null) {
				Console.Title = consoleTitle;
			}
		}
	}
}
