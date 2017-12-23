using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet {
	public struct TileEntry {
		public int TileNumber { get; set; }
		public bool HFlip { get; set; }
		public bool VFlip { get; set; }
		public int PaletteNumber { get; set; }

		public TileEntry(int tileNum, bool hflip, bool vflip)
			: this(tileNum, hflip, vflip, 0) { }
		public TileEntry(int tileNum, bool hflip, bool vflip, int paletteNum) {
			this.TileNumber = tileNum;
			this.HFlip = hflip;
			this.VFlip = vflip;
			this.PaletteNumber = paletteNum;
		}

		public override int GetHashCode() {
			// Pretty crappy hash.
			uint hash = (uint)(this.TileNumber + this.PaletteNumber << 16);
			if (this.HFlip) {
				hash ^= 0x55555555;
			}
			if (this.VFlip) {
				hash ^= 0xAAAAAAAA;
			}
			return (int)hash;
		}

		public override bool Equals(object obj) {
			return obj is TileEntry entry &&
				this.TileNumber == entry.TileNumber &&
				this.HFlip == entry.HFlip &&
				this.VFlip == entry.VFlip &&
				this.PaletteNumber == entry.PaletteNumber;
		}

		public static bool operator ==(TileEntry a, TileEntry b) {
			return a.Equals(b);
		}
		public static bool operator !=(TileEntry a, TileEntry b) {
			return !a.Equals(b);
		}
	}
}
