using PixelPet.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet {
	/// <summary>
	/// Command line interface acting on a PixelPet workbench.
	/// </summary>
	public class Cli {
		private static readonly IEnumerable<CliCommand> Commands = new CliCommand[] {
			new ImportBitmapCmd(),
			new ExportBitmapCmd(),
			new ExtractPaletteCmd(),
			new RenderPaletteCmd(),
		};

		public Workbench Workbench { get; }

		/// <summary>
		/// Creates a new command line interface acting on the specified workbench.
		/// </summary>
		/// <param name="workbench">The workbench to act on.</param>
		public Cli(Workbench workbench) {
			this.Workbench = workbench;

			this.Log("PixelPet v0.1-alpha by Prof. 9");
		}


		public int Run(IEnumerator<string> args) {
#if !DEBUG
			try {
#endif
				bool first = true;
				while (true) {
					if (first) {
						if (!args.MoveNext()) {
							break;
						}
						first = false;
					}
					if (!DoCommand(args)) {
						break;
					}
				}
#if !DEBUG
			} catch (Exception ex) {
				Log(ex.Message);
				return 1;
			}
#endif

			return 0;
		}

		private bool DoCommand(IEnumerator<string> args) {
			string cmdName = args.Current;

			CliCommand cmd = Commands.FirstOrDefault(c => c.Name.Equals(cmdName, StringComparison.InvariantCultureIgnoreCase));
			if (cmd == null) {
				throw new ArgumentException("Unrecognized command \"" + cmdName + "\"", nameof(args));
			}

			cmd.Prepare(args);
			cmd.Run(this.Workbench, this);
			return !cmd.ReachedEnd;
		}

		public void Log(string str) {
			Console.WriteLine(str);
		}
	}
}
