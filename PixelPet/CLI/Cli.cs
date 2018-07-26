using PixelPet.CLI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixelPet.CLI {
	internal class Cli : ILogger {
		protected static readonly IEnumerable<CliCommand> Commands = new ReadOnlyCollection<CliCommand>(new CliCommand[] {
			new ImportBitmapCmd(),
			new ExportBitmapCmd(),
			new ImportBytesCmd(),
			new ExportBytesCmd(),
			new ClearPalettesCmd(),
			new ClearTilesetCmd(),
			new ClearTilemapCmd(),
			new ReadPalettesCmd(),
			new RenderPalettesCmd(),
			new RenderTilesetCmd(),
			new RenderTilemapCmd(),
			new CropBitmapCmd(),
			new ConvertBitmapCmd(),
			new ConvertPalettesCmd(),
			new DeduplicatePalettesCmd(),
			new PadPalettesCmd(),
			new PadTilesetCmd(),
			new GenerateTilemapCmd(),
			new DeserializePalettesCmd(),
			new SerializePalettesCmd(),
			new DeserializeTilesetCmd(),
			new SerializeTilesetCmd(),
			new DeserializeTilemapCmd(),
			new SerializeTilemapCmd(),
		});

		public LogLevel MaximumLogLevel { get; private set; }
		public bool Verbose { get; private set; }
		public Workbench Workbench { get; }

		/// <summary>
		/// Creates a new command line interface acting on the specified workbench.
		/// </summary>
		/// <param name="workbench">The workbench to act on.</param>
		public Cli(Workbench workbench) {
			this.Workbench = workbench;
			this.ResetLogLevel();
			this.ResetVerbosity();
		}

		public void ResetLogLevel() {
			this.MaximumLogLevel = 0;
		}

		public void Run(IEnumerator<string> args) {
			if (args == null)
				throw new ArgumentNullException(nameof(args));

#if !DEBUG
			try {
#endif
				bool first = true;
				while (true) {
					if (this.MaximumLogLevel >= LogLevel.Error) {
						return;
					}
					
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
				this.Log(ex.Message, LogLevel.Exception);
			}
#endif
		}

		private bool DoCommand(IEnumerator<string> args) {
			string cmdName = args.Current;

			CliCommand cmd = Commands
				.FirstOrDefault(c => c.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase));

			if (cmd == null) {
				this.Log("Unrecognized command \"" + cmdName + "\".", LogLevel.Error);
				return false;
			}

			cmd.Prepare(args);

			this.Log("Running command: " + cmd.ToString() + "...", LogLevel.VerboseInformation);

			cmd.Run(this.Workbench, this);
			return !cmd.ReachedEnd;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.Write(System.String)")]
		public void Log(string str, LogLevel level) {
			if (level < 0 && !this.Verbose) {
				return;
			}

			if (level > this.MaximumLogLevel) {
				this.MaximumLogLevel = level;
			}

			ConsoleColor color = Console.ForegroundColor;

			switch (level) {
			case LogLevel.Exception:
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("FATAL: ");
				break;
			case LogLevel.Error:
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("ERROR: ");
				break;
			case LogLevel.Warning:
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("WARNING: ");
				break;
			}

			Console.ForegroundColor = color;
			Console.WriteLine(str);
		}

		public void SetVerbosity(bool verbose)
			=> this.Verbose = verbose;
		public void ResetVerbosity()
			=> this.Verbose = false;
	}
}
