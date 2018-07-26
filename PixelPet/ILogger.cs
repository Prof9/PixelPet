using System;

namespace PixelPet {
	internal interface ILogger {
		void Log(string logString, LogLevel logLevel = 0);

		void SetVerbosity(bool verbose);
		void ResetVerbosity();
	}
}
