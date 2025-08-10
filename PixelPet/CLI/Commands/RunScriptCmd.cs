using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PixelPet.CLI.Commands {
	internal sealed partial class RunScriptCmd : Command {

		[GeneratedRegex("\\\"(?:[^\"\\\\]|\\\\.)*\\\"|[^\"\\s]+")]
		private static partial Regex QuotedArgumentRegex();

		private HashSet<string> ScriptPaths { get; }

		public RunScriptCmd()
			: base("Run-Script",
				  new Parameter(true, new ParameterValue("path")),
				  new Parameter("recursive", "r", false)
			) {
			ScriptPaths = new();
		}

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string path = GetUnnamedParameter(0).Values[0].ToString();
			bool recursive = GetNamedParameter("--recursive").IsPresent;

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

			// Process and remove comments
			StringBuilder scriptWithoutComments = new();
			bool inLineComment = false;
			bool inBlockComment = false;
			bool inString = false;
			for (int i = 0; i < script.Length; i++) {
				char nextChar = script[i];
				ReadOnlySpan<char> next2Chars = script.AsSpan(i, Math.Min(2, script.Length - i));
				if (nextChar == '\\') {
					// Escaped char
					if (!inLineComment && !inBlockComment) {
						scriptWithoutComments.Append(next2Chars);
					}
					i++;
				} else if (inLineComment) {
					// End of line comment
					if (nextChar == '\n') {
						inLineComment = false;
					}
				} else if (inBlockComment) {
					// End of block comment
					if (next2Chars.SequenceEqual("*/")) {
						inBlockComment = false;
						i++;
					}
				} else if (!inString && next2Chars.SequenceEqual("//")) {
					// Start of line comment
					inLineComment = true;
				} else if (!inString && next2Chars.SequenceEqual("/*")) {
					// Start of block comment
					inBlockComment = true;
				} else {
					// Normal char
					if (nextChar == '"') {
						inString = !inString;
					}
					scriptWithoutComments.Append(nextChar);
				}
			}

			List<string> args = new();
			foreach (Match match in (IEnumerable<Match>)QuotedArgumentRegex().Matches(scriptWithoutComments.ToString())) {
				if (match.Value[0] == '"' && match.Value[^1] == '"') {
					args.Add(match.Value[1..^1]);
				} else {
					args.Add(match.Value);
				}
			}

			Runner.Run(args);

			ScriptPaths.Remove(fullPath);
			return true;
		}
	}
}
