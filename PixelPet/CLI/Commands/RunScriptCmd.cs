using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PixelPet.CLI.Commands {
	internal sealed partial class RunScriptCmd : CliCommand {

		[GeneratedRegex("\\\".*?\\\"|[^\"\\s]+")]
		private static partial Regex QuotedArgumentRegex();

		private HashSet<string> ScriptPaths { get; }

		public RunScriptCmd()
			: base("Run-Script",
				  new Parameter(true, new ParameterValue("path")),
				  new Parameter("recursive", "r", false)
			) {
			ScriptPaths = new HashSet<string>();
		}

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();
			bool recursive = FindNamedParameter("--recursive").IsPresent;

			// Check for recursion.
			string fullPath = Path.GetFullPath(path);
			if (!recursive && ScriptPaths.Contains(fullPath)) {
				logger?.Log($"Recursive script inclusion of script {Path.GetFileName(path)} is not allowed without --recursive option.", LogLevel.Error);
				return false;
			}

			// Add the script path onto the list of included scripts. Since we stay in the same CLI, these will be shared for further Run-Scripts.
			ScriptPaths.Add(fullPath);

			string script;
			try {
				script = File.ReadAllText(path);
			} catch (IOException) {
				logger?.Log($"Could not read file {Path.GetFileName(path)}", LogLevel.Error);
				return false;
			}

			List<string> args = new();
			foreach (Match match in (IEnumerable<Match>)QuotedArgumentRegex().Matches(script)) {
				if (match.Value[0] == '"' && match.Value[^1] == '"') {
					args.Add(match.Value[1..^1]);
				} else {
					args.Add(match.Value);
				}
			}

			CLI.Run(args);

			ScriptPaths.Remove(fullPath);
			return true;
		}
	}
}
