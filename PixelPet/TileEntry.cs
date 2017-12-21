using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelPet {
	public struct TilesetEntry {
		public int TileNumber { get; private set; }
		public bool HFlip { get; private set; }
		public bool VFlip { get; private set; }

		public TilesetEntry(int tileNum, bool hflip, bool vflip) {
			this.TileNumber = tileNum;
			this.HFlip = hflip;
			this.VFlip = vflip;
		}

		public override int GetHashCode() {
			// Pretty crappy hash.
			uint hash = (uint)(this.TileNumber);
			if (this.HFlip) {
				hash ^= 0x55555555;
			}
			if (this.VFlip) {
				hash ^= 0xAAAAAAAA;
			}
			return (int)hash;
		}

		public override bool Equals(object obj) {
			return obj is TilesetEntry entry &&
				this.TileNumber == entry.TileNumber &&
				this.HFlip == entry.HFlip &&
				this.VFlip == entry.VFlip;
		}

		public static bool operator ==(TilesetEntry a, TilesetEntry b) {
			return a.Equals(b);
		}
		public static bool operator !=(TilesetEntry a, TilesetEntry b) {
			return !a.Equals(b);
		}
	}
}
