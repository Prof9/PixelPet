using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PixelPet.CLI {
	internal class Parameter {
		public string LongName { get; }
		public string ShortName { get; }
		public ReadOnlyCollection<ParameterValue> Values { get; }
		public bool IsRequired { get; }
		public bool IsPresent { get; set; }

		public bool IsNamed
			=> this.LongName != null || this.ShortName != null;
		public string PrimaryName
			=> this.LongName ?? this.ShortName;
		public bool HasAllValues
			=> this.Values.All(v => v.HasValue);
		public bool IsLoaded
			=> this.IsPresent && this.HasAllValues;

		public override string ToString() {
			StringBuilder builder = new StringBuilder();

			if (this.LongName != null) {
				builder.Append("--" + this.LongName + ' ');
			} else if (this.ShortName != null) {
				builder.Append('-' + this.ShortName + ' ');
			}

			if (this.Values.Any()) {
				builder.Append(this.IsRequired ? '<' : '[');

				foreach (ParameterValue val in this.Values) {
					builder.Append(val.Name + ' ');
				}
				builder.Length--;

				builder.Append(this.IsRequired ? '>' : ']');
			}

			return builder.ToString();
		}

		public Parameter(bool required, params ParameterValue[] values)
			: this(null, null, required, values) { }
		public Parameter(in string longName, in string shortName, in bool required, params ParameterValue[] values) {
			if (longName == null && shortName == null && values.Length == 0)
				throw new ArgumentException("Parameter cannot be unnamed and also have no values.");
			if (longName == null && shortName == null && !required)
				throw new ArgumentException("Parameter cannot be unnamed and not required.");
			if (longName?.Any(c => char.IsWhiteSpace(c)) ?? false)
				throw new ArgumentException("Long name cannot contain white-space characters.", nameof(longName));
			if (longName?[0] == '-')
				throw new ArgumentException("Long name cannot start with a dash.");
			if (shortName?.Any(c => char.IsWhiteSpace(c)) ?? false)
				throw new ArgumentException("Short name cannot contain white-space characters.", nameof(shortName));
			if (shortName?[0] == '-')
				throw new ArgumentException("Short name cannot start with a dash.");
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			this.LongName = longName;
			this.ShortName = shortName;
			this.IsRequired = required;
			this.Values = Array.AsReadOnly(values);
		}
	}
}
