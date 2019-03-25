using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    // ****************************************************************************
    //
    // FLV Extract
    // Copyright (C) 2006-2012  J.D. Purcell (moitah@yahoo.com)
    //
    // This program is free software; you can redistribute it and/or modify
    // it under the terms of the GNU General Public License as published by
    // the Free Software Foundation; either version 2 of the License, or
    // (at your option) any later version.
    //
    // This program is distributed in the hope that it will be useful,
    // but WITHOUT ANY WARRANTY; without even the implied warranty of
    // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    // GNU General Public License for more details.
    //
    // You should have received a copy of the GNU General Public License
    // along with this program; if not, write to the Free Software
    // Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
    //
    // ****************************************************************************

    using System;
    using System.IO;

    namespace YoutubeExtractor
    {
        internal class FlvFile : IDisposable
        {
            private readonly long fileLength;
            private readonly string inputPath;
            private readonly string outputPath;
            private IAudioExtractor audioExtractor;
            private long fileOffset;
            private FileStream fileStream;

            /// <summary>
            /// Initializes a new instance of the <see cref="FlvFile"/> class.
            /// </summary>
            /// <param name="inputPath">The path of the input.</param>
            /// <param name="outputPath">The path of the output without extension.</param>
            public FlvFile(string inputPath, string outputPath)
            {
                this.inputPath = inputPath;
                this.outputPath = outputPath;
                this.fileStream = new FileStream(this.inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
                this.fileOffset = 0;
                this.fileLength = fileStream.Length;
            }

            public event EventHandler<ProgressEventArgs> ConversionProgressChanged;

            public bool ExtractedAudio { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <exception cref="Exception">The input file is not an FLV file.</exception>
            public void ExtractStreams()
            {
                this.Seek(0);

                if (this.ReadUInt32() != 0x464C5601)
                {
                    // not a FLV file
                    throw new Exception("Invalid input file. Impossible to extract audio track.");
                }

                this.ReadUInt8();
                uint dataOffset = this.ReadUInt32();

                this.Seek(dataOffset);

                this.ReadUInt32();

                while (fileOffset < fileLength)
                {
                    if (!ReadTag())
                    {
                        break;
                    }

                    if (fileLength - fileOffset < 4)
                    {
                        break;
                    }

                    this.ReadUInt32();

                    double progress = (this.fileOffset * 1.0 / this.fileLength) * 100;

                    if (this.ConversionProgressChanged != null)
                    {
                        this.ConversionProgressChanged(this, new ProgressEventArgs(progress));
                    }
                }

                this.CloseOutput(false);
            }

            private void CloseOutput(bool disposing)
            {
                if (this.audioExtractor != null)
                {
                    if (disposing && this.audioExtractor.VideoPath != null)
                    {
                        try
                        {
                            File.Delete(this.audioExtractor.VideoPath);
                        }
                        catch { }
                    }

                    this.audioExtractor.Dispose();
                    this.audioExtractor = null;
                }
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.fileStream != null)
                    {
                        this.fileStream.Close();
                        this.fileStream = null;
                    }

                    this.CloseOutput(true);
                }
            }

            private IAudioExtractor GetAudioWriter(uint mediaInfo)
            {
                uint format = mediaInfo >> 4;

                switch (format)
                {
                    case 14:
                    case 2:
                        return new Mp3AudioExtractor(this.outputPath);

                    case 10:
                        return new AacAudioExtractor(this.outputPath);
                }

                string typeStr;

                switch (format)
                {
                    case 1:
                        typeStr = "ADPCM";
                        break;

                    case 6:
                    case 5:
                    case 4:
                        typeStr = "Nellymoser";
                        break;

                    default:
                        typeStr = "format=" + format;
                        break;
                }

                throw new Exception("Unable to extract audio (" + typeStr + " is unsupported).");
            }

            private byte[] ReadBytes(int length)
            {
                var buff = new byte[length];

                this.fileStream.Read(buff, 0, length);
                this.fileOffset += length;

                return buff;
            }

            private bool ReadTag()
            {
                if (this.fileLength - this.fileOffset < 11)
                    return false;

                // Read tag header
                uint tagType = ReadUInt8();
                uint dataSize = ReadUInt24();
                uint timeStamp = ReadUInt24();
                timeStamp |= this.ReadUInt8() << 24;
                this.ReadUInt24();

                // Read tag data
                if (dataSize == 0)
                    return true;

                if (this.fileLength - this.fileOffset < dataSize)
                    return false;

                uint mediaInfo = this.ReadUInt8();
                dataSize -= 1;
                byte[] data = this.ReadBytes((int)dataSize);

                if (tagType == 0x8)
                {
                    // If we have no audio writer, create one
                    if (this.audioExtractor == null)
                    {
                        this.audioExtractor = this.GetAudioWriter(mediaInfo);
                        this.ExtractedAudio = this.audioExtractor != null;
                    }

                    if (this.audioExtractor == null)
                    {
                        throw new InvalidOperationException("No supported audio writer found.");
                    }

                    this.audioExtractor.WriteChunk(data, timeStamp);
                }

                return true;
            }

            private uint ReadUInt24()
            {
                var x = new byte[4];

                this.fileStream.Read(x, 1, 3);
                this.fileOffset += 3;

                return BigEndianBitConverter.ToUInt32(x, 0);
            }

            private uint ReadUInt32()
            {
                var x = new byte[4];

                this.fileStream.Read(x, 0, 4);
                this.fileOffset += 4;

                return BigEndianBitConverter.ToUInt32(x, 0);
            }

            private uint ReadUInt8()
            {
                this.fileOffset += 1;
                return (uint)this.fileStream.ReadByte();
            }

            private void Seek(long offset)
            {
                this.fileStream.Seek(offset, SeekOrigin.Begin);
                this.fileOffset = offset;
            }
        }
    }

    internal interface IAudioExtractor : IDisposable
    {
        string VideoPath { get; }

        /// <exception cref="Exception">An error occured while writing the chunk.</exception>
        void WriteChunk(byte[] chunk, uint timeStamp);
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(double progressPercentage)
        {
            this.ProgressPercentage = progressPercentage;
        }

        /// <summary>
        /// Gets or sets a token whether the operation that reports the progress should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the progress percentage in a range from 0.0 to 100.0.
        /// </summary>
        public double ProgressPercentage { get; private set; }
    }

    internal class Mp3AudioExtractor : IAudioExtractor
    {
        private readonly List<byte[]> chunkBuffer;
        private readonly FileStream fileStream;
        private readonly List<uint> frameOffsets;
        private readonly List<string> warnings;
        private int channelMode;
        private bool delayWrite;
        private int firstBitRate;
        private uint firstFrameHeader;
        private bool hasVbrHeader;
        private bool isVbr;
        private int mpegVersion;
        private int sampleRate;
        private uint totalFrameLength;
        private bool writeVbrHeader;

        public Mp3AudioExtractor(string path)
        {
            this.VideoPath = path;
            this.fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024);
            this.warnings = new List<string>();
            this.chunkBuffer = new List<byte[]>();
            this.frameOffsets = new List<uint>();
            this.delayWrite = true;
        }

        public string VideoPath { get; private set; }

        public IEnumerable<string> Warnings
        {
            get { return this.warnings; }
        }

        public void Dispose()
        {
            this.Flush();

            if (this.writeVbrHeader)
            {
                this.fileStream.Seek(0, SeekOrigin.Begin);
                this.WriteVbrHeader(false);
            }

            this.fileStream.Dispose();
        }

        public void WriteChunk(byte[] chunk, uint timeStamp)
        {
            this.chunkBuffer.Add(chunk);
            this.ParseMp3Frames(chunk);

            if (this.delayWrite && this.totalFrameLength >= 65536)
            {
                this.delayWrite = false;
            }

            if (!this.delayWrite)
            {
                this.Flush();
            }
        }

        private static int GetFrameDataOffset(int mpegVersion, int channelMode)
        {
            return 4 + (mpegVersion == 3 ?
                (channelMode == 3 ? 17 : 32) :
                (channelMode == 3 ? 9 : 17));
        }

        private static int GetFrameLength(int mpegVersion, int bitRate, int sampleRate, int padding)
        {
            return (mpegVersion == 3 ? 144 : 72) * bitRate / sampleRate + padding;
        }

        private void Flush()
        {
            foreach (byte[] chunk in chunkBuffer)
            {
                this.fileStream.Write(chunk, 0, chunk.Length);
            }

            this.chunkBuffer.Clear();
        }

        private void ParseMp3Frames(byte[] buffer)
        {
            var mpeg1BitRate = new[] { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 };
            var mpeg2XBitRate = new[] { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 };
            var mpeg1SampleRate = new[] { 44100, 48000, 32000, 0 };
            var mpeg20SampleRate = new[] { 22050, 24000, 16000, 0 };
            var mpeg25SampleRate = new[] { 11025, 12000, 8000, 0 };

            int offset = 0;
            int length = buffer.Length;

            while (length >= 4)
            {
                int mpegVersion, sampleRate, channelMode;

                ulong header = (ulong)BigEndianBitConverter.ToUInt32(buffer, offset) << 32;

                if (BitHelper.Read(ref header, 11) != 0x7FF)
                {
                    break;
                }

                mpegVersion = BitHelper.Read(ref header, 2);
                int layer = BitHelper.Read(ref header, 2);
                BitHelper.Read(ref header, 1);
                int bitRate = BitHelper.Read(ref header, 4);
                sampleRate = BitHelper.Read(ref header, 2);
                int padding = BitHelper.Read(ref header, 1);
                BitHelper.Read(ref header, 1);
                channelMode = BitHelper.Read(ref header, 2);

                if (mpegVersion == 1 || layer != 1 || bitRate == 0 || bitRate == 15 || sampleRate == 3)
                {
                    break;
                }

                bitRate = (mpegVersion == 3 ? mpeg1BitRate[bitRate] : mpeg2XBitRate[bitRate]) * 1000;

                switch (mpegVersion)
                {
                    case 2:
                        sampleRate = mpeg20SampleRate[sampleRate];
                        break;

                    case 3:
                        sampleRate = mpeg1SampleRate[sampleRate];
                        break;

                    default:
                        sampleRate = mpeg25SampleRate[sampleRate];
                        break;
                }

                int frameLenght = GetFrameLength(mpegVersion, bitRate, sampleRate, padding);

                if (frameLenght > length)
                {
                    break;
                }

                bool isVbrHeaderFrame = false;

                if (frameOffsets.Count == 0)
                {
                    // Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
                    int o = offset + GetFrameDataOffset(mpegVersion, channelMode);

                    if (BigEndianBitConverter.ToUInt32(buffer, o) == 0x58696E67)
                    {
                        // "Xing"
                        isVbrHeaderFrame = true;
                        this.delayWrite = false;
                        this.hasVbrHeader = true;
                    }
                }

                if (!isVbrHeaderFrame)
                {
                    if (this.firstBitRate == 0)
                    {
                        this.firstBitRate = bitRate;
                        this.mpegVersion = mpegVersion;
                        this.sampleRate = sampleRate;
                        this.channelMode = channelMode;
                        this.firstFrameHeader = BigEndianBitConverter.ToUInt32(buffer, offset);
                    }

                    else if (!this.isVbr && bitRate != this.firstBitRate)
                    {
                        this.isVbr = true;

                        if (!this.hasVbrHeader)
                        {
                            if (this.delayWrite)
                            {
                                this.WriteVbrHeader(true);
                                this.writeVbrHeader = true;
                                this.delayWrite = false;
                            }

                            else
                            {
                                this.warnings.Add("Detected VBR too late, cannot add VBR header.");
                            }
                        }
                    }
                }

                this.frameOffsets.Add(this.totalFrameLength + (uint)offset);

                offset += frameLenght;
                length -= frameLenght;
            }

            this.totalFrameLength += (uint)buffer.Length;
        }

        private void WriteVbrHeader(bool isPlaceholder)
        {
            var buffer = new byte[GetFrameLength(this.mpegVersion, 64000, this.sampleRate, 0)];

            if (!isPlaceholder)
            {
                uint header = this.firstFrameHeader;
                int dataOffset = GetFrameDataOffset(this.mpegVersion, this.channelMode);
                header &= 0xFFFE0DFF; // Clear CRC, bitrate, and padding fields
                header |= (uint)(mpegVersion == 3 ? 5 : 8) << 12; // 64 kbit/sec
                BitHelper.CopyBytes(buffer, 0, BigEndianBitConverter.GetBytes(header));
                BitHelper.CopyBytes(buffer, dataOffset, BigEndianBitConverter.GetBytes(0x58696E67)); // "Xing"
                BitHelper.CopyBytes(buffer, dataOffset + 4, BigEndianBitConverter.GetBytes((uint)0x7)); // Flags
                BitHelper.CopyBytes(buffer, dataOffset + 8, BigEndianBitConverter.GetBytes((uint)frameOffsets.Count)); // Frame count
                BitHelper.CopyBytes(buffer, dataOffset + 12, BigEndianBitConverter.GetBytes(totalFrameLength)); // File length

                for (int i = 0; i < 100; i++)
                {
                    int frameIndex = (int)((i / 100.0) * this.frameOffsets.Count);

                    buffer[dataOffset + 16 + i] = (byte)(this.frameOffsets[frameIndex] / (double)this.totalFrameLength * 256.0);
                }
            }

            this.fileStream.Write(buffer, 0, buffer.Length);
        }
    }

    internal class AacAudioExtractor : IAudioExtractor
    {
        private readonly FileStream fileStream;
        private int aacProfile;
        private int channelConfig;
        private int sampleRateIndex;

        public AacAudioExtractor(string path)
        {
            this.VideoPath = path;
            fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024);
        }

        public string VideoPath { get; private set; }

        public void Dispose()
        {
            this.fileStream.Dispose();
        }

        public void WriteChunk(byte[] chunk, uint timeStamp)
        {
            if (chunk.Length < 1)
            {
                return;
            }

            if (chunk[0] == 0)
            {
                // Header
                if (chunk.Length < 3)
                {
                    return;
                }

                ulong bits = (ulong)BigEndianBitConverter.ToUInt16(chunk, 1) << 48;

                aacProfile = BitHelper.Read(ref bits, 5) - 1;
                sampleRateIndex = BitHelper.Read(ref bits, 4);
                channelConfig = BitHelper.Read(ref bits, 4);

                if (aacProfile < 0 || aacProfile > 3)
                    throw new Exception("Unsupported AAC profile.");
                if (sampleRateIndex > 12)
                    throw new Exception("Invalid AAC sample rate index.");
                if (channelConfig > 6)
                    throw new Exception("Invalid AAC channel configuration.");
            }

            else
            {
                // Audio data
                int dataSize = chunk.Length - 1;
                ulong bits = 0;

                // Reference: WriteADTSHeader from FAAC's bitstream.c

                BitHelper.Write(ref bits, 12, 0xFFF);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 2, 0);
                BitHelper.Write(ref bits, 1, 1);
                BitHelper.Write(ref bits, 2, aacProfile);
                BitHelper.Write(ref bits, 4, sampleRateIndex);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 3, channelConfig);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 13, 7 + dataSize);
                BitHelper.Write(ref bits, 11, 0x7FF);
                BitHelper.Write(ref bits, 2, 0);

                fileStream.Write(BigEndianBitConverter.GetBytes(bits), 1, 7);
                fileStream.Write(chunk, 1, dataSize);
            }
        }
    }

    internal static class BigEndianBitConverter
    {
        public static byte[] GetBytes(ulong value)
        {
            var buff = new byte[8];

            buff[0] = (byte)(value >> 56);
            buff[1] = (byte)(value >> 48);
            buff[2] = (byte)(value >> 40);
            buff[3] = (byte)(value >> 32);
            buff[4] = (byte)(value >> 24);
            buff[5] = (byte)(value >> 16);
            buff[6] = (byte)(value >> 8);
            buff[7] = (byte)(value);

            return buff;
        }

        public static byte[] GetBytes(uint value)
        {
            var buff = new byte[4];

            buff[0] = (byte)(value >> 24);
            buff[1] = (byte)(value >> 16);
            buff[2] = (byte)(value >> 8);
            buff[3] = (byte)(value);

            return buff;
        }

        public static byte[] GetBytes(ushort value)
        {
            var buff = new byte[2];

            buff[0] = (byte)(value >> 8);
            buff[1] = (byte)(value);

            return buff;
        }

        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)(value[startIndex] << 8 | value[startIndex + 1]);
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return
                (uint)value[startIndex] << 24 |
                (uint)value[startIndex + 1] << 16 |
                (uint)value[startIndex + 2] << 8 |
                value[startIndex + 3];
        }

        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return
                (ulong)value[startIndex] << 56 |
                (ulong)value[startIndex + 1] << 48 |
                (ulong)value[startIndex + 2] << 40 |
                (ulong)value[startIndex + 3] << 32 |
                (ulong)value[startIndex + 4] << 24 |
                (ulong)value[startIndex + 5] << 16 |
                (ulong)value[startIndex + 6] << 8 |
                value[startIndex + 7];
        }
    }

    internal static class BitHelper
    {
        public static byte[] CopyBlock(byte[] bytes, int offset, int length)
        {
            int startByte = offset / 8;
            int endByte = (offset + length - 1) / 8;
            int shiftA = offset % 8;
            int shiftB = 8 - shiftA;
            var dst = new byte[(length + 7) / 8];

            if (shiftA == 0)
            {
                Buffer.BlockCopy(bytes, startByte, dst, 0, dst.Length);
            }

            else
            {
                int i;

                for (i = 0; i < endByte - startByte; i++)
                {
                    dst[i] = (byte)(bytes[startByte + i] << shiftA | bytes[startByte + i + 1] >> shiftB);
                }

                if (i < dst.Length)
                {
                    dst[i] = (byte)(bytes[startByte + i] << shiftA);
                }
            }

            dst[dst.Length - 1] &= (byte)(0xFF << dst.Length * 8 - length);

            return dst;
        }

        public static void CopyBytes(byte[] dst, int dstOffset, byte[] src)
        {
            Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length);
        }

        public static int Read(ref ulong x, int length)
        {
            int r = (int)(x >> 64 - length);
            x <<= length;
            return r;
        }

        public static int Read(byte[] bytes, ref int offset, int length)
        {
            int startByte = offset / 8;
            int endByte = (offset + length - 1) / 8;
            int skipBits = offset % 8;
            ulong bits = 0;

            for (int i = 0; i <= Math.Min(endByte - startByte, 7); i++)
            {
                bits |= (ulong)bytes[startByte + i] << 56 - i * 8;
            }

            if (skipBits != 0)
            {
                Read(ref bits, skipBits);
            }

            offset += length;

            return Read(ref bits, length);
        }

        public static void Write(ref ulong x, int length, int value)
        {
            ulong mask = 0xFFFFFFFFFFFFFFFF >> 64 - length;
            x = x << length | (ulong)value & mask;
        }
    }

}
