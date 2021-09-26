using LibPixelPet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelPet.CLI.Commands {
	internal class DeduplicatePalettesCmd : CliCommand {
		public DeduplicatePalettesCmd()
			: base("Deduplicate-Palettes") { }

		protected override void Run(Workbench workbench, ILogger logger) {
			int seed = 0;

			Random rng = new Random(seed);

			int dupes = 0;
			foreach (PaletteEntry pe in workbench.PaletteSet) {
				dupes += DeduplicatePalette(pe.Palette, rng, logger);
			}

			logger?.Log("Adding " + dupes + " new colors to palette.");
		}

		private static int DeduplicatePalette(Palette palette, Random rng, ILogger logger) {
			// Create sequential color format to remove alpha channel and padding.
			ColorFormat seqFmt = ColorFormat.SequentialARGB(
				palette.Format.RedBits,
				palette.Format.GreenBits,
				palette.Format.BlueBits,
				0
			);
			int n = seqFmt.MaxValue;

			if (palette.Count > n) {
				logger?.Log("Cannot deduplicate palette; palette size exceeds color space.", LogLevel.Error);
				return 0;
			}

			// Set of distinct colors with lookup O(1).
			// Use Dictionary instead of HashSet to pre-allocate capacity.
			Dictionary<int, int> colToIdxDict = new Dictionary<int, int>(palette.Count);

			// Put colors in the dictionary and count dupes.
			List<int> dupes = new List<int>();
			for (int i = 0; i < palette.Count; i++) {
				int c = seqFmt.Convert(palette[i], palette.Format);

				if (!colToIdxDict.ContainsKey(c)) {
					colToIdxDict[c] = i;
				} else {
					dupes.Add(i);
				}
			}

			// Sort colors ascending.
			List<int> colSet = colToIdxDict.Keys.ToList();
			colSet.Sort();

			// For each dupe, add a new color.
			foreach (int dupe in dupes) {
				int? candidate = RandomIntByBinarySearch(colSet, rng, n);
				if (candidate is int c) {
					colSet.Add(c);
					colSet.Sort();
					palette[dupe] = palette.Format.Convert(c, seqFmt);
				} else {
					logger?.Log("Failed to deduplicate palette .", LogLevel.Error);
					return 0;
				}
			}

			return dupes.Count;
		}

		/*private static int? RandomIntByBruteForce(in ICollection<int> excluded, Random rng, in int n, int trials) {
			// Just try creating random numbers. Note: goes on forever if trials < 0.
			while (trials-- != 0) {
				int c = rng.Next(n);
				if (!excluded.Contains(c)) {
					return c;
				}
			}
			return null;
		}*/

		private static int? RandomIntByBinarySearch(in List<int> excluded, Random rng, in int n) {
			// (Probably) already have every possible int in the collection.
			if (excluded.Count >= n) {
				return null;
			}

			// Generate index of int in set of unused ints.
			int t = rng.Next(n - excluded.Count);

			// Binary search for the gap in excluded in which t occurs.
			int l = 0;
			int r = excluded.Count - 1;
			int m;
			int x;
			while (l <= r) {
				m = (l + r) / 2;
				x = excluded[m] - m;
				if (x <= t) {
					l = m + 1;
				} else if (x > t) {
					r = m - 1;
				}
			}
			return t + Math.Min(l, r) + 1;
		}
	}
}
