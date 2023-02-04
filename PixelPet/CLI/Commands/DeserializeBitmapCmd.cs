using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal sealed class DeserializeBitmapCmd : CLICommand {
		public DeserializeBitmapCmd()
			: base("Deserialize-Bitmap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter(true, new ParameterValue("width")),
				new Parameter(true, new ParameterValue("height")),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].ToString();
			int w = FindUnnamedParameter(1).Values[0].ToInt32();
			int h = FindUnnamedParameter(2).Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();

			if (ColorFormat.GetFormat(fmtName) is not ColorFormat fmt) {
				logger?.Log($"Unknown color format {fmtName}.", LogLevel.Error);
				return false;
			}
			if (w <= 0) {
				logger?.Log("Invalid width.", LogLevel.Error);
				return false;
			}
			if (h <= 0) {
				logger?.Log("Invalid height.", LogLevel.Error);
				return false;
			}
			if (workbench.Stream.Length * 8 < fmt.Bits * w * h) {
				logger?.Log($"Bytes stream is not long enough to hold {w}x{h} bitmap with format {fmtName}; need 0x{(fmt.Bits * w * h + 7) / 8:X} bytes, have 0x{workbench.Stream.Length:X} bytes.", LogLevel.Error);
				return false;
			}

			workbench.Bitmap = new Bitmap(w, h);
			workbench.Stream.Position = offset;

			using (PixelReader pixelReader = new(workbench.Stream, fmt, true)) {
				pixelReader.ReadPixels(workbench.Bitmap.Pixels, 0, w * h);
			}

			workbench.BitmapFormat = fmt;

			logger?.Log($"Deserialized {workbench.Bitmap.Width}x{workbench.Bitmap.Height} bitmap.", LogLevel.Information);
			return true;
		}
	}
}
