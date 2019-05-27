using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PixelPet.CLI.Commands {
	internal class RunScriptCmd : CliCommand {
		private HashSet<string> ScriptPaths { get; }

		public RunScriptCmd()
			: base("Run-Script",
				  new Parameter(true, new ParameterValue("path")),
				  new Parameter("recursive", "r", false)
			) {
			this.ScriptPaths = new HashSet<string>();
		}

		protected override void Run(Workbench workbench, ILogger logger) {
			string path = FindUnnamedParameter(0).Values[0].ToString();
			bool recursive = FindNamedParameter("--recursive").IsPresent;

			// Check for recursion.
			string fullPath = Path.GetFullPath(path);
			if (!recursive && ScriptPaths.Contains(fullPath)) {
				logger?.Log("Recursive script inclusion of script " + Path.GetFileName(path) + " is not allowed without --recursive option.", LogLevel.Error);
				return;
			}

			// Add the script path onto the list of included scripts. Since we stay in the same CLI, these will be shared for further Run-Scripts.
			this.ScriptPaths.Add(fullPath);

			string script;
			try {
				script = File.ReadAllText(path);
			} catch (IOException) {
				logger?.Log("Could not read file " + Path.GetFileName(path), LogLevel.Error);
				return;
			}

			List<string> args = new List<string>();
			foreach (Match match in Regex.Matches(script, @"\"".*?\""|[^""\s]+")) {
				if (match.Value.StartsWith("\"", StringComparison.Ordinal) && match.Value.EndsWith("\"", StringComparison.Ordinal)) {
					args.Add(match.Value.Substring(1, match.Value.Length - 2));
				} else {
					args.Add(match.Value);
				}
			}

			CLI.Run(((IEnumerable<string>)args).GetEnumerator());

			this.ScriptPaths.Remove(fullPath);
		}
	}
}
