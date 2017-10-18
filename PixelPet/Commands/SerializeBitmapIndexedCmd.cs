using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelPet.Commands {
	internal class SerializeBitmapIndexedCmd : CliCommand {
		public SerializeBitmapIndexedCmd()
			: base("Serialize-Bitmap-Indexed") { }

		public override void Run(Workbench workbench, Cli cli) {
			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadWrite,
				workbench.Bitmap.PixelFormat
			);
			byte[] buffer = new byte[bmpData.Stride * bmpData.Height];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);

			workbench.Stream.SetLength(0);

			int bpp = 4;		// bits per pixel
			int sw = 8;			// tile size width
			int sh = 8;			// tile size height

			int ppb = 8 / bpp;	// pixels per byte
			int pi = 0;			// pixel index in current byte
			int b = 0;			// current byte

			bool foundUnmatchedColor = false;
			bool foundOutOfBounds = false;

			int tw = (bmpData.Width + (sw - 1)) / sw;
			int th = (bmpData.Height + (sh - 1)) / sh;
			int argb, c;
			for (int tj = 0; tj < th; tj++) {
				for (int ti = 0; ti < tw; ti++) {
					for (int ty = 0; ty < sh; ty++) {
						for (int tx = 0; tx < sw; tx++) {
							int py = tj * sh + ty;
							int px = ti * sw + tx;
							int ptr = py * bmpData.Stride + px * 4;

							if (px < 0 || py < 0 || px >= bmpData.Width || py >= bmpData.Height) {
								c = 0;
								if (!foundOutOfBounds) {
									cli.Log("WARNING: Tried to read out of bounds pixel in bitmap at (" + px + ", " + py + ").");
									foundOutOfBounds = true;
								}
							} else {
								argb = BitConverter.ToInt32(buffer, ptr);
								c = FindPaletteIndex(workbench.Palette, argb);

								if (c == -1) {
									c = 0;
									if (!foundUnmatchedColor) {
										cli.Log("WARNING: Found unmatched color in bitmap at (" + px + ", " + py + ").");
										foundUnmatchedColor = true;
									}
								}
							}

							// Add pixel to current byte.
							b |= (c << (pi * bpp));
							if (++pi == ppb) {
								// Write byte to stream if it's finished.
								workbench.Stream.WriteByte((byte)b);
								b = 0;
								pi = 0;
							}
						}
					}
				}
			}
		}

		private int FindPaletteIndex(IList<Color> palette, int argb) {
			// Set to totally transparent color, if it exists.
			int a = argb >> 24;
			if (a == 0 && palette[0].A == 0) {
				return 0;
			}

			// Find matching color.
			for (int i = 0; i < palette.Count; i++) {
				if (palette[i].ToArgb() == argb) {
					return i;
				}
			}

			// No matching color found.
			return -1;
		}
	}
}
