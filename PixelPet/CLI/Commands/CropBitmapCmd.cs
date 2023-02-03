namespace PixelPet.CLI.Commands {
	internal sealed class CropBitmapCmd : CliCommand {
		public CropBitmapCmd()
			: base("Crop-Bitmap",
				new Parameter("x", "x", false, new ParameterValue("pixels", "0")),
				new Parameter("y", "y", false, new ParameterValue("pixels", "0")),
				new Parameter("width", "w", false, new ParameterValue("pixels", "-1")),
				new Parameter("height", "h", false, new ParameterValue("pixels", "-1"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			int x = FindNamedParameter("--x").Values[0].ToInt32();
			int y = FindNamedParameter("--y").Values[0].ToInt32();
			int w = FindNamedParameter("--width").Values[0].ToInt32();
			int h = FindNamedParameter("--height").Values[0].ToInt32();

			workbench.Bitmap = workbench.Bitmap.GetCroppedBitmap(x, y, w, h);

			logger?.Log($"Cropped {workbench.Bitmap.Width}x{workbench.Bitmap.Height} bitmap.");
			return true;
		}
	}
}
