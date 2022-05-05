using LibPixelPet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelPet.CLI {
	public abstract class CliCommand {
		/// <summary>
		/// Gets or sets the CLI currently running this command.
		/// </summary>
		protected Cli CLI { get; private set; }
		public string Name { get; }
		internal IList<Parameter> Parameters { get; }
		public bool ReachedEnd { get; private set; }
		public bool IsPrepared { get; private set; }

		protected CliCommand(in string name, params Parameter[] parameters) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Any(c => char.IsWhiteSpace(c)))
				throw new ArgumentException("Name cannot contain white-space characters.", nameof(name));
			if (name[0] == '-')
				throw new ArgumentException("Name cannot start with a dash.");
			if (parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			this.Name = name;
			this.Parameters = parameters.ToList();
			this.IsPrepared = false;
		}

		public bool Run(Workbench workbench, ILogger logger) {
			if (!this.IsPrepared)
				throw new InvalidOperationException("Command is not prepared to run");

			return this.RunImplementation(workbench, logger);
		}
		protected abstract bool RunImplementation(Workbench workbench, ILogger logger);

		protected void ClearParameter() {
			foreach (Parameter par in this.Parameters) {
				par.IsPresent = false;

				foreach (ParameterValue val in par.Values) {
					val.Clear();
				}
			}
			this.IsPrepared = false;
		}

		public void Prepare(Cli cli, IEnumerator<string> args) {
			if (cli is null)
				throw new ArgumentNullException(nameof(cli));
			if (args is null)
				throw new ArgumentNullException(nameof(args));

			this.ClearParameter();

			this.CLI = cli;

			bool reachedEnd = true;
			while (args.MoveNext()) {
				// Read parameters until none left.
				if (!ReadParameter(args)) {
					reachedEnd = false;
					break;
				}
			}
			this.ReachedEnd = reachedEnd;

			IEnumerable<Parameter> missingParameters = this.Parameters
				.Where(p => p.IsRequired && !p.IsLoaded);
			if (missingParameters.Any()) {
				throw new ArgumentException("Missing required parameters: "
					+ string.Join(", ", missingParameters.Select(a => a.ToString()))
					+ ".", nameof(args));
			}

			this.IsPrepared = true;
		}

		private bool ReadParameter(IEnumerator<string> args) {
			// Find parameter to read.
			Parameter par = FindUnpreparedParameter(args.Current);
			if (par == null) {
				// Did not find a suitable parameter.
				return false;
			}

			if (par.IsLoaded) {
				throw new ArgumentException("Parameter " + par.PrimaryName + " already defined.");
			}

			par.IsPresent = true;

			// Read parameter values.
			bool first = true;
			foreach (ParameterValue value in par.Values) {
				if (!(first && !par.IsNamed)) {
					if (!args.MoveNext()) {
						throw new ArgumentException("Unexpected end of input for parameter \"" + par.PrimaryName + "\".");
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
				matches = Regex.Matches(value, @"<([^<>]+)>");
				// Go right to left so we can do multiple in one pass
				foreach (Match match in matches.Reverse()) {
					string varName = match.Groups[1].Value;
					if (!(this.CLI?.Variables?.TryGetValue(varName, out string varValue) ?? false)) {
						throw new ArgumentException($"Unknown variable {varName} in {before}");
					}

					// Insert the variable into the string
					value = value[..match.Index] + varValue + value[(match.Index + match.Length)..];
				}
			}
			while (matches.Count > 0);

			int varStartIdx = value.IndexOf('<');
			int varEndIdx   = value.IndexOf('>');
			if (varStartIdx >= 0) {
				throw new ArgumentException($"Unterminated variable {value[(varStartIdx + 1)..]} in {value}");
			}
			if (varEndIdx >= 0) {
				throw new ArgumentException($"Unexpected > in {value}");
			}

			return value;
		}

		private Parameter FindUnpreparedParameter(in string str)
			=> this.FindNamedParameter(str) ?? this.FindUnnamedParameter(0, true);

		protected Parameter FindNamedParameter(string str) {
			if (str == null) {
				return null;
			} else if (str.StartsWith("--", StringComparison.Ordinal)) {
				return this.Parameters.FirstOrDefault(
					p => string.CompareOrdinal(str, 2, p.LongName, 0, int.MaxValue) == 0
				) ?? throw new ArgumentException("Unrecognized parameter \"" + str + "\".");
			} else if (str.StartsWith("-", StringComparison.Ordinal)) {
				return this.Parameters.FirstOrDefault(
					p => string.CompareOrdinal(str, 1, p.ShortName, 0, int.MaxValue) == 0
				) ?? throw new ArgumentException("Unrecognized parameter \"" + str + "\".");
			} else {
				return null;
			}
		}

		protected Parameter FindUnnamedParameter(in int skip)
			=> this.FindUnnamedParameter(skip, false);
		private Parameter FindUnnamedParameter(in int skip, bool unloaded) {
			return this.Parameters
				.Where(p => !p.IsNamed && (!unloaded || !p.IsLoaded))
				.Skip(skip)
				.FirstOrDefault();
		}
	}
}
