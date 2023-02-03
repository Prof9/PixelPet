using LibPixelPet;
using System.IO;

namespace PixelPet {
	/// <summary>
	/// PixelPet workbench instance.
	/// </summary>
	public class Workbench {
		public PaletteSet PaletteSet { get; set; }
		public Bitmap Bitmap { get; set; }
		public MemoryStream Stream { get; private set; }
		public Tileset Tileset { get; set; }
		public Tilemap Tilemap { get; set; }
		public ColorFormat BitmapFormat { get; set; }

		public Workbench() {
			PaletteSet = new();
			Bitmap = new(0, 0);
			Stream = new();
			Tileset = new(8, 8);
			Tilemap = new(TilemapFormat.GBA4BPP);
		}
	}
}
