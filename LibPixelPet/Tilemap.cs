using System;
using System.Collections;
using System.Collections.Generic;

namespace LibPixelPet {
	public class Tilemap : IEnumerable<TileEntry>, ICloneable {
		private List<TileEntry> TileEntries { get; }

		/// <summary>
		/// Gets or sets the format of the tilemap.
		/// </summary>
		public TilemapFormat TilemapFormat { get; set; }

		/// <summary>
		/// Gets the amount of tile entries currently in the tilemap.
		/// </summary>
		public int Count => TileEntries.Count;

		public TileEntry this[int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return TileEntries[index];
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				TileEntries[index] = value;
			}
		}

		/// <summary>
		/// Creates a new empty tilemap with the given format.
		/// </summary>
		/// <param name="mapFmt">The tilemap format to use.</param>
		public Tilemap(TilemapFormat mapFmt) {
			TilemapFormat = mapFmt;
			TileEntries = new List<TileEntry>();
		}

		/// <summary>
		/// Adds the specified tile entry to the tilemap.
		/// </summary>
		/// <param name="entry">The tile entry to add.</param>
		public void Add(TileEntry entry) {
			TileEntries.Add(entry);
		}

		/// <summary>
		/// Clears the tilemap.
		/// </summary>
		public void Clear() 
			=> TileEntries.Clear();

		/// <summary>
		/// Converts and adds the specified bitmap to the current tilemap, adding new tiles to the specified tileset.
		/// </summary>
		/// <param name="bmp">The bitmap to convert.</param>
		/// <param name="tileset">The tileset to use.</param>
		/// <param name="format">The tilemap format to use.</param>
		/// <param name="reduce">If true, reduces the tilemap; otherwise, does not reduce.</param>
		public void AddBitmap(Bitmap bmp, Tileset tileset, TilemapFormat format, bool reduce) {
			if (bmp is null)
				throw new ArgumentNullException(nameof(bmp));
			if (tileset is null)
				throw new ArgumentNullException(nameof(tileset));

			TileCutter cutter = new(tileset.TileWidth, tileset.TileHeight);
			foreach (Tile tile in cutter.CutTiles(bmp)) {
				// Find existing tileset entry if reducing.
				if (!reduce || !tileset.TryFindTileEntry(tile, format.CanFlipHorizontal, format.CanFlipVertical, out TileEntry te)) {
					// Otherwise, create a new tileset entry.
					te = tileset.AddTile(tile);
				}
				// Create new tilemap entry with original tile number.
				te = new TileEntry(te.TileNumber, te.HFlip, te.VFlip, tile.PaletteNumber);
				TileEntries.Add(te);
			}

			// Update the color format of the tileset
			tileset.ColorFormat = format.ColorFormat;
			tileset.IsIndexed = false;
		}

		public void AddBitmapIndexed(Bitmap bmp, Tileset tileset, PaletteSet palettes, TilemapFormat format, bool reduce) {
			if (bmp is null)
				throw new ArgumentNullException(nameof(bmp));
			if (tileset is null)
				throw new ArgumentNullException(nameof(tileset));
			if (palettes is null)
				throw new ArgumentNullException(nameof(palettes));

			TileCutter cutter = new(tileset.TileWidth, tileset.TileHeight);
			Tile[] indexedTiles = new Tile[palettes.Count];
			foreach (Tile tile in cutter.CutTiles(bmp)) {
				// Generate indexed tiles for each palette.
				int indexedCount = tile.GenerateIndexedTiles(palettes, indexedTiles);

				TileEntry? te = null;
				if (reduce) {
					// See if this tile can match an already indexed tile.
					for (int i = 0; i < indexedCount; i++) {
						Tile indexedTile = indexedTiles[i];
						if (tileset.TryFindTileEntry(indexedTile, format.CanFlipHorizontal, format.CanFlipVertical, out TileEntry candidate)) {
							// Create new tilemap entry with original tile number and flips, but new palette.
							te = new TileEntry(candidate.TileNumber, candidate.HFlip, candidate.VFlip, indexedTile.PaletteNumber);
							break;
						}
					}
				}
				if (te is null && indexedCount > 0) {
					Tile indexedTile = indexedTiles[0].Clone();
					TileEntry newEntry = tileset.AddTile(indexedTile);
					te = new TileEntry(newEntry.TileNumber, newEntry.HFlip, newEntry.VFlip, indexedTile.PaletteNumber);
				}
				// Could not index tile.
				te ??= default;

				Add((TileEntry)te);
			}

			// Update the color format of the tileset to the indexed format.
			tileset.ColorFormat = format.ColorFormat;
			tileset.IsIndexed = true;
		}

		public IEnumerator<TileEntry> GetEnumerator()
			=> TileEntries.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> TileEntries.GetEnumerator();

		/// <summary>
		/// Converts this tilemap to a bitmap using the specified tileset.
		/// </summary>
		/// <param name="tileset">The tileset to use.</param>
		/// <param name="tilesPerRow">The width of the tilemap in number of tiles.</param>
		/// <param name="tilesPerColumn">The height of the tilemap in number of tiles.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmap(Tileset tileset, in int tilesPerRow, in int tilesPerColumn)
			=> ToBitmapInternal(tileset, null, tilesPerRow, tilesPerColumn, false);

		/// <summary>
		/// Converts this indexed tilemap to a bitmap using the specified tileset and palette set.
		/// </summary>
		/// <param name="tileset">The tileset to use.</param>
		/// <param name="palettes">The palette set to use.</param>
		/// <param name="tilesPerRow">The width of the tilemap in number of tiles.</param>
		/// <param name="tilesPerColumn">The height of the tilemap in number of tiles.</param>
		/// <returns>The created bitmap.</returns>
		public Bitmap ToBitmapIndexed(Tileset tileset, PaletteSet palettes, in int tilesPerRow, in int tilesPerColumn)
			=> ToBitmapInternal(tileset, palettes, tilesPerRow, tilesPerColumn, true);

		private Bitmap ToBitmapInternal(Tileset tileset, PaletteSet palettes, in int tilesPerRow, in int tilesPerColumn, bool indexed) {
			if (tileset is null)
				throw new ArgumentNullException(nameof(tileset));
			if (tilesPerRow <= 0)
				throw new ArgumentOutOfRangeException(nameof(tilesPerRow));
			if (tilesPerColumn <= 0)
				throw new ArgumentOutOfRangeException(nameof(tilesPerColumn));
			if (indexed && palettes is null)
				throw new ArgumentNullException(nameof(palettes));

			ColorFormat bgra8888 = ColorFormat.ARGB8888;

			Bitmap bmp = new(tileset.TileWidth * tilesPerRow, tileset.TileHeight * tilesPerColumn);

			// Draw all the tile entries in the tilemap.
			for (int t = 0; t < Count; t++) {
				TileEntry te = this[t];
				int ti = t % tilesPerRow;
				int tj = t / tilesPerRow;

				// Stop if height exceeded.
				if (tj >= tilesPerColumn) {
					break;
				}

				// Skip if invalid tile number.
				if (te.TileNumber >= tileset.Count) {
					continue;
				}

				// Draw the tile for this tile entry.
				Tile tile = tileset[te.TileNumber];
				int palNum = TilemapFormat.BitmapEncoding switch {
					BitmapEncoding.NintendoDSTexture => 0,
					_ => te.PaletteNumber
				};
				Palette pal = palettes?.FindPalette(palNum);
				int pi = 0;
				foreach (int p in tile.EnumerateTile(te.HFlip, te.VFlip)) {
					int px = pi % tileset.TileWidth + ti * tileset.TileWidth;
					int py = pi / tileset.TileWidth + tj * tileset.TileHeight;

					GetColor(te, pal, p, out int c, out ColorFormat fmt);

					bmp[px, py] = bgra8888.Convert(c, fmt);
					pi++;
				}
			}

			return bmp;

			void GetColor(TileEntry te, Palette pal, int p, out int c, out ColorFormat fmt) {
				if (TilemapFormat.BitmapEncoding == BitmapEncoding.NintendoDSTexture) {
					switch (te.TextureMode) {
					case 0:
						if (p == 3) {
							c = 0;
							fmt = bgra8888;
						} else {
							GetColorFromPalette(pal, te.PaletteNumber * 2 + p, out c, out fmt);
						}
						break;
					case 1:
						if (p == 3) {
							c = 0;
							fmt = bgra8888;
						} else if (p == 2) {
							GetColorFromPalette(pal, te.PaletteNumber * 2 + 0, out int c1, out ColorFormat fmt1);
							GetColorFromPalette(pal, te.PaletteNumber * 2 + 1, out int c2, out ColorFormat _);
							c = BlendColors(c1, c2, 1, 1, fmt1);
							fmt = bgra8888;
						} else {
							GetColorFromPalette(pal, te.PaletteNumber * 2 + p, out c, out fmt);
						}
						break;
					case 2:
						GetColorFromPalette(pal, te.PaletteNumber * 2 + p, out c, out fmt);
						break;
					case 3:
						if (p == 3) {
							GetColorFromPalette(pal, te.PaletteNumber * 2 + 0, out int c1, out ColorFormat fmt1);
							GetColorFromPalette(pal, te.PaletteNumber * 2 + 1, out int c2, out ColorFormat _);
							c = BlendColors(c1, c2, 3, 5, fmt1);
							fmt = bgra8888;
						} else if (p == 2) {
							GetColorFromPalette(pal, te.PaletteNumber * 2 + 0, out int c1, out ColorFormat fmt1);
							GetColorFromPalette(pal, te.PaletteNumber * 2 + 1, out int c2, out ColorFormat _);
							c = BlendColors(c1, c2, 5, 3, fmt1);
							fmt = bgra8888;
						} else {
							GetColorFromPalette(pal, te.PaletteNumber * 2 + p, out c, out fmt);
						}
						break;
					default:
						// Should never happen
						c = 0;
						fmt = bgra8888;
						break;
					}
				} else if (indexed) {
					GetColorFromPalette(pal, p, out c, out fmt);
				} else {
					c = p;
					fmt = tileset.ColorFormat;
				}
			}
			int BlendColors(int c1, int c2, int ratio1, int ratio2, ColorFormat fmt) {
				int r1, r2, r3, g1, g2, g3, b1, b2, b3, a1, a2, a3;
				int den = ratio1 + ratio2;

				// Normalize to 32-bit color
				c1 = bgra8888.Convert(c1, fmt);
				c2 = bgra8888.Convert(c2, fmt);

				// Get components
				r1 = (c1 >> bgra8888.  RedShift) & bgra8888.  RedMax;
				r2 = (c2 >> bgra8888.  RedShift) & bgra8888.  RedMax;
				g1 = (c1 >> bgra8888.GreenShift) & bgra8888.GreenMax;
				g2 = (c2 >> bgra8888.GreenShift) & bgra8888.GreenMax;
				b1 = (c1 >> bgra8888. BlueShift) & bgra8888. BlueMax;
				b2 = (c2 >> bgra8888. BlueShift) & bgra8888. BlueMax;
				a1 = (c1 >> bgra8888.AlphaShift) & bgra8888.AlphaMax;
				a2 = (c2 >> bgra8888.AlphaShift) & bgra8888.AlphaMax;

				// Blend components
				r3 = (r1 * ratio1 + r2 * ratio2 + den / 2) / den;
				g3 = (g1 * ratio1 + g2 * ratio2 + den / 2) / den;
				b3 = (b1 * ratio1 + b2 * ratio2 + den / 2) / den;
				a3 = (a1 * ratio1 + a2 * ratio2 + den / 2) / den;

				// Form new color
				return (r3 << bgra8888.RedShift) | (g3 << bgra8888.GreenShift) | (b3 << bgra8888.BlueShift) | (a3 << bgra8888.AlphaShift);
			}
			void GetColorFromPalette(Palette pal, int p, out int c, out ColorFormat fmt) {
				if (pal is not null && p < pal.Count) {
					c = pal[p];
					fmt = pal.Format;
				} else {
					// TODO: Throw an exception?
					c = p;
					fmt = tileset.ColorFormat;
				}
			}
		}

		public Tilemap Clone() {
			Tilemap clone = new(TilemapFormat);
			clone.TileEntries.AddRange(TileEntries);
			return clone;
		}
		object ICloneable.Clone() => Clone();
	}
}
