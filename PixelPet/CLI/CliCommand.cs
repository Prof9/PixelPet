﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelPet.CLI {
	public abstract partial class CliCommand {

		[GeneratedRegex("<([^<>]+)>")]
		private static partial Regex VariableRegex();

		/// <summary>
		/// Gets or sets the CLI currently running this command.
		/// </summary>
		protected CommandRunner CLI { get; private set; }
		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		public string Name { get; }
		internal IList<Parameter> Parameters { get; }
		/// <summary>
		/// Gets whether the command has consumed all its arguments.
		/// </summary>
		public bool ReachedEnd { get; private set; }
		/// <summary>
		/// Gets whether the command is ready to run.
		/// </summary>
		public bool IsReadyToRun { get; private set; }

		protected CliCommand(in string name, params Parameter[] parameters) {
			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (name.Any(c => char.IsWhiteSpace(c)))
				throw new ArgumentException("Name cannot contain white-space characters.", nameof(name));
			if (name[0] == '-')
				throw new ArgumentException("Name cannot start with a dash.");
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			Name = name;
			Parameters = parameters.ToList();
			IsReadyToRun = false;
		}

		/// <summary>
		/// Run the command. The command must first be prepared with <see cref="PrepareToRun"/>.
		/// </summary>
		/// <param name="workbench"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public bool Run(Workbench workbench, ILogger logger) {
			if (!IsReadyToRun)
				throw new InvalidOperationException("Command is not ready to run");

			return RunImplementation(workbench, logger);
		}
		protected abstract bool RunImplementation(Workbench workbench, ILogger logger);

		protected void ClearParameter() {
			foreach (Parameter par in Parameters) {
				par.IsPresent = false;

				foreach (ParameterValue val in par.Values) {
					val.Clear();
				}
			}
			IsReadyToRun = false;
		}

		/// <summary>
		/// Prepare the command to run on the given CLI with the given arguments.
		/// </summary>
		/// <param name="cli">CLI that will run the command.</param>
		/// <param name="args">String enumerator from which arguments can be consumed.</param>
		public void PrepareToRun(CommandRunner cli, IEnumerator<string> args) {
			if (cli is null)
				throw new ArgumentNullException(nameof(cli));
			if (args is null)
				throw new ArgumentNullException(nameof(args));

			ClearParameter();

			CLI = cli;

			bool reachedEnd = true;
			while (args.MoveNext()) {
				// Read parameters until none left.
				if (!ReadParameter(args)) {
					reachedEnd = false;
					break;
				}
			}
			ReachedEnd = reachedEnd;

			IEnumerable<Parameter> missingParams = Parameters
				.Where(p => p.IsRequired && !p.IsLoaded);
			if (missingParams.Any()) {
				string missingParamsStr = string.Join(", ", missingParams.Select(a => a.ToString()));
				throw new ArgumentException($"Missing required parameters: {missingParamsStr}.", nameof(args));
			}

			IsReadyToRun = true;
		}

		private bool ReadParameter(IEnumerator<string> args) {
			// Find parameter to read.
			Parameter par = FindUnpreparedParameter(args.Current);
			if (par is null) {
				// Did not find a suitable parameter.
				return false;
			}

			if (par.IsLoaded) {
				throw new ArgumentException($"Parameter {par.PrimaryName} already defined.");
			}

			par.IsPresent = true;

			// Read parameter values.
			bool first = true;
			foreach (ParameterValue value in par.Values) {
				if (!(first && !par.IsNamed)) {
					if (!args.MoveNext()) {
						throw new ArgumentException($"Unexpected end of input for parameter {par.PrimaryName}.");
					}
				}
				first = false;
				value.Value = ExpandValue(args.Current);
			}

			return true;
		}

		/// <summary>
		/// Expands any variables in the given parameter value.
		/// </summary>
		/// <param name="value">The parameter value to expand.</param>
		/// <returns>The expanded value.</returns>
		private string ExpandValue(string value) {
			string before = value;

			MatchCollection matches;
			do {
				matches = VariableRegex().Matches(value);
				// Go right to left so we can do multiple in one pass
				foreach (Match match in matches.Reverse()) {
					string varName = match.Groups[1].Value;
					if (!(CLI?.Variables?.TryGetValue(varName, out string varValue) ?? false)) {
						throw new ArgumentException($"Unknown variable {varName} in {before}");
					}

					// Insert the variable into the string
					value = value[..match.Index] + varValue + value[(match.Index + match.Length)..];
				}
			}
			while (matches.Count > 0);

			int varStartIdx = value.IndexOf('<', StringComparison.Ordinal);
			if (varStartIdx >= 0) {
				throw new ArgumentException($"Unterminated variable {value[(varStartIdx + 1)..]} in {value}");
			}
			if (value.Contains('>', StringComparison.Ordinal)) {
				throw new ArgumentException($"Unexpected > in {value}");
			}

			return value;
		}

		private Parameter FindUnpreparedParameter(in string str)
			=> FindNamedParameter(str) ?? FindUnnamedParameter(0, true);

		protected Parameter FindNamedParameter(string str) {
			if (str is null) {
				return null;
			} else if (str.StartsWith("--", StringComparison.Ordinal)) {
				return Parameters.FirstOrDefault(
					p => string.CompareOrdinal(str, 2, p.LongName, 0, int.MaxValue) == 0
				) ?? throw new ArgumentException($"Unrecognized parameter {str}.");
			} else if (str.StartsWith("-", StringComparison.Ordinal)) {
				return Parameters.FirstOrDefault(
					p => string.CompareOrdinal(str, 1, p.ShortName, 0, int.MaxValue) == 0
				) ?? throw new ArgumentException($"Unrecognized parameter {str}.");
			} else {
				return null;
			}
		}

		protected Parameter FindUnnamedParameter(in int skip)
			=> FindUnnamedParameter(skip, false);
		private Parameter FindUnnamedParameter(in int skip, bool unloaded) {
			return Parameters
				.Where(p => !p.IsNamed && (!unloaded || !p.IsLoaded))
				.Skip(skip)
				.FirstOrDefault();
		}
	}
}
