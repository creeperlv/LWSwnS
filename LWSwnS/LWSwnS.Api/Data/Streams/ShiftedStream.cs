﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWSwnS.Api.Data.Streams
{
    /// <summary>
    /// This stream transforms the origin stream. This will shifts all bytes with a given distance 'Shift'.
    /// </summary>
    public class ShiftedStream : Stream
    {
        public override bool CanRead => OriginalStream.CanRead;

        public override bool CanSeek => OriginalStream.CanSeek;

        public override bool CanWrite => OriginalStream.CanWrite;

        public override long Length => OriginalStream.Length;

        public override long Position { get => OriginalStream.Position; set => OriginalStream.Position = value; }

        public bool isEnd { get; private set; } = false;

        public Stream OriginalStream;
        public int Shift;

        public ShiftedStream(Stream OriginalStream, int shift)
        {
            this.Shift = shift;
            if (shift <= 0 || shift > 256)
            {
                throw new Exception("Shift value is beyond boundary!");
            }
            this.OriginalStream = OriginalStream;
        }

        public override void Flush()
        {
            OriginalStream.Flush();
        }
        public void WriteEnd()
        {
            this.OriginalStream.WriteByte(0);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] buf = new byte[buffer.Length * 2];
            int p = OriginalStream.Read(buf, offset, buf.Length);
            int d = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                d += buf[i];
                if (i % 2 == 0 && buf[i] == 0)
                {
                    isEnd = true;
                }
                if (i % 2 == 1)
                {
                    d -= Shift;
                    buffer[i / 2] = (byte)d;
                    d = 0;
                }
            }
            return p/2;
        }
        public override int ReadByte()
        {
            int d = 0;
            d += OriginalStream.ReadByte();
            if (d == 0)
            {
                isEnd = true;
                return 0;
            }
            d += OriginalStream.ReadByte();
            return d-Shift;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return OriginalStream.Seek(offset * 2, origin);
        }

        public override void SetLength(long value)
        {
            OriginalStream.SetLength(value * 2);
        }
        public void WriteAndFlush(byte[] buffer, int offset, int count)
        {
            Write(buffer, offset, count);
            Flush();
        }
        public override void WriteByte(byte value)
        {
            int d = value + Shift;
            WriteByte(d > 255 ? (byte)255 : (byte)d);
            WriteByte(d > 255 ? (byte)(d - 255) : (byte)0);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] buf = new byte[buffer.Length * 2];
            int d = 0;
            for (int i = 0; i <count; i++)
            {
                d = buffer[i] + Shift;
                buf[i * 2] = d > 255 ? (byte)255 : (byte)d;
                buf[i * 2 + 1] = d > 255 ? (byte)(d - 255) : (byte)0;
            }
            OriginalStream.Write(buf, offset, count * 2);
        }
    }
}
