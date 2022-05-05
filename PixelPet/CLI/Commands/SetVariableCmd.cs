using System.Text.RegularExpressions;

namespace PixelPet.CLI.Commands {
	internal class SetVariableCmd : CliCommand {
		public SetVariableCmd()
			: base("Set-Variable",
				new Parameter(true, new ParameterValue("name")),
				new Parameter(true, new ParameterValue("value"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string name  = FindUnnamedParameter(0).Values[0].ToString();
			string value = FindUnnamedParameter(1).Values[0].ToString();

			if (!Regex.IsMatch(name, @"^[\w]+$")) {
				logger.Log($"Variable name may only contains alphanumerical characters and underscores; got: {name}", LogLevel.Error);
				return false;
			}
			if (!Regex.IsMatch(value, @"^[^<>]+$")) {
				logger.Log($"Variable value may not contain <>; got: {value}", LogLevel.Error);
				return false;
			}

			this.CLI.Variables[name] = value;

			logger?.Log($"{name} := {value}", LogLevel.VerboseInformation);
			return true;
		}
	}
}
