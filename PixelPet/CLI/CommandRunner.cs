using PixelPet.CLI.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelPet.CLI {
	public class CommandRunner : ILogger {
		internal static readonly Command[] InternalCommands = new Command[] {
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
		internal IList<Command>? RegisteredCommands { get; private set; }

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
		public CommandRunner(Workbench workbench) {
			ArgumentNullException.ThrowIfNull(workbench);

			ConsoleOut = Console.Out;
			ConsoleError = Console.Error;

			Variables = new Dictionary<string, string>();
			Workbench = workbench;
			ResetLogLevel();
			ResetVerbosity();
		}

		/// <summary>
		/// Registers an additional command on the CLI.
		/// </summary>
		/// <param name="command">Command to register.</param>
		public void RegisterCommand(Command command) {
			ArgumentNullException.ThrowIfNull(command);

			RegisteredCommands ??= new List<Command>();
			RegisteredCommands.Add(command);
		}

		/// <summary>
		/// Resets the maximum log level encountered.
		/// </summary>
		public void ResetLogLevel() {
			MaximumLogLevel = 0;
		}

		/// <summary>
		/// Run commands from the given arguments.
		/// </summary>
		/// <param name="args">Arguments containing commands to run.</param>
		public void Run(IEnumerable<string> args) => Run((args ?? Array.Empty<string>()).GetEnumerator());

		internal void Run(IEnumerator<string> args) {
			ArgumentNullException.ThrowIfNull(args);

#if !DEBUG
			try {
#endif
				bool first = true;
				while (true) {
					if (MaximumLogLevel >= LogLevel.Error) {
						return;
					}
					
					if (first) {
						if (!args.MoveNext()) {
							break;
						}
						first = false;
					}
					if (!RunCommand(args)) {
						break;
					}
				}
#if !DEBUG
#pragma warning disable CA1031 // Do not catch general exception types
			} catch (Exception ex) {
#pragma warning restore CA1031 // Do not catch general exception types
				Log(ex.Message, LogLevel.Exception);
			}
#endif
		}

		private bool RunCommand(IEnumerator<string> args) {
			string cmdName = args.Current;

			Command? cmd;
			cmd = RegisteredCommands?
				.FirstOrDefault(c => c.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase));
			cmd ??= InternalCommands
				.FirstOrDefault(c => c.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase));

			if (cmd is null) {
				Log($"Unrecognized command {cmdName}.", LogLevel.Error);
				return false;
			}

			cmd.PrepareToRun(this, args);

			Log($"Running command: {cmd}...", LogLevel.VerboseInformation);

			if (!cmd.Run(Workbench, this)) {
				return false;
			}
			return !cmd.ReachedEnd;
		}

		public void Log(string logString, LogLevel logLevel) {
			if (logLevel < 0 && !Verbose) {
				return;
			}

			if (logLevel > MaximumLogLevel) {
				MaximumLogLevel = logLevel;
			}

			ConsoleColor color = Console.ForegroundColor;

			TextWriter console = ConsoleOut;
			if (logLevel >= LogLevel.Exception) {
				console = ConsoleError;
				if (console == Console.Error) {
					Console.ForegroundColor = ConsoleColor.Red;
				}
				console.Write("FATAL: ");
			} else if (logLevel == LogLevel.Error) {
				console = ConsoleError;
				if (console == Console.Error) {
					Console.ForegroundColor = ConsoleColor.Red;
				}
				console.Write("ERROR: ");
			} else if (logLevel == LogLevel.Warning) {
				console = ConsoleError;
				if (console == Console.Error) {
					Console.ForegroundColor = ConsoleColor.Yellow;
				}
				console.Write("WARNING: ");
			}
			Console.ForegroundColor = color;

			console.WriteLine(logString);
		}

		public void SetVerbosity(bool verbose)
			=> Verbose = verbose;
		public void ResetVerbosity()
			=> Verbose = false;
	}
}
