namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Text;

    internal class Base64Stream : DelegatedStream, IEncodableStream
    {
        private static byte[] base64DecodeMap = new byte[] { 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x3e, 0xff, 0xff, 0xff, 0x3f, 
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 60, 0x3d, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 
            15, 0x10, 0x11, 0x12, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0x1a, 0x1b, 0x1c, 0x1d, 30, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 40, 
            0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 50, 0x33, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
         };
        private static byte[] base64EncodeMap = new byte[] { 
            0x41, 0x42, 0x43, 0x44, 0x45, 70, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 80, 
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 90, 0x61, 0x62, 0x63, 100, 0x65, 0x66, 
            0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 110, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 
            0x77, 120, 0x79, 0x7a, 0x30, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x2b, 0x2f, 
            0x3d
         };
        private const byte invalidBase64Value = 0xff;
        private int lineLength;
        private ReadStateInfo readState;
        private const int sizeOfBase64EncodedChar = 4;
        private const int sizeOfSoftCRLF = 3;
        private Base64WriteStateInfo writeState;

        internal Base64Stream(Base64WriteStateInfo writeStateInfo)
        {
            this.lineLength = writeStateInfo.MaxLineLength;
            this.writeState = writeStateInfo;
        }

        internal Base64Stream(Stream stream, int lineLength) : base(stream)
        {
            this.lineLength = lineLength;
            this.writeState = new Base64WriteStateInfo();
        }

        internal Base64Stream(Stream stream, Base64WriteStateInfo writeStateInfo) : base(stream)
        {
            this.writeState = new Base64WriteStateInfo();
            this.lineLength = writeStateInfo.MaxLineLength;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
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
            ReadAsyncResult result = new ReadAsyncResult(this, buffer, offset, count, callback, state);
            result.Read();
            return result;
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
            if ((this.writeState != null) && (this.WriteState.Length > 0))
            {
                switch (this.WriteState.Padding)
                {
                    case 1:
                    {
                        int num5;
                        int num6;
                        Base64WriteStateInfo writeState = this.WriteState;
                        writeState.Length = (num5 = writeState.Length) + 1;
                        this.WriteState.Buffer[num5] = base64EncodeMap[this.WriteState.LastBits];
                        Base64WriteStateInfo info5 = this.WriteState;
                        info5.Length = (num6 = info5.Length) + 1;
                        this.WriteState.Buffer[num6] = base64EncodeMap[0x40];
                        break;
                    }
                    case 2:
                    {
                        int num2;
                        int num3;
                        int num4;
                        Base64WriteStateInfo info1 = this.WriteState;
                        info1.Length = (num2 = info1.Length) + 1;
                        this.WriteState.Buffer[num2] = base64EncodeMap[this.WriteState.LastBits];
                        Base64WriteStateInfo info2 = this.WriteState;
                        info2.Length = (num3 = info2.Length) + 1;
                        this.WriteState.Buffer[num3] = base64EncodeMap[0x40];
                        Base64WriteStateInfo info3 = this.WriteState;
                        info3.Length = (num4 = info3.Length) + 1;
                        this.WriteState.Buffer[num4] = base64EncodeMap[0x40];
                        break;
                    }
                }
                this.WriteState.Padding = 0;
                this.FlushInternal();
            }
            base.Close();
        }

        public unsafe int DecodeBytes(byte[] buffer, int offset, int count)
        {
            fixed (byte* numRef = buffer)
            {
                byte* numPtr = numRef + offset;
                byte* numPtr2 = numPtr;
                byte* numPtr3 = numPtr;
                byte* numPtr4 = numPtr + count;
                while (numPtr2 < numPtr4)
                {
                    if (((numPtr2[0] == 13) || (numPtr2[0] == 10)) || (((numPtr2[0] == 0x3d) || (numPtr2[0] == 0x20)) || (numPtr2[0] == 9)))
                    {
                        numPtr2++;
                        continue;
                    }
                    byte num = base64DecodeMap[numPtr2[0]];
                    if (num == 0xff)
                    {
                        throw new FormatException(SR.GetString("MailBase64InvalidCharacter"));
                    }
                    switch (this.ReadState.Pos)
                    {
                        case 0:
                        {
                            this.ReadState.Val = (byte) (num << 2);
                            ReadStateInfo readState = this.ReadState;
                            readState.Pos = (byte) (readState.Pos + 1);
                            break;
                        }
                        case 1:
                        {
                            numPtr3++;
                            numPtr3[0] = (byte) (this.ReadState.Val + (num >> 4));
                            this.ReadState.Val = (byte) (num << 4);
                            ReadStateInfo info2 = this.ReadState;
                            info2.Pos = (byte) (info2.Pos + 1);
                            break;
                        }
                        case 2:
                        {
                            numPtr3++;
                            numPtr3[0] = (byte) (this.ReadState.Val + (num >> 2));
                            this.ReadState.Val = (byte) (num << 6);
                            ReadStateInfo info3 = this.ReadState;
                            info3.Pos = (byte) (info3.Pos + 1);
                            break;
                        }
                        case 3:
                            numPtr3++;
                            numPtr3[0] = (byte) (this.ReadState.Val + num);
                            this.ReadState.Pos = 0;
                            break;
                    }
                    numPtr2++;
                }
                count = (int) ((long) ((numPtr3 - numPtr) / 1));
            }
            return count;
        }

        public int EncodeBytes(byte[] buffer, int offset, int count)
        {
            return this.EncodeBytes(buffer, offset, count, true, true);
        }

        internal int EncodeBytes(byte[] buffer, int offset, int count, bool dontDeferFinalBytes, bool shouldAppendSpaceToCRLF)
        {
            bool flag = this.writeState.HeaderLength != 0;
            int index = offset;
            this.WriteState.AppendHeader();
            Base64WriteStateInfo writeState = this.WriteState;
            writeState.CurrentLineLength += this.writeState.MimeHeaderLength;
            switch (this.WriteState.Padding)
            {
                case 1:
                {
                    int num7;
                    int num8;
                    Base64WriteStateInfo info6 = this.WriteState;
                    info6.Length = (num7 = info6.Length) + 1;
                    this.WriteState.Buffer[num7] = base64EncodeMap[this.WriteState.LastBits | ((buffer[index] & 0xc0) >> 6)];
                    Base64WriteStateInfo info7 = this.WriteState;
                    info7.Length = (num8 = info7.Length) + 1;
                    this.WriteState.Buffer[num8] = base64EncodeMap[buffer[index] & 0x3f];
                    index++;
                    count--;
                    this.WriteState.Padding = 0;
                    Base64WriteStateInfo info8 = this.WriteState;
                    info8.CurrentLineLength++;
                    break;
                }
                case 2:
                {
                    int num4;
                    Base64WriteStateInfo info2 = this.WriteState;
                    info2.Length = (num4 = info2.Length) + 1;
                    this.WriteState.Buffer[num4] = base64EncodeMap[this.WriteState.LastBits | ((buffer[index] & 240) >> 4)];
                    if (count != 1)
                    {
                        int num5;
                        int num6;
                        Base64WriteStateInfo info3 = this.WriteState;
                        info3.Length = (num5 = info3.Length) + 1;
                        this.WriteState.Buffer[num5] = base64EncodeMap[((buffer[index] & 15) << 2) | ((buffer[index + 1] & 0xc0) >> 6)];
                        Base64WriteStateInfo info4 = this.WriteState;
                        info4.Length = (num6 = info4.Length) + 1;
                        this.WriteState.Buffer[num6] = base64EncodeMap[buffer[index + 1] & 0x3f];
                        index += 2;
                        count -= 2;
                        this.WriteState.Padding = 0;
                        Base64WriteStateInfo info5 = this.WriteState;
                        info5.CurrentLineLength += 2;
                        break;
                    }
                    this.WriteState.LastBits = (byte) ((buffer[index] & 15) << 2);
                    this.WriteState.Padding = 1;
                    return (index - offset);
                }
            }
            int num2 = index + (count - (count % 3));
            while (index < num2)
            {
                int num12;
                int num13;
                int num14;
                int num15;
                if (((this.lineLength != -1) && ((this.WriteState.CurrentLineLength + 4) > (this.lineLength - 3))) && !flag)
                {
                    int num9;
                    int num10;
                    if ((this.WriteState.Length + 3) >= this.WriteState.Buffer.Length)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    Base64WriteStateInfo info9 = this.WriteState;
                    info9.Length = (num9 = info9.Length) + 1;
                    this.WriteState.Buffer[num9] = 13;
                    Base64WriteStateInfo info10 = this.WriteState;
                    info10.Length = (num10 = info10.Length) + 1;
                    this.WriteState.Buffer[num10] = 10;
                    if (shouldAppendSpaceToCRLF)
                    {
                        int num11;
                        Base64WriteStateInfo info11 = this.WriteState;
                        info11.Length = (num11 = info11.Length) + 1;
                        this.WriteState.Buffer[num11] = 0x20;
                    }
                    this.WriteState.CurrentLineLength = 0;
                }
                if ((this.WriteState.Length + 4) > this.WriteState.Buffer.Length)
                {
                    this.WriteState.ResizeBuffer();
                }
                Base64WriteStateInfo info12 = this.WriteState;
                info12.Length = (num12 = info12.Length) + 1;
                this.WriteState.Buffer[num12] = base64EncodeMap[(buffer[index] & 0xfc) >> 2];
                Base64WriteStateInfo info13 = this.WriteState;
                info13.Length = (num13 = info13.Length) + 1;
                this.WriteState.Buffer[num13] = base64EncodeMap[((buffer[index] & 3) << 4) | ((buffer[index + 1] & 240) >> 4)];
                Base64WriteStateInfo info14 = this.WriteState;
                info14.Length = (num14 = info14.Length) + 1;
                this.WriteState.Buffer[num14] = base64EncodeMap[((buffer[index + 1] & 15) << 2) | ((buffer[index + 2] & 0xc0) >> 6)];
                Base64WriteStateInfo info15 = this.WriteState;
                info15.Length = (num15 = info15.Length) + 1;
                this.WriteState.Buffer[num15] = base64EncodeMap[buffer[index + 2] & 0x3f];
                Base64WriteStateInfo info16 = this.WriteState;
                info16.CurrentLineLength += 4;
                if ((((this.WriteState.CurrentLineLength + 4) + this.writeState.FooterLength) >= EncodedStreamFactory.DefaultMaxLineLength) && flag)
                {
                    int num16;
                    int num17;
                    if (((this.WriteState.Length + this.writeState.FooterLength) + this.writeState.HeaderLength) > this.WriteState.Buffer.Length)
                    {
                        this.WriteState.ResizeBuffer();
                    }
                    this.WriteState.AppendFooter();
                    Base64WriteStateInfo info17 = this.WriteState;
                    info17.Length = (num16 = info17.Length) + 1;
                    this.writeState.Buffer[num16] = 13;
                    Base64WriteStateInfo info18 = this.WriteState;
                    info18.Length = (num17 = info18.Length) + 1;
                    this.writeState.Buffer[num17] = 10;
                    if (shouldAppendSpaceToCRLF)
                    {
                        int num18;
                        Base64WriteStateInfo info19 = this.WriteState;
                        info19.Length = (num18 = info19.Length) + 1;
                        this.WriteState.Buffer[num18] = 0x20;
                    }
                    this.WriteState.AppendHeader();
                    this.WriteState.CurrentLineLength = this.WriteState.HeaderLength + 1;
                }
                index += 3;
            }
            index = num2;
            if ((this.WriteState.Length + 4) > this.WriteState.Buffer.Length)
            {
                this.WriteState.ResizeBuffer();
            }
            switch ((count % 3))
            {
                case 1:
                {
                    int num24;
                    Base64WriteStateInfo info26 = this.WriteState;
                    info26.Length = (num24 = info26.Length) + 1;
                    this.WriteState.Buffer[num24] = base64EncodeMap[(buffer[index] & 0xfc) >> 2];
                    if (!dontDeferFinalBytes)
                    {
                        this.WriteState.LastBits = (byte) ((buffer[index] & 3) << 4);
                        this.WriteState.Padding = 2;
                        Base64WriteStateInfo info31 = this.WriteState;
                        info31.CurrentLineLength++;
                    }
                    else
                    {
                        int num25;
                        int num26;
                        int num27;
                        Base64WriteStateInfo info27 = this.WriteState;
                        info27.Length = (num25 = info27.Length) + 1;
                        this.WriteState.Buffer[num25] = base64EncodeMap[(byte) ((buffer[index] & 3) << 4)];
                        Base64WriteStateInfo info28 = this.WriteState;
                        info28.Length = (num26 = info28.Length) + 1;
                        this.WriteState.Buffer[num26] = base64EncodeMap[0x40];
                        Base64WriteStateInfo info29 = this.WriteState;
                        info29.Length = (num27 = info29.Length) + 1;
                        this.WriteState.Buffer[num27] = base64EncodeMap[0x40];
                        this.WriteState.Padding = 0;
                        Base64WriteStateInfo info30 = this.WriteState;
                        info30.CurrentLineLength += 4;
                    }
                    index++;
                    goto Label_07B2;
                }
                case 2:
                {
                    int num20;
                    int num21;
                    int num22;
                    int num23;
                    Base64WriteStateInfo info20 = this.WriteState;
                    info20.Length = (num20 = info20.Length) + 1;
                    this.WriteState.Buffer[num20] = base64EncodeMap[(buffer[index] & 0xfc) >> 2];
                    Base64WriteStateInfo info21 = this.WriteState;
                    info21.Length = (num21 = info21.Length) + 1;
                    this.WriteState.Buffer[num21] = base64EncodeMap[((buffer[index] & 3) << 4) | ((buffer[index + 1] & 240) >> 4)];
                    if (!dontDeferFinalBytes)
                    {
                        this.WriteState.LastBits = (byte) ((buffer[index + 1] & 15) << 2);
                        this.WriteState.Padding = 1;
                        Base64WriteStateInfo info25 = this.WriteState;
                        info25.CurrentLineLength += 2;
                        break;
                    }
                    Base64WriteStateInfo info22 = this.WriteState;
                    info22.Length = (num22 = info22.Length) + 1;
                    this.WriteState.Buffer[num22] = base64EncodeMap[(buffer[index + 1] & 15) << 2];
                    Base64WriteStateInfo info23 = this.WriteState;
                    info23.Length = (num23 = info23.Length) + 1;
                    this.WriteState.Buffer[num23] = base64EncodeMap[0x40];
                    this.WriteState.Padding = 0;
                    Base64WriteStateInfo info24 = this.WriteState;
                    info24.CurrentLineLength += 4;
                    break;
                }
                default:
                    goto Label_07B2;
            }
            index += 2;
        Label_07B2:
            this.WriteState.AppendFooter();
            return (index - offset);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            return ReadAsyncResult.End(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            WriteAsyncResult.End(asyncResult);
        }

        public override void Flush()
        {
            if ((this.writeState != null) && (this.WriteState.Length > 0))
            {
                this.FlushInternal();
            }
            base.Flush();
        }

        private void FlushInternal()
        {
            base.Write(this.WriteState.Buffer, 0, this.WriteState.Length);
            this.WriteState.Length = 0;
        }

        public string GetEncodedString()
        {
            return Encoding.ASCII.GetString(this.WriteState.Buffer, 0, this.WriteState.Length);
        }

        public Stream GetStream()
        {
            return this;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num;
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
            do
            {
                num = base.Read(buffer, offset, count);
                if (num == 0)
                {
                    return 0;
                }
                num = this.DecodeBytes(buffer, offset, num);
            }
            while (num <= 0);
            return num;
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
                num += this.EncodeBytes(buffer, offset + num, count - num, false, false);
                if (num >= count)
                {
                    break;
                }
                this.FlushInternal();
            }
        }

        public override bool CanWrite
        {
            get
            {
                return base.CanWrite;
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

        internal Base64WriteStateInfo WriteState
        {
            get
            {
                return this.writeState;
            }
        }

        private class ReadAsyncResult : LazyAsyncResult
        {
            private byte[] buffer;
            private int count;
            private int offset;
            private static AsyncCallback onRead = new AsyncCallback(Base64Stream.ReadAsyncResult.OnRead);
            private Base64Stream parent;
            private int read;

            internal ReadAsyncResult(Base64Stream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
            }

            private bool CompleteRead(IAsyncResult result)
            {
                this.read = this.parent.BaseStream.EndRead(result);
                if (this.read == 0)
                {
                    base.InvokeCallback();
                    return true;
                }
                this.read = this.parent.DecodeBytes(this.buffer, this.offset, this.read);
                if (this.read > 0)
                {
                    base.InvokeCallback();
                    return true;
                }
                return false;
            }

            internal static int End(IAsyncResult result)
            {
                Base64Stream.ReadAsyncResult result2 = (Base64Stream.ReadAsyncResult) result;
                result2.InternalWaitForCompletion();
                return result2.read;
            }

            private static void OnRead(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Base64Stream.ReadAsyncResult asyncState = (Base64Stream.ReadAsyncResult) result.AsyncState;
                    try
                    {
                        if (!asyncState.CompleteRead(result))
                        {
                            asyncState.Read();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (asyncState.IsCompleted)
                        {
                            throw;
                        }
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            internal void Read()
            {
                IAsyncResult result;
                do
                {
                    result = this.parent.BaseStream.BeginRead(this.buffer, this.offset, this.count, onRead, this);
                }
                while (result.CompletedSynchronously && !this.CompleteRead(result));
            }
        }

        private class ReadStateInfo
        {
            private byte pos;
            private byte val;

            internal byte Pos
            {
                get
                {
                    return this.pos;
                }
                set
                {
                    this.pos = value;
                }
            }

            internal byte Val
            {
                get
                {
                    return this.val;
                }
                set
                {
                    this.val = value;
                }
            }
        }

        private class WriteAsyncResult : LazyAsyncResult
        {
            private byte[] buffer;
            private int count;
            private int offset;
            private static AsyncCallback onWrite = new AsyncCallback(Base64Stream.WriteAsyncResult.OnWrite);
            private Base64Stream parent;
            private int written;

            internal WriteAsyncResult(Base64Stream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(null, state, callback)
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
                ((Base64Stream.WriteAsyncResult) result).InternalWaitForCompletion();
            }

            private static void OnWrite(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Base64Stream.WriteAsyncResult asyncState = (Base64Stream.WriteAsyncResult) result.AsyncState;
                    try
                    {
                        asyncState.CompleteWrite(result);
                        asyncState.Write();
                    }
                    catch (Exception exception)
                    {
                        if (asyncState.IsCompleted)
                        {
                            throw;
                        }
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            internal void Write()
            {
                while (true)
                {
                    this.written += this.parent.EncodeBytes(this.buffer, this.offset + this.written, this.count - this.written, false, false);
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

