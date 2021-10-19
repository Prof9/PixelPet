using System;
using System.Collections.Generic;
using System.IO;

namespace LibPixelPet {
	/// <summary>
	/// A writer that writes pixels to a stream with a given color format.
	/// </summary>
	public class PixelWriter : IDisposable {
		/// <summary>
		/// Gets the color format being written.
		/// </summary>
		public ColorFormat ColorFormat { get; }
		/// <summary>
		/// Gets the base stream being serialized to.
		/// </summary>
		public Stream BaseStream { get; }

		/// <summary>
		/// Gets a boolean that indicates whether a write operation has started.
		/// </summary>
		public bool IsWriting { get; private set; }

		/// <summary>
		/// Gets the pixel bytes buffer.
		/// </summary>
		private byte[] PixelBuffer { get; }
		/// <summary>
		/// Gets the number of bits written to the pixel buffer.
		/// </summary>
		private int BitsWritten { get; set; }
		/// <summary>
		/// Gets the bit shift count for the pixel buffer.
		/// </summary>
		private int BitShift { get; set; }

		/// <summary>
		/// Gets or sets whether to leave the underlying stream open when this PixelReader is closed.
		/// </summary>
		private bool LeaveOpen { get; set; }

		/// <summary>
		/// Creates a new PixelWriter using the given color format that writes to the given stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="colFmt">The color format for the stream.</param>
		/// <param name="leaveOpen">Whether to leave the underlying stream open when this PixelWriter is disposed.</param>
		public PixelWriter(Stream stream, ColorFormat colFmt, bool leaveOpen) {
			if (stream is null)
				throw new ArgumentNullException(nameof(stream));

			this.BaseStream  = stream;
			this.ColorFormat = colFmt;
			this.IsWriting   = false;
			this.BitsWritten = 0;
			this.BitShift    = 0;
			this.LeaveOpen   = leaveOpen;

			// Add 1 byte to account for bad alignment
			this.PixelBuffer = new byte[colFmt.Bytes + 1];
		}

		/// <summary>
		/// Writes pixels to the stream.
		/// </summary>
		/// <param name="source">List of pixels to read from.</param>
		/// <param name="offset">Offset in array to start reading.</param>
		/// <param name="pixelCount">Number of pixels to write.</param>
		public void WritePixels(IList<int> source, int offset, int pixelCount) {
			if (offset < 0 || offset > source.Count)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (pixelCount < 0 || offset + pixelCount > source.Count)
				throw new ArgumentOutOfRangeException(nameof(pixelCount));
			
			this.BeginWrite();

			for (int i = 0; i < pixelCount; i++) {
				this.WriteNextPixel(source[offset + i]);
			}

			this.EndWrite();
		}

		/// <summary>
		/// Starts a new write operation.
		/// </summary>
		public void BeginWrite() {
			if (this.IsWriting) {
				throw new InvalidOperationException("Already performing a write operation.");
			}

			for (int i = 0; i < this.PixelBuffer.Length; i++) {
				this.PixelBuffer[i] = 0;
			}
			this.BitsWritten = 0;
			this.BitShift    = 0;
			this.IsWriting   = true;
		}

		/// <summary>
		/// Ends a write operation.
		/// </summary>
		public void EndWrite() {
			if (!this.IsWriting) {
				throw new InvalidOperationException("Not performing a write operation.");
			}

			this.FlushData(true);
			this.IsWriting = false;
		}

		/// <summary>
		/// Writes the next pixel to the base stream in an ongoing write operation.
		/// The write operation must be started with BeginWrite and ended with EndWrite.
		/// </summary>
		/// <param name="pixel">The pixel to write.</param>
		public void WriteNextPixel(int pixel) {
			if (!this.IsWriting) {
				throw new InvalidOperationException("BeginWrite must be called first.");
			}

			int bitsWrote = 0;
			int byteIndex = 0;
			while (bitsWrote < this.ColorFormat.Bits) {
				int bitsToWrite = this.ColorFormat.Encoding switch {
					// Game Boy writes 1 bit per byte
					PixelEncoding.GameBoy => 1,
					// Write all bits for this pixel that are in this byte
					_ => Math.Min(this.ColorFormat.Bits, 8 - this.BitShift),
				};

				int x = pixel & ((1 << bitsToWrite) - 1);

				if (this.ColorFormat.Encoding == PixelEncoding.GameBoy) {
					x <<= 8 - bitsToWrite - this.BitShift;
					// Game Boy updates bit shift after writing all bytes
				} else {
					x <<= this.BitShift;
					this.BitShift = (this.BitShift + bitsToWrite) % 8;
				}

				this.PixelBuffer[byteIndex] |= (byte)x;
				pixel >>= bitsToWrite;

				bitsWrote += bitsToWrite;
				byteIndex++;
			}
			this.BitsWritten += bitsWrote;

			if (this.ColorFormat.Encoding == PixelEncoding.GameBoy) {
				this.BitShift = (this.BitShift + 1) % 8;
			}

			this.FlushData(false);
		}

		/// <summary>
		/// Flushes data in the pixel buffer to the base stream.
		/// </summary>
		/// <param name="force">If true, also flush data even when not all bits have been written.</param>
		private void FlushData(bool force) {
			if (this.BitsWritten >= this.ColorFormat.Bytes * 8) {
				int bytesToWrite = force switch {
					true => (this.BitsWritten + 7) / 8,
					false => this.BitsWritten / 8,
				};
				int bytesLeft = this.PixelBuffer.Length - bytesToWrite;

				this.BaseStream.Write(this.PixelBuffer, 0, bytesToWrite);

				for (int i = 0; i < bytesLeft; i++) {
					this.PixelBuffer[i] = this.PixelBuffer[this.PixelBuffer.Length - bytesLeft + i];
				}
				for (int i = this.PixelBuffer.Length - bytesToWrite; i < this.PixelBuffer.Length; i++) {
					this.PixelBuffer[i] = 0;
				}

				this.BitsWritten -= bytesToWrite * 8;
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (!this.LeaveOpen) {
					this.BaseStream?.Close();
				}
			}
		}

		~PixelWriter() => this.Dispose(false);

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
