using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MythCodec.Lib
{
    class BitReader
    {
        readonly Stream stream;

        private int bitIndex;
        private byte cacheByte;

        public BitReader(Stream stream)
        {
            this.stream = stream;
            if (stream == null)
            {
                EndOfStream = true;
                return;
            }
            Read();
        }
        public bool EndOfStream { get; private set; }
        private void Read()
        {
            try
            {
                var cache = stream.ReadByte();
                if (cache == -1)
                {
                    EndOfStream = true;
                    stream.Dispose();
                    return;
                }
                cacheByte = (byte)cache;
            }
            catch (EndOfStreamException)
            {
                EndOfStream = true;
                stream.Dispose();
            }
        }
        /// <summary>
        /// 读取n个比特转换成ulong
        /// </summary>
        /// <param name="n">比特数，不超过64</param>
        /// <param name="bitCount">实际读取位数</param>
        /// <returns></returns>
        public ulong ReadULong(int n)
        {
            if (EndOfStream) throw new EndOfStreamException();
            if (n > 64) throw new NotSupportedException();
            ulong ret = 0;
            int retBitIndex = 0;
            while (n-->0) {
                ret |= (cacheByte & 1UL) << retBitIndex;
                cacheByte >>= 1;
                bitIndex++;
                retBitIndex++;
                if (bitIndex > 7)
                {
                    Read();
                    bitIndex = 0;
                    if (EndOfStream) return ret;
                }
            }
            return ret;
        }
    }
    class BitWriter
    {
        readonly MemoryStream stream;

        private int bitIndex;
        private byte cacheByte;

        public BitWriter()
        {
            stream = new MemoryStream();
        }
        /// <summary>
        /// 读取n个比特转换成ulong
        /// </summary>
        /// <param name="n">比特数，不超过64</param>
        /// <returns></returns>
        public void WriteULong(ulong index,int n)
        {
            if (n > 64) throw new NotSupportedException();
            while (n-- > 0)
            {
                cacheByte |= (byte)((index & 1L) << bitIndex);
                index >>= 1;
                bitIndex++;
                if (bitIndex > 7)
                {
                    stream.WriteByte(cacheByte);
                    cacheByte = 0;
                    bitIndex = 0;
                }
            }
        }
        public byte[] GetBytes()
        {
            if (bitIndex > 0)
            {
                stream.WriteByte(cacheByte);
                bitIndex = 0;
            }
            return stream.ToArray();
        }
    }
}
