using System.Text;

namespace Tests {
	/// <summary>
	/// A text writer that buffers and passes through text to an underlying writer.
	/// </summary>
	internal class PassThroughBufferTextWriter : TextWriter {
		/// <summary>
		/// Gets the underlying text writer.
		/// </summary>
		private TextWriter BaseWriter { get; }
		/// <summary>
		/// Gets the own text buffer.
		/// </summary>
		private StringBuilder OwnBuffer { get; } = new StringBuilder();

		/// <summary>
		/// Gets the encoding in which output is written.
		/// </summary>
		public override Encoding Encoding => BaseWriter.Encoding;

		/// <summary>
		/// Creates a new passthrough text writer.
		/// </summary>
		/// <param name="writer">Underlying writer to pass text to.</param>
		public PassThroughBufferTextWriter(TextWriter writer) {
			BaseWriter = writer;
		}

		public override void Write(char value) {
			OwnBuffer.Append(value);
			BaseWriter.Write(value);
		}
		public override void Write(string? value) {
			OwnBuffer.Append(value);
			BaseWriter.Write(value);
		}
		public override void Flush() => BaseWriter.Flush();
		public override Task FlushAsync() => BaseWriter.FlushAsync();

		public override string ToString() => OwnBuffer.ToString();
	}
}
