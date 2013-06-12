namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    internal class QEncodedStream : DelegatedStream, IEncodableStream
    {
        private static byte[] hexDecodeMap = new byte[] { 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 10, 11, 12, 13, 14, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 10, 11, 12, 13, 14, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
         };
        private static byte[] hexEncodeMap = new byte[] { 0x30, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 70 };
        private int lineLength;
        private ReadStateInfo readState;
        private const int sizeOfEncodedChar = 3;
        private const int sizeOfEncodedCRLF = 6;
        private const int sizeOfFoldingCRLF = 5;
        private QuotedStringWriteStateInfo writeState;

        internal QEncodedStream(int lineLength)
        {
            this.lineLength = lineLength;
        }

        internal QEncodedStream(Stream stream) : this(stream, EncodedStreamFactory.DefaultMaxLineLength)
        {
        }

        internal QEncodedStream(QuotedStringWriteStateInfo wsi)
        {
            this.lineLength = EncodedStreamFactory.DefaultMaxLineLength;
            this.writeState = wsi;
        }

        internal QEncodedStream(Stream stream, int lineLength) : base(stream)
        {
            if (lineLength < 0)
            {
                throw new ArgumentOutOfRangeException("lineLength");
            }
            this.lineLength = lineLength;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            WriteAsyncResult result = new WriteAsyncResult(this, buffer, offset, count, callback, state);
            result.Write();
            return result;
        }

        public override void Close()
        {
            this.FlushInternal();
            base.Close();
        }

        public unsafe int DecodeBytes(byte[] buffer, int offset, int count)
        {
            try
            {
                fixed (byte* numRef = buffer)
                {
                    byte* numPtr = numRef + offset;
                    byte* numPtr2 = numPtr;
                    byte* numPtr3 = numPtr;
                    byte* numPtr4 = numPtr + count;
                    if (this.ReadState.IsEscaped)
                    {
                        if (this.ReadState.Byte == -1)
                        {
                            if (count == 1)
                            {
                                this.ReadState.Byte = numPtr2[0];
                                return 0;
                            }
                            if ((numPtr2[0] != 13) || (numPtr2[1] != 10))
                            {
                                byte num = hexDecodeMap[numPtr2[0]];
                                byte num2 = hexDecodeMap[numPtr2[1]];
                                if (num == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num }));
                                }
                                if (num2 == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num2 }));
                                }
                                numPtr3++;
                                numPtr3[0] = (byte) ((num << 4) + num2);
                            }
                            numPtr2 += 2;
                        }
                        else
                        {
                            if ((this.ReadState.Byte != 13) || (numPtr2[0] != 10))
                            {
                                byte num3 = hexDecodeMap[this.ReadState.Byte];
                                byte num4 = hexDecodeMap[numPtr2[0]];
                                if (num3 == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num3 }));
                                }
                                if (num4 == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num4 }));
                                }
                                numPtr3++;
                                numPtr3[0] = (byte) ((num3 << 4) + num4);
                            }
                            numPtr2++;
                        }
                        this.ReadState.IsEscaped = false;
                        this.ReadState.Byte = -1;
                    }
                    while (numPtr2 < numPtr4)
                    {
                        if (numPtr2[0] != 0x3d)
                        {
                            if (numPtr2[0] == 0x5f)
                            {
                                numPtr3++;
                                numPtr3[0] = 0x20;
                                numPtr2++;
                            }
                            else
                            {
                                numPtr3++;
                                numPtr2++;
                                numPtr3[0] = numPtr2[0];
                            }
                            continue;
                        }
                        long num8 = (long) ((numPtr4 - numPtr2) / 1);
                        if ((num8 <= 2L) && (num8 >= 1L))
                        {
                            switch (((int) (num8 - 1L)))
                            {
                                case 0:
                                    goto Label_022E;

                                case 1:
                                    this.ReadState.Byte = numPtr2[1];
                                    goto Label_022E;
                            }
                        }
                        goto Label_023F;
                    Label_022E:
                        this.ReadState.IsEscaped = true;
                        break;
                    Label_023F:
                        if ((numPtr2[1] != 13) || (numPtr2[2] != 10))
                        {
                            byte num5 = hexDecodeMap[numPtr2[1]];
                            byte num6 = hexDecodeMap[numPtr2[2]];
                            if (num5 == 0xff)
                            {
                                throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num5 }));
                            }
                            if (num6 == 0xff)
                            {
                                throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num6 }));
                            }
                            numPtr3++;
                            numPtr3[0] = (byte) ((num5 << 4) + num6);
                        }
                        numPtr2 += 3;
                    }
                    count = (int) ((long) ((numPtr3 - numPtr) / 1));
                }
            }
            finally
            {
                numRef = null;
            }
            return count;
        }

        public int EncodeBytes(byte[] buffer, int offset, int count)
        {
            this.writeState.CurrentLineLength += this.writeState.HeaderLength + this.writeState.MimeHeaderLength;
            this.writeState.AppendHeader();
            int index = offset;
            while (index < (count + offset))
            {
                if ((((this.lineLength != -1) && ((this.WriteState.CurrentLineLength + 5) >= this.lineLength)) && (((buffer[index] == 0x20) || (buffer[index] == 9)) || ((buffer[index] == 13) || (buffer[index] == 10)))) || ((this.WriteState.CurrentLineLength + this.writeState.FooterLength) >= this.lineLength))
                {
                    int num2;
                    int num3;
                    int num4;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < this.WriteState.FooterLength)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    this.WriteState.AppendFooter();
                    WriteStateInfoBase writeState = this.WriteState;
                    writeState.Length = (num2 = writeState.Length) + 1;
                    this.WriteState.Buffer[num2] = 13;
                    WriteStateInfoBase base2 = this.WriteState;
                    base2.Length = (num3 = base2.Length) + 1;
                    this.WriteState.Buffer[num3] = 10;
                    WriteStateInfoBase base3 = this.WriteState;
                    base3.Length = (num4 = base3.Length) + 1;
                    this.WriteState.Buffer[num4] = 0x20;
                    this.WriteState.AppendHeader();
                    this.WriteState.CurrentLineLength = this.WriteState.HeaderLength;
                }
                if ((this.WriteState.CurrentLineLength == 0) && (buffer[index] == 0x2e))
                {
                    int num5;
                    WriteStateInfoBase base4 = this.WriteState;
                    base4.Length = (num5 = base4.Length) + 1;
                    this.WriteState.Buffer[num5] = 0x2e;
                }
                if (((buffer[index] == 13) && ((index + 1) < (count + offset))) && (buffer[index + 1] == 10))
                {
                    int num6;
                    int num7;
                    int num8;
                    int num9;
                    int num10;
                    int num11;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 6)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    index++;
                    WriteStateInfoBase base5 = this.WriteState;
                    base5.Length = (num6 = base5.Length) + 1;
                    this.WriteState.Buffer[num6] = 0x3d;
                    WriteStateInfoBase base6 = this.WriteState;
                    base6.Length = (num7 = base6.Length) + 1;
                    this.WriteState.Buffer[num7] = 0x30;
                    WriteStateInfoBase base7 = this.WriteState;
                    base7.Length = (num8 = base7.Length) + 1;
                    this.WriteState.Buffer[num8] = 0x44;
                    WriteStateInfoBase base8 = this.WriteState;
                    base8.Length = (num9 = base8.Length) + 1;
                    this.WriteState.Buffer[num9] = 0x3d;
                    WriteStateInfoBase base9 = this.WriteState;
                    base9.Length = (num10 = base9.Length) + 1;
                    this.WriteState.Buffer[num10] = 0x30;
                    WriteStateInfoBase base10 = this.WriteState;
                    base10.Length = (num11 = base10.Length) + 1;
                    this.WriteState.Buffer[num11] = 0x41;
                    WriteStateInfoBase base11 = this.WriteState;
                    base11.CurrentLineLength += 6;
                }
                else if (((buffer[index] < 0x20) && (buffer[index] != 9)) || ((buffer[index] == 0x3d) || (buffer[index] > 0x7e)))
                {
                    int num12;
                    int num13;
                    int num14;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 3)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    WriteStateInfoBase base12 = this.WriteState;
                    base12.CurrentLineLength += 3;
                    WriteStateInfoBase base13 = this.WriteState;
                    base13.Length = (num12 = base13.Length) + 1;
                    this.WriteState.Buffer[num12] = 0x3d;
                    WriteStateInfoBase base14 = this.WriteState;
                    base14.Length = (num13 = base14.Length) + 1;
                    this.WriteState.Buffer[num13] = hexEncodeMap[buffer[index] >> 4];
                    WriteStateInfoBase base15 = this.WriteState;
                    base15.Length = (num14 = base15.Length) + 1;
                    this.WriteState.Buffer[num14] = hexEncodeMap[buffer[index] & 15];
                }
                else if (buffer[index] == 0x20)
                {
                    int num15;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 1)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    WriteStateInfoBase base16 = this.WriteState;
                    base16.CurrentLineLength++;
                    WriteStateInfoBase base17 = this.WriteState;
                    base17.Length = (num15 = base17.Length) + 1;
                    this.WriteState.Buffer[num15] = 0x5f;
                }
                else
                {
                    int num16;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 1)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    WriteStateInfoBase base18 = this.WriteState;
                    base18.CurrentLineLength++;
                    WriteStateInfoBase base19 = this.WriteState;
                    base19.Length = (num16 = base19.Length) + 1;
                    this.WriteState.Buffer[num16] = buffer[index];
                }
                index++;
            }
            this.WriteState.AppendFooter();
            return (index - offset);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            WriteAsyncResult.End(asyncResult);
        }

        public override void Flush()
        {
            this.FlushInternal();
            base.Flush();
        }

        private void FlushInternal()
        {
            if ((this.writeState != null) && (this.writeState.Length > 0))
            {
                base.Write(this.WriteState.Buffer, 0, this.WriteState.Length);
                this.WriteState.Length = 0;
            }
        }

        public string GetEncodedString()
        {
            return Encoding.ASCII.GetString(this.WriteState.Buffer, 0, this.WriteState.Length);
        }

        public Stream GetStream()
        {
            return this;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            int num = 0;
            while (true)
            {
                num += this.EncodeBytes(buffer, offset + num, count - num);
                if (num >= count)
                {
                    break;
                }
                this.FlushInternal();
            }
        }

        private ReadStateInfo ReadState
        {
            get
            {
                if (this.readState == null)
                {
                    this.readState = new ReadStateInfo();
                }
                return this.readState;
            }
        }

        internal WriteStateInfoBase WriteState
        {
            get
            {
                if (this.writeState == null)
                {
                    this.writeState = new QuotedStringWriteStateInfo(0x400, null, null, 0x4c);
                }
                return this.writeState;
            }
        }

        private class ReadStateInfo
        {
            private short b1 = -1;
            private bool isEscaped;

            internal short Byte
            {
                get
                {
                    return this.b1;
                }
                set
                {
                    this.b1 = value;
                }
            }

            internal bool IsEscaped
            {
                get
                {
                    return this.isEscaped;
                }
                set
                {
                    this.isEscaped = value;
                }
            }
        }

        private class WriteAsyncResult : LazyAsyncResult
        {
            private byte[] buffer;
            private int count;
            private int offset;
            private static AsyncCallback onWrite = new AsyncCallback(QEncodedStream.WriteAsyncResult.OnWrite);
            private QEncodedStream parent;
            private int written;

            internal WriteAsyncResult(QEncodedStream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
            }

            private void CompleteWrite(IAsyncResult result)
            {
                this.parent.BaseStream.EndWrite(result);
                this.parent.WriteState.Length = 0;
            }

            internal static void End(IAsyncResult result)
            {
                ((QEncodedStream.WriteAsyncResult) result).InternalWaitForCompletion();
            }

            private static void OnWrite(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    QEncodedStream.WriteAsyncResult asyncState = (QEncodedStream.WriteAsyncResult) result.AsyncState;
                    try
                    {
                        asyncState.CompleteWrite(result);
                        asyncState.Write();
                    }
                    catch (Exception exception)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            internal void Write()
            {
                while (true)
                {
                    this.written += this.parent.EncodeBytes(this.buffer, this.offset + this.written, this.count - this.written);
                    if (this.written >= this.count)
                    {
                        break;
                    }
                    IAsyncResult result = this.parent.BaseStream.BeginWrite(this.parent.WriteState.Buffer, 0, this.parent.WriteState.Length, onWrite, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.CompleteWrite(result);
                }
                base.InvokeCallback();
            }
        }
    }
}

