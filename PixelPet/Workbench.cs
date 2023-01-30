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
			this.PaletteSet = new PaletteSet();
			this.Bitmap = new Bitmap(0, 0);
			this.Stream = new MemoryStream();
			this.Tileset = new Tileset(8, 8);
			this.Tilemap = new Tilemap(TilemapFormat.GBA4BPP);
		}
	}
}
