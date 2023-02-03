using System;
using System.Collections.Generic;
using System.IO;

namespace LibPixelPet {
	/// <summary>
	/// A reader that reads pixels from a stream with a given color format.
	/// </summary>
	public class PixelReader : IDisposable {
		/// <summary>
		/// Gets the color format being read.
		/// </summary>
		public ColorFormat ColorFormat { get; }
		/// <summary>
		/// Gets the base stream being read from.
		/// </summary>
		public Stream BaseStream { get; }

		/// <summary>
		/// Gets a boolean that indicates whether a read operation has started.
		/// </summary>
		public bool IsReading { get; private set; }

		/// <summary>
		/// Gets the pixel bytes buffer.
		/// </summary>
		private byte[] PixelBuffer { get; }
		/// <summary>
		/// Gets the number of bits left in the pixel buffer.
		/// </summary>
		private int BitsLeft { get; set; }
		/// <summary>
		/// Gets the bit shift count for the pixel buffer.
		/// </summary>
		private int BitShift { get; set; }

		/// <summary>
		/// Gets or sets whether to leave the underlying stream open when this PixelReader is closed.
		/// </summary>
		private bool LeaveOpen { get; set; }

		/// <summary>
		/// Creates a new PixelReader using the given color format that reads from the given stream.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		/// <param name="colFmt">The color format of the stream.</param>
		/// <param name="leaveOpen">Whether to leave the underlying stream open when this PixelReader is disposed.</param>
		public PixelReader(Stream stream, ColorFormat colFmt, bool leaveOpen) {
			if (stream is null)
				throw new ArgumentNullException(nameof(stream));
			
			BaseStream  = stream;
			ColorFormat = colFmt;
			IsReading   = false;
			BitsLeft    = 0;
			BitShift    = 0;
			LeaveOpen   = leaveOpen;

			// Add 1 byte to account for bad alignment
			PixelBuffer = new byte[colFmt.Bytes + 1];
		}

		/// <summary>
		/// Reads pixels from the stream.
		/// </summary>
		/// <param name="destination">List of pixels to write to.</param>
		/// <param name="offset">Offset in array to start writing.</param>
		/// <param name="pixelCount">Number of pixels to read.</param>
		/// <returns>The number of pixels that were read.</returns>
		public int ReadPixels(IList<int> destination, int offset, int pixelCount) {
			if (destination is null)
				throw new ArgumentNullException(nameof(destination));
			if (offset < 0 || offset > destination.Count)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (pixelCount < 0 || offset + pixelCount > destination.Count)
				throw new ArgumentOutOfRangeException(nameof(pixelCount));
			
			BeginRead();

			int pixelsRead = 0;
			while (pixelsRead < pixelCount) {
				if (!FeedData()) {
					break;
				}

				destination[offset + pixelsRead] = ReadNextPixel();
				pixelsRead++;
			}

			EndRead();
			return pixelsRead;
		}

		/// <summary>
		/// Starts a new read operation.
		/// </summary>
		public void BeginRead() {
			if (IsReading) {
				throw new InvalidOperationException("Already performing a read operation.");
			}
			BitsLeft  = 0;
			BitShift  = 0;
			IsReading = true;
		}

		/// <summary>
		/// Ends a read operation.
		/// </summary>
		public void EndRead() {
			if (!IsReading) {
				throw new InvalidOperationException("Not performing a read operation.");
			}
			IsReading = false;
		}

		/// <summary>
		/// Reads the next pixel from the base stream in an ongoing read operation.
		/// The read operation must be started with BeginRead and ended with EndRead.
		/// </summary>
		/// <returns>The pixel that was read.</returns>
		public int ReadNextPixel() {
			if (!IsReading) {
				throw new InvalidOperationException("BeginRead must be called first.");
			}
			if (!FeedData()) {
				throw new IOException("Read past the end of the stream");
			}

			int pixel     = 0;
			int bitsRead  = 0;
			int byteIndex = 0;
			while (bitsRead < ColorFormat.Bits) {
				int bitsToRead = ColorFormat.Encoding switch {
					// Game Boy reads 1 bit per byte
					PixelEncoding.GameBoy => 1,
					// Read all bits for this pixel that are in this byte
					_ => Math.Min(ColorFormat.Bits, 8 - BitShift),
				};

				int x = PixelBuffer[byteIndex];

				if (ColorFormat.Encoding == PixelEncoding.GameBoy) {
					x >>= 8 - bitsToRead - BitShift;
					// Game Boy updates bit shift after reading all bytes
				} else {
					x >>= BitShift;
					BitShift = (BitShift + bitsToRead) % 8;
				}

				x &= (1 << bitsToRead) - 1;
				pixel |= x << bitsRead;

				bitsRead += bitsToRead;
				byteIndex++;
			}
			BitsLeft -= bitsRead;

			if (ColorFormat.Encoding == PixelEncoding.GameBoy) {
				BitShift = (BitShift + 1) % 8;
			}

			return pixel;
		}

		/// <summary>
		/// Feeds data for next pixel into pixel buffer.
		/// </summary>
		/// <returns>true if there is more data; false otherwise.</returns>
		private bool FeedData() {
			if (BitsLeft < ColorFormat.Bits) {
				int bytesToRead = ColorFormat.Bytes;
				int bytesLeft = (BitsLeft + 7) / 8;

				for (int i = 0; i < bytesLeft; i++) {
					PixelBuffer[i] = PixelBuffer[PixelBuffer.Length - bytesLeft + i];
				}

				if (BaseStream.Read(PixelBuffer, bytesLeft, bytesToRead) != bytesToRead) {
					// Read past end of stream
					return false;
				}

				BitsLeft += bytesToRead * 8;
			}
			return true;
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (!LeaveOpen) {
					BaseStream?.Close();
				}
			}
		}

		~PixelReader() => Dispose(false);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
