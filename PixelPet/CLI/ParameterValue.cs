using LibPixelPet;
using System;

namespace PixelPet.CLI {
	public class ParameterValue {
		public string Name { get; }
		public string? DefaultValue { get; }
		public string? CurrentValue { get; private set; }

		public bool HasDefaultValue => DefaultValue is not null;
		public bool HasCurrentValue => CurrentValue is not null;
		public bool HasValue => HasCurrentValue || HasDefaultValue;
		public string? Value {
			get => CurrentValue ?? DefaultValue;
			set => CurrentValue = value;
		}

		public ParameterValue(in string name)
			: this(name, null) { }
		public ParameterValue(in string name, in string? defaultValue) {
			ArgumentNullException.ThrowIfNull(name);
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name cannot consist of only white-space characters.", nameof(name));

			Name = name;
			DefaultValue = defaultValue;
			CurrentValue = null;
		}

		public void Clear()
			=> CurrentValue = null;

		public int ToInt32() {
			if (NumberParser.TryParseInt32(ToString(), out int r)) {
				return r;
			} else {
				throw new InvalidOperationException($"Value of {Name} parameter cannot be parsed as 32-bit integer");
			}
		}
		public long ToInt64() {
			if (NumberParser.TryParseInt64(ToString(), out long r)) {
				return r;
			} else {
				throw new InvalidOperationException($"Value of {Name} parameter cannot be parsed as 64-bit integer");
			}
		}
		public override string ToString()
			=> Value ?? "";
	}
}
