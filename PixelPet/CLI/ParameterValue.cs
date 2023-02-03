using LibPixelPet;
using System;

namespace PixelPet.CLI {
	public class ParameterValue {
		public string Name { get; }
		public string DefaultValue { get; }
		public string CurrentValue { get; private set; }

		public bool HasDefaultValue => DefaultValue is not null;
		public bool HasCurrentValue => CurrentValue is not null;
		public bool HasValue => HasCurrentValue || HasDefaultValue;
		public string Value {
			get => CurrentValue ?? DefaultValue;
			set => CurrentValue = value;
		}

		public ParameterValue(in string name)
			: this(name, null) { }
		public ParameterValue(in string name, in string defaultValue) {
			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name cannot consist of only white-space characters.", nameof(name));

			Name = name;
			DefaultValue = defaultValue;
			CurrentValue = null;
		}

		public void Clear()
			=> CurrentValue = null;

		public int ToInt32() {
			if (Value is not null && NumberParser.TryParseInt32(Value, out int r)) {
				return r;
			} else {
				return 0;
			}
		}
		public long ToInt64() {
			if (NumberParser.TryParseInt64(Value, out long r)) {
				return r;
			} else {
				return 0;
			}
		}
		public override string ToString()
			=> Value ?? "";
	}
}
