using PixelPet.CLI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PixelPet {
	[AttributeUsage(AttributeTargets.Assembly)]
	internal class AssemblyBuildInfoAttribute : Attribute
	{
		public string Timestamp { get; }
		public string CommitID { get; }
		public AssemblyBuildInfoAttribute(string timestamp, string commitID)
		{
			Timestamp = timestamp;
			CommitID = commitID;
		}
	}

	internal class Program {
		public static string Version => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		public static AssemblyBuildInfoAttribute BuildInfo => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyBuildInfoAttribute>();

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
			Console.WriteLine($"PixelPet {Version} by Prof. 9 ({BuildInfo.CommitID} @ {BuildInfo.Timestamp})");
			Console.WriteLine();

			Workbench workbench = new Workbench();
			Cli cli = new Cli(workbench);

			cli.Run(args);

			if ((int)cli.MaximumLogLevel > this.ErrorLevel) {
				this.ErrorLevel = (int)cli.MaximumLogLevel;
			}

			if (this.ErrorLevel <= (int)LogLevel.Warning) {
				Console.WriteLine("Done.");
			} else {
				Console.WriteLine("Aborted.");
			}
#if DEBUG
			Console.ReadKey();
#endif
			Console.Title = consoleTitle;
		}
	}
}
