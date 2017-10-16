using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PixelPet {
	internal abstract class CliCommand {
		protected class ParameterValue {
			public string Name { get; }
			public string DefaultValue { get; }
			public string CurrentValue { get; set; }

			public bool HasDefaultValue => this.DefaultValue != null;
			public bool HasCurrentValue => this.CurrentValue != null;
			public bool HasValue => this.HasCurrentValue || this.HasDefaultValue;
			public string GetValue() {
				if (this.HasCurrentValue) {
					return this.CurrentValue;
				}
				if (this.HasDefaultValue) {
					return this.DefaultValue;
				}
				return null;
			}

			public ParameterValue(string name)
				: this(name, null) { }
			public ParameterValue(string name, string defaultValue) {
				// Defaults
				if (string.IsNullOrWhiteSpace(defaultValue))
					defaultValue = null;

				// Exceptions
				if (name == null)
					throw new ArgumentNullException(nameof(name));
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentException("Name cannot consist only of white-space characters.", nameof(name));

				// Assigns
				this.Name = name;
				this.DefaultValue = defaultValue;
				this.CurrentValue = null;
			}
		}

		protected class Parameter {
			public string LongName { get; }
			public string ShortName { get; }
			public ReadOnlyCollection<ParameterValue> Values { get; }
			public bool Required { get; }

			public bool IsNamed => this.LongName != null || this.ShortName != null;
			public string PrimaryName => this.LongName ?? this.ShortName;
			public bool HasValues => this.Values.All(v => v.HasValue);
			public bool IsPresent { get; set; }
			public bool IsLoaded => this.IsPresent && this.HasValues;

			public new string ToString() {
				StringBuilder builder = new StringBuilder();

				if (this.LongName != null) {
					builder.Append("--" + this.LongName + " ");
				} else if (this.ShortName != null) {
					builder.Append("-" + this.ShortName + " ");
				}

				if (this.Values.Count > 0) {
					builder.Append(this.Required ? '<' : '[');

					foreach (ParameterValue val in this.Values) {
						builder.Append(val.Name + ' ');
					}
					builder.Length--;

					builder.Append(this.Required ? '>' : ']');
				}

				return builder.ToString();
			}

			public Parameter(bool required, params ParameterValue[] values)
				: this(null, null, required, values) { }
			public Parameter(string longName, string shortName, bool required, params ParameterValue[] values) {
				// Defaults
				if (string.IsNullOrWhiteSpace(longName))
					longName = null;
				if (string.IsNullOrWhiteSpace(shortName))
					shortName = null;
				if (values == null)
					values = new ParameterValue[0];

				// Exceptions
				if (longName == null && shortName == null && values.Length == 0)
					throw new ArgumentException("Parameter cannot be unnamed and also have no values.");
				if (longName == null && shortName == null && !required)
					throw new ArgumentException("Parameter cannot be unnamed and not required.");
				if (longName != null && longName.Any(c => char.IsWhiteSpace(c)))
					throw new ArgumentException("Long name cannot contain white-space characters.", nameof(longName));
				if (longName != null && longName[0] == '-')
					throw new ArgumentException("Long name cannot start with a dash.");
				if (shortName != null && shortName.Any(c => char.IsWhiteSpace(c)))
					throw new ArgumentException("Short name cannot contain white-space characters.", nameof(shortName));
				if (shortName != null && shortName[0] == '-')
					throw new ArgumentException("Short name cannot start with a dash.");

				// Assigns
				this.LongName = longName;
				this.ShortName = shortName;
				this.Required = required;
				this.Values = Array.AsReadOnly(values);
			}
		}

		public string Name { get; }
		protected List<Parameter> Parameters { get; }
		public bool ReachedEnd { get; private set; }

		protected CliCommand(string name, params Parameter[] parameters) {
			// Defaults
			if (parameters == null)
				parameters = new Parameter[0];
			
			// Exceptions
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Any(c => char.IsWhiteSpace(c)))
				throw new ArgumentException("Name cannot contain white-space characters.", nameof(name));
			if (name[0] == '-')
				throw new ArgumentException("Name cannot start with a dash.");

			// Assigns
			this.Name = name;
			this.Parameters = parameters.ToList();
		}

		public abstract void Run(Workbench workbench, Cli cli);

		protected void ClearParameters() {
			foreach (Parameter par in this.Parameters) {
				par.IsPresent = false;

				foreach (ParameterValue val in par.Values) {
					val.CurrentValue = null;
				}
			}
		}

		public void Prepare(IEnumerator<string> args) {
			this.ClearParameters();

			bool reachedEnd = true;
			while (args.MoveNext()) {
				// Read parameters until none left.
				if (!ReadParameter(args)) {
					reachedEnd = false;
					break;
				}
			}
			this.ReachedEnd = reachedEnd;

			IEnumerable<Parameter> missingParameters = this.Parameters.Where(
				p => p.Required && !p.IsLoaded
			);
			if (missingParameters.Any()) {
				throw new ArgumentException("Missing parameters: " + string.Join(", ", missingParameters.Select(a => a.ToString())) + ".", nameof(args));
			}
		}

		private bool ReadParameter(IEnumerator<string> args) {
			// Find parameter to read.
			Parameter par = FindParameter(args.Current);
			if (par == null) {
				// Did not find a suitable parameter.
				return false;
			}

			par.IsPresent = true;

			// Read parameter values.
			bool first = true;
			foreach (ParameterValue val in par.Values) {
				if (!(first && !par.IsNamed)) {
					if (!args.MoveNext()) {
						throw new ArgumentException("Unexpected end of input.");
					}
				}
				first = false;
				val.CurrentValue = args.Current;
			}

			return true;
		}

		private Parameter FindParameter(string str) {
			Parameter par = null;
			if (str.StartsWith("--")) {
				// Find based on long name.
				par = this.Parameters.FirstOrDefault(
					p => str.Equals(p.LongName, StringComparison.InvariantCultureIgnoreCase)
				);
				if (par == null) {
					throw new ArgumentException("Unrecognized parameter \"" + str + "\".");
				}
			} else if (str.StartsWith("-")) {
				// Find based on short name.
				par = this.Parameters.FirstOrDefault(
					p => str.Equals(p.ShortName, StringComparison.InvariantCultureIgnoreCase)
				);
				if (par == null) {
					throw new ArgumentException("Unrecognized parameter \"" + str + "\".");
				}
			} else {
				// Find first unloaded unnamed parameter.
				par = this.Parameters.FirstOrDefault(
					p => !p.IsNamed && !p.IsLoaded
				);
			}
			return par;
		}
	}
}
