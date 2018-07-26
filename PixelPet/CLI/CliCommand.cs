using LibPixelPet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelPet.CLI {
	internal abstract class CliCommand {
		public string Name { get; }
		protected IList<Parameter> Parameters { get; }
		public bool ReachedEnd { get; private set; }

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
		}

		public abstract void Run(Workbench workbench, ILogger logger);

		protected void ClearParameter() {
			foreach (Parameter par in this.Parameters) {
				par.IsPresent = false;

				foreach (ParameterValue val in par.Values) {
					val.Clear();
				}
			}
		}

		public void Prepare(IEnumerator<string> args) {
			this.ClearParameter();

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
				value.Value = args.Current;
			}

			return true;
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
