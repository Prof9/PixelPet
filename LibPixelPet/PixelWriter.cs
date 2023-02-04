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
			ArgumentNullException.ThrowIfNull(stream);

			BaseStream  = stream;
			ColorFormat = colFmt;
			IsWriting   = false;
			BitsWritten = 0;
			BitShift    = 0;
			LeaveOpen   = leaveOpen;

			// Add 1 byte to account for bad alignment
			PixelBuffer = new byte[colFmt.Bytes + 1];
		}

		/// <summary>
		/// Writes pixels to the stream.
		/// </summary>
		/// <param name="source">List of pixels to read from.</param>
		/// <param name="offset">Offset in array to start reading.</param>
		/// <param name="pixelCount">Number of pixels to write.</param>
		public void WritePixels(IList<int> source, int offset, int pixelCount) {
			ArgumentNullException.ThrowIfNull(source);
			if (offset < 0 || offset > source.Count)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (pixelCount < 0 || offset + pixelCount > source.Count)
				throw new ArgumentOutOfRangeException(nameof(pixelCount));
			
			BeginWrite();

			for (int i = 0; i < pixelCount; i++) {
				WriteNextPixel(source[offset + i]);
			}

			EndWrite();
		}

		/// <summary>
		/// Starts a new write operation.
		/// </summary>
		public void BeginWrite() {
			if (IsWriting) {
				throw new InvalidOperationException("Already performing a write operation.");
			}

			for (int i = 0; i < PixelBuffer.Length; i++) {
				PixelBuffer[i] = 0;
			}
			BitsWritten = 0;
			BitShift    = 0;
			IsWriting   = true;
		}

		/// <summary>
		/// Ends a write operation.
		/// </summary>
		public void EndWrite() {
			if (!IsWriting) {
				throw new InvalidOperationException("Not performing a write operation.");
			}

			FlushData(true);
			IsWriting = false;
		}

		/// <summary>
		/// Writes the next pixel to the base stream in an ongoing write operation.
		/// The write operation must be started with BeginWrite and ended with EndWrite.
		/// </summary>
		/// <param name="pixel">The pixel to write.</param>
		public void WriteNextPixel(int pixel) {
			if (!IsWriting) {
				throw new InvalidOperationException("BeginWrite must be called first.");
			}

			int bitsWrote = 0;
			int byteIndex = 0;
			while (bitsWrote < ColorFormat.Bits) {
				int bitsToWrite = ColorFormat.Encoding switch {
					// Game Boy writes 1 bit per byte
					PixelEncoding.GameBoy => 1,
					// Write all bits for this pixel that are in this byte
					_ => Math.Min(ColorFormat.Bits, 8 - BitShift),
				};

				int x = pixel & ((1 << bitsToWrite) - 1);

				if (ColorFormat.Encoding == PixelEncoding.GameBoy) {
					x <<= 8 - bitsToWrite - BitShift;
					// Game Boy updates bit shift after writing all bytes
				} else {
					x <<= BitShift;
					BitShift = (BitShift + bitsToWrite) % 8;
				}

				PixelBuffer[byteIndex] |= (byte)x;
				pixel >>= bitsToWrite;

				bitsWrote += bitsToWrite;
				byteIndex++;
			}
			BitsWritten += bitsWrote;

			if (ColorFormat.Encoding == PixelEncoding.GameBoy) {
				BitShift = (BitShift + 1) % 8;
			}

			FlushData(false);
		}

		/// <summary>
		/// Flushes data in the pixel buffer to the base stream.
		/// </summary>
		/// <param name="force">If true, also flush data even when not all bits have been written.</param>
		private void FlushData(bool force) {
			if (BitsWritten >= ColorFormat.Bytes * 8) {
				int bytesToWrite = force switch {
					true => (BitsWritten + 7) / 8,
					false => BitsWritten / 8,
				};
				int bytesLeft = PixelBuffer.Length - bytesToWrite;

				BaseStream.Write(PixelBuffer, 0, bytesToWrite);

				for (int i = 0; i < bytesLeft; i++) {
					PixelBuffer[i] = PixelBuffer[PixelBuffer.Length - bytesLeft + i];
				}
				for (int i = PixelBuffer.Length - bytesToWrite; i < PixelBuffer.Length; i++) {
					PixelBuffer[i] = 0;
				}

				BitsWritten -= bytesToWrite * 8;
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (!LeaveOpen) {
					BaseStream?.Close();
				}
			}
		}

		~PixelWriter() => Dispose(false);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
