using LibPixelPet;
using System;

namespace PixelPet.CLI {
	public class ParameterValue {
		public string Name { get; }
		public string DefaultValue { get; }
		public string CurrentValue { get; private set; }

		public bool HasDefaultValue => this.DefaultValue != null;
		public bool HasCurrentValue => this.CurrentValue != null;
		public bool HasValue => this.HasCurrentValue || this.HasDefaultValue;
		public string Value {
			get => this.CurrentValue ?? this.DefaultValue;
			set => this.CurrentValue = value;
		}

		public ParameterValue(in string name)
			: this(name, null) { }
		public ParameterValue(in string name, in string defaultValue) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name cannot consist of only white-space characters.", nameof(name));

			this.Name = name;
			this.DefaultValue = defaultValue;
			this.CurrentValue = null;
		}

		public void Clear()
			=> this.CurrentValue = null;

		public int ToInt32() {
			if (this.Value is not null && NumberParser.TryParseInt32(this.Value, out int r)) {
				return r;
			} else {
				return 0;
			}
		}
		public long ToInt64() {
			if (NumberParser.TryParseInt64(this.Value, out long r)) {
				return r;
			} else {
				return 0;
			}
		}
		public override string ToString()
			=> this.Value ?? "";
	}
}
