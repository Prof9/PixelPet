using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PixelPet.CLI {
	public class Parameter {
		public string LongName { get; }
		public string ShortName { get; }
		public ReadOnlyCollection<ParameterValue> Values { get; }
		public bool IsRequired { get; }
		public bool IsPresent { get; set; }

		public bool IsNamed
			=> LongName is not null || ShortName is not null;
		public string PrimaryName
			=> LongName ?? ShortName;
		public bool HasAllValues
			=> Values.All(v => v.HasValue);
		public bool IsLoaded
			=> IsPresent && HasAllValues;

		public override string ToString() {
			StringBuilder builder = new();

			if (LongName is not null) {
				builder.Append("--" + LongName + ' ');
			} else if (ShortName is not null) {
				builder.Append('-' + ShortName + ' ');
			}

			if (Values.Any()) {
				builder.Append(IsRequired ? '<' : '[');

				foreach (ParameterValue val in Values) {
					builder.Append(val.Name + ' ');
				}
				builder.Length--;

				builder.Append(IsRequired ? '>' : ']');
			}

			return builder.ToString();
		}

		public Parameter(bool required, params ParameterValue[] values)
			: this(null, null, required, values) { }
		public Parameter(in string longName, in string shortName, in bool required, params ParameterValue[] values) {
			if (values is null)
				throw new ArgumentNullException(nameof(values));
			if (longName is null && shortName is null && values.Length == 0)
				throw new ArgumentException("Parameter cannot be unnamed and also have no values.");
			if (longName is null && shortName is null && !required)
				throw new ArgumentException("Parameter cannot be unnamed and not required.");
			if (longName?.Any(c => char.IsWhiteSpace(c)) ?? false)
				throw new ArgumentException("Long name cannot contain white-space characters.", nameof(longName));
			if (longName?[0] == '-')
				throw new ArgumentException("Long name cannot start with a dash.");
			if (shortName?.Any(c => char.IsWhiteSpace(c)) ?? false)
				throw new ArgumentException("Short name cannot contain white-space characters.", nameof(shortName));
			if (shortName?[0] == '-')
				throw new ArgumentException("Short name cannot start with a dash.");

			LongName = longName;
			ShortName = shortName;
			IsRequired = required;
			Values = Array.AsReadOnly(values);
		}
	}
}
