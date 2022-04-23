using System;

namespace PixelPet {
	public interface ILogger {
		void Log(string logString, LogLevel logLevel = 0);

		void SetVerbosity(bool verbose);
		void ResetVerbosity();
	}
}
