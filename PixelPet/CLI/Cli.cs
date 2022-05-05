using PixelPet.CLI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PixelPet.CLI {
	public class Cli : ILogger {
		internal IList<CliCommand> Commands = new List<CliCommand>() {
			new HelpCmd(),
			new SetVariableCmd(),
			new RunScriptCmd(),
			new ImportBitmapCmd(),
			new ExportBitmapCmd(),
			new ImportBytesCmd(),
			new ExportBytesCmd(),
			new ClearPalettesCmd(),
			new ClearTilesetCmd(),
			new ClearTilemapCmd(),
			new ExtractPalettesCmd(),
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
			new DeserializeBitmapCmd(),
			new SerializeBitmapCmd(),
			new ApplyPaletteBitmapCmd(),
			new QuantizeBitmapCmd(),
		};

		public LogLevel MaximumLogLevel { get; private set; }
		public bool Verbose { get; private set; }
		public Workbench Workbench { get; }
		public TextWriter ConsoleOut { get; set; }
		public TextWriter ConsoleError { get; set; }
		public IDictionary<string, string> Variables { get; }

		/// <summary>
		/// Creates a new command line interface acting on the specified workbench.
		/// </summary>
		/// <param name="workbench">The workbench to act on.</param>
		public Cli(Workbench workbench) {
			if (workbench is null)
				throw new ArgumentNullException(nameof(workbench));

			this.ConsoleOut = Console.Out;
			this.ConsoleError = Console.Error;

			this.Variables = new Dictionary<string, string>();
			this.Workbench = workbench;
			this.ResetLogLevel();
			this.ResetVerbosity();
		}

		/// <summary>
		/// Registers an additional command on the CLI.
		/// </summary>
		/// <param name="command">Command to register.</param>
		public void RegisterCommand(CliCommand command) {
			if (command is null)
				throw new ArgumentNullException(nameof(command));

			this.Commands.Add(command);
		}

		/// <summary>
		/// Resets the maximum log level encountered.
		/// </summary>
		public void ResetLogLevel() {
			this.MaximumLogLevel = 0;
		}

		/// <summary>
		/// Run commands from the given arguments.
		/// </summary>
		/// <param name="args">Arguments containing commands to run.</param>
		public void Run(IEnumerable<string> args) => Run(args.GetEnumerator());

		internal void Run(IEnumerator<string> args) {
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

			cmd.Prepare(this, args);

			this.Log("Running command: " + cmd.ToString() + "...", LogLevel.VerboseInformation);

			if (!cmd.Run(this.Workbench, this)) {
				return false;
			}
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

			TextWriter console = this.ConsoleOut;
			if (level >= LogLevel.Exception) {
				console = this.ConsoleError;
				if (console == Console.Error) {
					Console.ForegroundColor = ConsoleColor.Red;
				}
				console.Write("FATAL: ");
			} else if (level == LogLevel.Error) {
				console = this.ConsoleError;
				if (console == Console.Error) {
					Console.ForegroundColor = ConsoleColor.Red;
				}
				console.Write("ERROR: ");
			} else if (level == LogLevel.Warning) {
				console = this.ConsoleError;
				if (console == Console.Error) {
					Console.ForegroundColor = ConsoleColor.Yellow;
				}
				console.Write("WARNING: ");
			}
			Console.ForegroundColor = color;

			console.WriteLine(str);
		}

		public void SetVerbosity(bool verbose)
			=> this.Verbose = verbose;
		public void ResetVerbosity()
			=> this.Verbose = false;
	}
}
