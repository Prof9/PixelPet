using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal sealed class SerializeBitmapCmd : Command {
		public SerializeBitmapCmd()
			: base("Serialize-Bitmap",
				new Parameter("append", "a", false)
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			bool append = GetNamedParameter("--append").IsPresent;

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			using (PixelWriter pixelWriter = new(workbench.Stream, workbench.BitmapFormat, true)) {
				pixelWriter.WritePixels(workbench.Bitmap.Pixels, 0, workbench.Bitmap.Width * workbench.Bitmap.Height);
			}

			logger?.Log($"Serialized {workbench.Bitmap.Width}x{workbench.Bitmap.Height} bitmap.", LogLevel.Information);
			return true;
		}
	}
}
