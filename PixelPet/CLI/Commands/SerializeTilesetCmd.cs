using LibPixelPet;

namespace PixelPet.CLI.Commands {
	internal class SerializeTilesetCmd : CliCommand {
		public SerializeTilesetCmd()
			: base("Serialize-Tileset",
				new Parameter("append", "a", false),
				new Parameter("color-offset", "o", false, new ParameterValue("value", "0"))
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			bool append = FindNamedParameter("--append").IsPresent;
			long colorOffset = FindNamedParameter("--color-offset").Values[0].ToInt64();

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			using (PixelWriter pixelWriter = new PixelWriter(workbench.Stream, workbench.Tileset.ColorFormat, true)) {
				foreach (Tile tile in workbench.Tileset) {
					pixelWriter.BeginWrite();
					foreach (int c in tile.EnumerateTile()) {
						pixelWriter.WriteNextPixel((int)(c + colorOffset));
					}
					pixelWriter.EndWrite();
				}
			}

			logger?.Log("Serialized " + workbench.Tileset.Count + " tiles.", LogLevel.Information);
		}
	}
}
