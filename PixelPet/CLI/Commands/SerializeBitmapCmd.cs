using LibPixelPet;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelPet.CLI.Commands {
	internal class SerializeBitmapCmd : CliCommand {
		public SerializeBitmapCmd()
			: base("Serialize-Bitmap",
				new Parameter("append", "a", false)
			) { }

		protected override void Run(Workbench workbench, ILogger logger) {
			bool append = FindNamedParameter("--append").IsPresent;

			if (append) {
				workbench.Stream.Position = workbench.Stream.Length;
			} else {
				workbench.Stream.SetLength(0);
			}

			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, workbench.Bitmap.Width, workbench.Bitmap.Height),
				ImageLockMode.ReadWrite,
				workbench.Bitmap.PixelFormat
			);
			int[] buffer = new int[bmpData.Stride * workbench.Bitmap.Height];
			Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length / 4);

			using (PixelWriter pixelWriter = new PixelWriter(workbench.Stream, workbench.BitmapFormat, true)) {
				pixelWriter.WritePixels(buffer, 0, workbench.Bitmap.Width * workbench.Bitmap.Height);
			}

			workbench.Bitmap.UnlockBits(bmpData);

			logger?.Log("Serialized " + workbench.Bitmap.Width + "x" + workbench.Bitmap.Height + " bitmap.", LogLevel.Information);
		}
	}
}
