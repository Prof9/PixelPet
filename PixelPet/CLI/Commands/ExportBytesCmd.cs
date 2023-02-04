using System.IO;

namespace PixelPet.CLI.Commands {
	internal sealed class ExportBytesCmd : Command {
		public ExportBytesCmd()
			: base("Export-Bytes",
				new Parameter(true, new ParameterValue("path"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string path = GetUnnamedParameter(0).Values[0].ToString();

			try {
				Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)!)!);
				using FileStream fs = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
				workbench.Stream.WriteTo(fs);
			} catch (IOException) {
				logger?.Log($"Could not save binary file {Path.GetFileName(path)}", LogLevel.Error);
				return false;
			}

			logger?.Log($"Exported {workbench.Stream.Length} bytes to {Path.GetFileName(path)}.", LogLevel.Information);
			return true;
		}
	}
}
