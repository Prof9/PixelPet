namespace PixelPet.CLI.Commands {
	internal sealed class ClearTilesetCmd : CLICommand {
		public ClearTilesetCmd()
			: base("Clear-Tileset",
				new Parameter("tile-size", "s", false, new ParameterValue("width", "8"), new ParameterValue("height", "8"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			Parameter ts = FindNamedParameter("--tile-size");
			int tw = ts.Values[0].ToInt32();
			int th = ts.Values[1].ToInt32();

			if (ts.IsPresent && tw <= 0) {
				logger?.Log($"Invalid tile width {tw}.", LogLevel.Error);
				return false;
			}
			if (ts.IsPresent && th <= 0) {
				logger?.Log($"Invalid tile height {th}.", LogLevel.Error);
				return false;
			}

			workbench.Tileset.Clear();
			workbench.Tileset.TileWidth = tw;
			workbench.Tileset.TileHeight = th;

			logger?.Log($"Created new {tw}x{th} tileset.");
			return true;
		}
	}
}
