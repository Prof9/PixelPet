using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PixelPet.Commands {
	internal class DeduplicatePaletteCmd : CliCommand {
		public DeduplicatePaletteCmd()
			: base("Deduplicate-Palette") { }

		public override void Run(Workbench workbench, Cli cli) {
			Random rng = new Random(0);

			// Set of distinct colors with lookup O(1).
			// Use Dictionary instead of HashSet to pre-allocate capacity.
			Dictionary<int, int> colToIdxDict = new Dictionary<int, int>(workbench.Palette.Count);

			// Put colors in the dictionary and count dupes.
			List<int> dupes = new List<int>();
			for (int i = 0; i < workbench.Palette.Count; i++) {
				Color color = workbench.Palette[i];

				// Remove the alpha component.
				int c = color.ToArgb() & 0xFFFFFF;

				if (!colToIdxDict.ContainsKey(c)) {
					colToIdxDict[c] = i;
				} else {
					dupes.Add(i);
				}
			}

			// Sort colors and hues.
			List<int> colSet = colToIdxDict.Keys.ToList();
			colSet.Sort();

			// For each dupe, add a new color.
			foreach (int dupe in dupes) {
				int c = RandomIntByBinarySearch(colSet, rng, 0xFFFFFFF - 1);

				colSet.Add(c);
				colSet.Sort();
				colToIdxDict[dupe] = c;
				workbench.Palette[dupe] = Color.FromArgb((int)(0xFF000000 | c));
			}

			cli.Log("Added " + dupes.Count + " new colors to palette.");
		}

		private int RandomIntByBruteForce<T>(IDictionary<int, T> colDict, Random rng, int trials) {
			// Just try creating random numbers. Note: goes on forever if trials < 0
			while (trials-- != 0) {
				int c = rng.Next(0xFFFFFF + 1);
				if (!colDict.ContainsKey(c)) {
					return c;
				}
			}
			return -1;
		}

		private int RandomIntByBinarySearch(List<int> excluded, Random rng, int n) {
			// Already have every possible RGB888 color in the dictionary.
			if (excluded.Count == 0xFFFFFF + 1) {
				return -1;
			}

			// Generate index of color in set of unused colors.
			int t = rng.Next(n - excluded.Count);

			// Binary search for the gap in colList in which t occurs
			int l = 0;
			int r = excluded.Count - 1;
			int m;
			int x;
			while (true) {
				if (l > r) {
					return t + Math.Min(l, r) + 1;
				}

				m = (l + r) / 2;
				x = excluded[m] - m;
				if (x <= t) {
					l = m + 1;
				} else if (x > t) {
					r = m - 1;
				}
			}
		}
	}
}
