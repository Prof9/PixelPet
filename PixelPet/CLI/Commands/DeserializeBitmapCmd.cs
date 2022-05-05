using LibPixelPet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelPet.CLI.Commands {
	internal class DeserializeBitmapCmd : CliCommand {
		public DeserializeBitmapCmd()
			: base("Deserialize-Bitmap",
				new Parameter(true, new ParameterValue("format")),
				new Parameter(true, new ParameterValue("width")),
				new Parameter(true, new ParameterValue("height")),
				new Parameter("offset", "o", false, new ParameterValue("count", "0"))
			) { }

		protected override bool RunImplementation(Workbench workbench, ILogger logger) {
			string fmtName = FindUnnamedParameter(0).Values[0].ToString();
			int w = FindUnnamedParameter(1).Values[0].ToInt32();
			int h = FindUnnamedParameter(2).Values[0].ToInt32();
			long offset = FindNamedParameter("--offset").Values[0].ToInt64();

			if (!(ColorFormat.GetFormat(fmtName) is ColorFormat fmt)) {
				logger?.Log("Unknown color format \"" + fmtName + "\".", LogLevel.Error);
				return false;
			}
			if (w <= 0) {
				logger?.Log("Invalid width.", LogLevel.Error);
				return false;
			}
			if (h <= 0) {
				logger?.Log("Invalid height.", LogLevel.Error);
				return false;
			}
			if (workbench.Stream.Length * 8 < fmt.Bits * w * h) {
				logger?.Log("Bytes stream is not long enough to hold " + w + "x" + h + " bitmap with format \"" + fmtName + "\".", LogLevel.Error);
				return false;
			}

			workbench.ClearBitmap(w, h);
			workbench.Stream.Position = offset;

			BitmapData bmpData = workbench.Bitmap.LockBits(
				new Rectangle(0, 0, w, h),
				ImageLockMode.WriteOnly,
				workbench.Bitmap.PixelFormat
			);
			int[] buffer = new int[(bmpData.Stride * h) / 4];

			using (PixelReader pixelReader = new PixelReader(workbench.Stream, fmt, true)) {
				pixelReader.ReadPixels(buffer, 0, w * h);
			}
			workbench.BitmapFormat = fmt;

			Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
			workbench.Bitmap.UnlockBits(bmpData);

			logger?.Log("Deserialized " + workbench.Bitmap.Width + "x" + workbench.Bitmap.Height + " bitmap.", LogLevel.Information);
			return true;
		}
	}
}
