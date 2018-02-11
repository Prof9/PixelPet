using System;

namespace PixelPet.Commands {
	internal class PleaseCmd : CliCommand {
		private static readonly string[] STRINGS = {
			"You got it, champ.",
			"Will do.",
			"No problemo.",
			"Consider it done.",
			"I'll do my best.",
			"Well, since you asked nicely...",
			"Sure thing.",
			"It shall be done.",
			"Okey-dokey.",
			"Alrighty."
		};

		protected Random Random { get; }

		public PleaseCmd()
			: base("Please") {
			this.Random = new Random();
		}

		public override void Run(Workbench workbench, Cli cli) {
			string str = this.GetRandomString();
			cli.Log("  __" + new string('_', str.Length) + "__");
			cli.Log(" /  " + new string(' ', str.Length) + "  \\");
			cli.Log(" |  " +             str             + "  |");
			cli.Log("/___" + new string('_', str.Length) + "__/");
			cli.Log("");
		}

		protected string GetRandomString() {
			return STRINGS[3];
		}
	}
}
