using System.Text.RegularExpressions;

namespace PixelPet.CLI.Commands {
	internal sealed partial class SetVariableCmd : Command {
		[GeneratedRegex("^[\\w]+$")]
		private static partial Regex VariableNameRegex();
		[GeneratedRegex("^[^<>]+$")]
		private static partial Regex VariableValueRegex();

		public SetVariableCmd()
			: base("Set-Variable",
				new Parameter(true, new ParameterValue("name")),
				new Parameter(true, new ParameterValue("value"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string name  = GetUnnamedParameter(0).Values[0].ToString();
			string value = GetUnnamedParameter(1).Values[0].ToString();

			if (!VariableNameRegex().IsMatch(name)) {
				logger.Log($"Variable name may only contains alphanumerical characters and underscores; got: {name}", LogLevel.Error);
				return false;
			}
			if (!VariableValueRegex().IsMatch(value)) {
				logger.Log($"Variable value may not contain <>; got: {value}", LogLevel.Error);
				return false;
			}

			Runner.Variables[name] = value;

			logger?.Log($"{name} := {value}", LogLevel.VerboseInformation);
			return true;
		}
	}
}
