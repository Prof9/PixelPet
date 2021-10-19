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
			
			this.BaseStream  = stream;
			this.ColorFormat = colFmt;
			this.IsReading   = false;
			this.BitsLeft    = 0;
			this.BitShift    = 0;
			this.LeaveOpen   = leaveOpen;

			// Add 1 byte to account for bad alignment
			this.PixelBuffer = new byte[colFmt.Bytes + 1];
		}

		/// <summary>
		/// Reads pixels from the stream.
		/// </summary>
		/// <param name="destination">List of pixels to write to.</param>
		/// <param name="offset">Offset in array to start writing.</param>
		/// <param name="pixelCount">Number of pixels to read.</param>
		/// <returns>The number of pixels that were read.</returns>
		public int ReadPixels(IList<int> destination, int offset, int pixelCount) {
			if (offset < 0 || offset > destination.Count)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (pixelCount < 0 || offset + pixelCount > destination.Count)
				throw new ArgumentOutOfRangeException(nameof(pixelCount));
			
			this.BeginRead();

			int pixelsRead = 0;
			while (pixelsRead < pixelCount) {
				if (!this.FeedData()) {
					break;
				}

				destination[offset + pixelsRead] = this.ReadNextPixel();
				pixelsRead++;
			}

			this.EndRead();
			return pixelsRead;
		}

		/// <summary>
		/// Starts a new read operation.
		/// </summary>
		public void BeginRead() {
			if (this.IsReading) {
				throw new InvalidOperationException("Already performing a read operation.");
			}
			this.BitsLeft  = 0;
			this.BitShift  = 0;
			this.IsReading = true;
		}

		/// <summary>
		/// Ends a read operation.
		/// </summary>
		public void EndRead() {
			if (!this.IsReading) {
				throw new InvalidOperationException("Not performing a read operation.");
			}
			this.IsReading = false;
		}

		/// <summary>
		/// Reads the next pixel from the base stream in an ongoing read operation.
		/// The read operation must be started with BeginRead and ended with EndRead.
		/// </summary>
		/// <returns>The pixel that was read.</returns>
		public int ReadNextPixel() {
			if (!this.IsReading) {
				throw new InvalidOperationException("BeginRead must be called first.");
			}
			if (!this.FeedData()) {
				throw new IOException("Read past the end of the stream");
			}

			int pixel     = 0;
			int bitsRead  = 0;
			int byteIndex = 0;
			while (bitsRead < this.ColorFormat.Bits) {
				int bitsToRead = this.ColorFormat.Encoding switch {
					// Game Boy reads 1 bit per byte
					PixelEncoding.GameBoy => 1,
					// Read all bits for this pixel that are in this byte
					_ => Math.Min(this.ColorFormat.Bits, 8 - this.BitShift),
				};

				int x = this.PixelBuffer[byteIndex];

				if (this.ColorFormat.Encoding == PixelEncoding.GameBoy) {
					x >>= 8 - bitsToRead - this.BitShift;
					// Game Boy updates bit shift after reading all bytes
				} else {
					x >>= this.BitShift;
					this.BitShift = (this.BitShift + bitsToRead) % 8;
				}

				x &= (1 << bitsToRead) - 1;
				pixel |= x << bitsRead;

				bitsRead += bitsToRead;
				byteIndex++;
			}
			this.BitsLeft -= bitsRead;

			if (this.ColorFormat.Encoding == PixelEncoding.GameBoy) {
				this.BitShift = (this.BitShift + 1) % 8;
			}

			return pixel;
		}

		/// <summary>
		/// Feeds data for next pixel into pixel buffer.
		/// </summary>
		/// <returns>true if there is more data; false otherwise.</returns>
		private bool FeedData() {
			if (this.BitsLeft < this.ColorFormat.Bits) {
				int bytesToRead = this.ColorFormat.Bytes;
				int bytesLeft = (this.BitsLeft + 7) / 8;

				for (int i = 0; i < bytesLeft; i++) {
					this.PixelBuffer[i] = this.PixelBuffer[this.PixelBuffer.Length - bytesLeft + i];
				}

				if (this.BaseStream.Read(this.PixelBuffer, bytesLeft, bytesToRead) != bytesToRead) {
					// Read past end of stream
					return false;
				}

				this.BitsLeft += bytesToRead * 8;
			}
			return true;
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (!this.LeaveOpen) {
					this.BaseStream?.Close();
				}
			}
		}

		~PixelReader() => this.Dispose(false);

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
