using System;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal sealed class HelpCmd : Command {
		public HelpCmd()
			: base ("Help") { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			Console.WriteLine();
			Console.WriteLine("Supported commands:");
			Console.WriteLine();
			foreach (Command cmd in CommandRunner.InternalCommands.Union(Runner.RegisteredCommands ?? Array.Empty<Command>())) {
				Console.WriteLine(cmd.Name);
				Console.Write("\t");
				bool first = true;
				foreach (Parameter param in cmd.Parameters) {
					if (!first) {
						Console.Write(' ');
					}
					first = false;

					if (!param.IsRequired) {
						Console.Write('[');
					}

					if (param.IsNamed) {
						if (param.LongName is not null) {
							Console.Write("--" + param.LongName);
						}
						if (param.LongName is not null && param.ShortName is not null) {
							Console.Write('/');
						}
						if (param.ShortName is not null) {
							Console.Write('-' + param.ShortName);
						}
					}

					if (param.IsNamed && param.Values.Any()) {
						Console.Write(' ');
					}

					Console.Write(string.Join(" ", param.Values.Select(v => '<' + v.Name + '>')));

					if (!param.IsRequired) {
						Console.Write(']');
					}
				}
				if (!first) {
					Console.WriteLine();
				}
				Console.WriteLine();
			}
			Console.WriteLine();
			return true;
		}
	}
}
