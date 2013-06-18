namespace System.Messaging
{
    using System;
    using System.IO;
    using System.Messaging.Interop;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ActiveXMessageFormatter : IMessageFormatter, ICloneable
    {
        private ASCIIEncoding asciiEncoding;
        private byte[] internalBuffer;
        private char[] internalCharBuffer;
        private UnicodeEncoding unicodeEncoding;
        internal const short VT_ARRAY = 0x2000;
        internal const short VT_BOOL = 11;
        internal const short VT_BSTR = 8;
        internal const short VT_CLSID = 0x48;
        internal const short VT_CY = 6;
        internal const short VT_DATE = 7;
        internal const short VT_I1 = 0x10;
        internal const short VT_I2 = 2;
        internal const short VT_I4 = 3;
        internal const short VT_I8 = 20;
        internal const short VT_LPSTR = 30;
        internal const short VT_LPWSTR = 0x1f;
        internal const short VT_NULL = 1;
        internal const short VT_R4 = 4;
        internal const short VT_R8 = 5;
        internal const short VT_STORED_OBJECT = 0x45;
        internal const short VT_STREAMED_OBJECT = 0x44;
        internal const short VT_UI1 = 0x11;
        internal const short VT_UI2 = 0x12;
        internal const short VT_UI4 = 0x13;
        internal const short VT_UI8 = 0x15;
        internal const short VT_VECTOR = 0x1000;

        public bool CanRead(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            int bodyType = message.BodyType;
            if ((((((bodyType != 11) && (bodyType != 0x48)) && ((bodyType != 6) && (bodyType != 7))) && (((bodyType != 0x10) && (bodyType != 0x11)) && ((bodyType != 2) && (bodyType != 0x12)))) && ((((bodyType != 3) && (bodyType != 0x13)) && ((bodyType != 20) && (bodyType != 0x15))) && (((bodyType != 1) && (bodyType != 4)) && ((bodyType != 20) && (bodyType != 0x44))))) && ((((bodyType != 0x45) && (bodyType != 0x1011)) && ((bodyType != 30) && (bodyType != 0x1f))) && ((bodyType != 8) && (bodyType != 5))))
            {
                return false;
            }
            return true;
        }

        public object Clone()
        {
            return new ActiveXMessageFormatter();
        }

        public static void InitStreamedObject(object streamedObject)
        {
            IPersistStreamInit init = streamedObject as IPersistStreamInit;
            if (init != null)
            {
                init.InitNew();
            }
        }

        public object Read(Message message)
        {
            Stream bodyStream;
            byte[] buffer;
            byte[] buffer2;
            int num;
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            switch (message.BodyType)
            {
                case 1:
                    return null;

                case 2:
                    bodyStream = message.BodyStream;
                    buffer = new byte[2];
                    bodyStream.Read(buffer, 0, 2);
                    return BitConverter.ToInt16(buffer, 0);

                case 3:
                    bodyStream = message.BodyStream;
                    buffer = new byte[4];
                    bodyStream.Read(buffer, 0, 4);
                    return BitConverter.ToInt32(buffer, 0);

                case 4:
                    bodyStream = message.BodyStream;
                    buffer = new byte[4];
                    bodyStream.Read(buffer, 0, 4);
                    return BitConverter.ToSingle(buffer, 0);

                case 5:
                    bodyStream = message.BodyStream;
                    buffer = new byte[8];
                    bodyStream.Read(buffer, 0, 8);
                    return BitConverter.ToDouble(buffer, 0);

                case 6:
                    buffer = message.properties.GetUI1Vector(9);
                    buffer2 = new byte[8];
                    Array.Copy(buffer, buffer2, 8);
                    return decimal.FromOACurrency(BitConverter.ToInt64(buffer2, 0));

                case 7:
                    buffer = message.properties.GetUI1Vector(9);
                    buffer2 = new byte[8];
                    Array.Copy(buffer, buffer2, 8);
                    return new DateTime(BitConverter.ToInt64(buffer2, 0));

                case 8:
                case 0x1f:
                    buffer = message.properties.GetUI1Vector(9);
                    num = message.properties.GetUI4(10) / 2;
                    if ((this.internalCharBuffer == null) || (this.internalCharBuffer.Length < num))
                    {
                        this.internalCharBuffer = new char[num];
                    }
                    if (this.unicodeEncoding == null)
                    {
                        this.unicodeEncoding = new UnicodeEncoding();
                    }
                    this.unicodeEncoding.GetChars(buffer, 0, num * 2, this.internalCharBuffer, 0);
                    return new string(this.internalCharBuffer, 0, num);

                case 11:
                    buffer = message.properties.GetUI1Vector(9);
                    buffer2 = new byte[1];
                    Array.Copy(buffer, buffer2, 1);
                    if (buffer[0] == 0)
                    {
                        return false;
                    }
                    return true;

                case 0x10:
                case 0x11:
                    bodyStream = message.BodyStream;
                    buffer = new byte[1];
                    bodyStream.Read(buffer, 0, 1);
                    return buffer[0];

                case 0x12:
                    bodyStream = message.BodyStream;
                    buffer = new byte[2];
                    bodyStream.Read(buffer, 0, 2);
                    return BitConverter.ToUInt16(buffer, 0);

                case 0x13:
                    bodyStream = message.BodyStream;
                    buffer = new byte[4];
                    bodyStream.Read(buffer, 0, 4);
                    return BitConverter.ToUInt32(buffer, 0);

                case 20:
                    bodyStream = message.BodyStream;
                    buffer = new byte[8];
                    bodyStream.Read(buffer, 0, 8);
                    return BitConverter.ToInt64(buffer, 0);

                case 0x15:
                    bodyStream = message.BodyStream;
                    buffer = new byte[8];
                    bodyStream.Read(buffer, 0, 8);
                    return BitConverter.ToUInt64(buffer, 0);

                case 30:
                    buffer = message.properties.GetUI1Vector(9);
                    num = message.properties.GetUI4(10);
                    if ((this.internalCharBuffer == null) || (this.internalCharBuffer.Length < num))
                    {
                        this.internalCharBuffer = new char[num];
                    }
                    if (this.asciiEncoding == null)
                    {
                        this.asciiEncoding = new ASCIIEncoding();
                    }
                    this.asciiEncoding.GetChars(buffer, 0, num, this.internalCharBuffer, 0);
                    return new string(this.internalCharBuffer, 0, num);

                case 0x44:
                {
                    ComStreamFromDataStream stream = new ComStreamFromDataStream(message.BodyStream);
                    return System.Messaging.Interop.NativeMethods.OleLoadFromStream(stream, ref System.Messaging.Interop.NativeMethods.IID_IUnknown);
                }
                case 0x45:
                    throw new NotSupportedException(Res.GetString("StoredObjectsNotSupported"));

                case 0x48:
                    buffer = message.properties.GetUI1Vector(9);
                    buffer2 = new byte[0x10];
                    Array.Copy(buffer, buffer2, 0x10);
                    return new Guid(buffer2);

                case 0x1011:
                    buffer = message.properties.GetUI1Vector(9);
                    num = message.properties.GetUI4(10);
                    buffer2 = new byte[num];
                    Array.Copy(buffer, buffer2, num);
                    return buffer2;
            }
            throw new InvalidOperationException(Res.GetString("InvalidTypeDeserialization"));
        }

        public void Write(Message message, object obj)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (obj is string)
            {
                int size = ((string) obj).Length * 2;
                if ((this.internalBuffer == null) || (this.internalBuffer.Length < size))
                {
                    this.internalBuffer = new byte[size];
                }
                if (this.unicodeEncoding == null)
                {
                    this.unicodeEncoding = new UnicodeEncoding();
                }
                this.unicodeEncoding.GetBytes(((string) obj).ToCharArray(), 0, size / 2, this.internalBuffer, 0);
                message.properties.SetUI1Vector(9, this.internalBuffer);
                message.properties.AdjustSize(9, size);
                message.properties.SetUI4(10, size);
                message.properties.SetUI4(0x2a, 0x1f);
            }
            else if (obj is byte[])
            {
                byte[] sourceArray = (byte[]) obj;
                if ((this.internalBuffer == null) || (this.internalBuffer.Length < sourceArray.Length))
                {
                    this.internalBuffer = new byte[sourceArray.Length];
                }
                Array.Copy(sourceArray, this.internalBuffer, sourceArray.Length);
                message.properties.SetUI1Vector(9, this.internalBuffer);
                message.properties.AdjustSize(9, sourceArray.Length);
                message.properties.SetUI4(10, sourceArray.Length);
                message.properties.SetUI4(0x2a, 0x1011);
            }
            else if (obj is char[])
            {
                char[] chars = (char[]) obj;
                int num3 = chars.Length * 2;
                if ((this.internalBuffer == null) || (this.internalBuffer.Length < num3))
                {
                    this.internalBuffer = new byte[num3];
                }
                if (this.unicodeEncoding == null)
                {
                    this.unicodeEncoding = new UnicodeEncoding();
                }
                this.unicodeEncoding.GetBytes(chars, 0, num3 / 2, this.internalBuffer, 0);
                message.properties.SetUI1Vector(9, this.internalBuffer);
                message.properties.SetUI4(10, num3);
                message.properties.SetUI4(0x2a, 0x1f);
            }
            else
            {
                Stream dataStream;
                int num;
                if (obj is byte)
                {
                    dataStream = new MemoryStream(1);
                    dataStream.Write(new byte[] { (byte) obj }, 0, 1);
                    num = 0x11;
                }
                else if (obj is bool)
                {
                    dataStream = new MemoryStream(1);
                    if ((bool) obj)
                    {
                        dataStream.Write(new byte[] { 0xff }, 0, 1);
                    }
                    else
                    {
                        byte[] buffer = new byte[1];
                        dataStream.Write(buffer, 0, 1);
                    }
                    num = 11;
                }
                else if (obj is char)
                {
                    dataStream = new MemoryStream(2);
                    byte[] bytes = BitConverter.GetBytes((char) obj);
                    dataStream.Write(bytes, 0, 2);
                    num = 0x12;
                }
                else if (obj is decimal)
                {
                    dataStream = new MemoryStream(8);
                    byte[] buffer3 = BitConverter.GetBytes(decimal.ToOACurrency((decimal) obj));
                    dataStream.Write(buffer3, 0, 8);
                    num = 6;
                }
                else if (obj is DateTime)
                {
                    dataStream = new MemoryStream(8);
                    DateTime time = (DateTime) obj;
                    byte[] buffer4 = BitConverter.GetBytes(time.Ticks);
                    dataStream.Write(buffer4, 0, 8);
                    num = 7;
                }
                else if (obj is double)
                {
                    dataStream = new MemoryStream(8);
                    byte[] buffer5 = BitConverter.GetBytes((double) obj);
                    dataStream.Write(buffer5, 0, 8);
                    num = 5;
                }
                else if (obj is short)
                {
                    dataStream = new MemoryStream(2);
                    byte[] buffer6 = BitConverter.GetBytes((short) obj);
                    dataStream.Write(buffer6, 0, 2);
                    num = 2;
                }
                else if (obj is ushort)
                {
                    dataStream = new MemoryStream(2);
                    byte[] buffer7 = BitConverter.GetBytes((ushort) obj);
                    dataStream.Write(buffer7, 0, 2);
                    num = 0x12;
                }
                else if (obj is int)
                {
                    dataStream = new MemoryStream(4);
                    byte[] buffer8 = BitConverter.GetBytes((int) obj);
                    dataStream.Write(buffer8, 0, 4);
                    num = 3;
                }
                else if (obj is uint)
                {
                    dataStream = new MemoryStream(4);
                    byte[] buffer9 = BitConverter.GetBytes((uint) obj);
                    dataStream.Write(buffer9, 0, 4);
                    num = 0x13;
                }
                else if (obj is long)
                {
                    dataStream = new MemoryStream(8);
                    byte[] buffer10 = BitConverter.GetBytes((long) obj);
                    dataStream.Write(buffer10, 0, 8);
                    num = 20;
                }
                else if (obj is ulong)
                {
                    dataStream = new MemoryStream(8);
                    byte[] buffer11 = BitConverter.GetBytes((ulong) obj);
                    dataStream.Write(buffer11, 0, 8);
                    num = 0x15;
                }
                else if (obj is float)
                {
                    dataStream = new MemoryStream(4);
                    byte[] buffer12 = BitConverter.GetBytes((float) obj);
                    dataStream.Write(buffer12, 0, 4);
                    num = 4;
                }
                else if (obj is IPersistStream)
                {
                    IPersistStream persistStream = (IPersistStream) obj;
                    ComStreamFromDataStream stream3 = new ComStreamFromDataStream(new MemoryStream());
                    System.Messaging.Interop.NativeMethods.OleSaveToStream(persistStream, stream3);
                    dataStream = stream3.GetDataStream();
                    num = 0x44;
                }
                else
                {
                    if (obj != null)
                    {
                        throw new InvalidOperationException(Res.GetString("InvalidTypeSerialization"));
                    }
                    dataStream = new MemoryStream();
                    num = 1;
                }
                message.BodyStream = dataStream;
                message.BodyType = num;
            }
        }

        [ComVisible(false)]
        private class ComStreamFromDataStream : IStream
        {
            private Stream dataStream;
            private long virtualPosition = -1L;

            public ComStreamFromDataStream(Stream dataStream)
            {
                if (dataStream == null)
                {
                    throw new ArgumentNullException("dataStream");
                }
                this.dataStream = dataStream;
            }

            private void ActualizeVirtualPosition()
            {
                if (this.virtualPosition != -1L)
                {
                    if (this.virtualPosition > this.dataStream.Length)
                    {
                        this.dataStream.SetLength(this.virtualPosition);
                    }
                    this.dataStream.Position = this.virtualPosition;
                    this.virtualPosition = -1L;
                }
            }

            public IStream Clone()
            {
                NotImplemented();
                return null;
            }

            public void Commit(int grfCommitFlags)
            {
                this.dataStream.Flush();
                this.ActualizeVirtualPosition();
            }

            public long CopyTo(IStream pstm, long cb, long[] pcbRead)
            {
                int num = 0x1000;
                IntPtr buf = Marshal.AllocHGlobal((IntPtr) num);
                if (buf == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                long num2 = 0L;
                try
                {
                    while (num2 < cb)
                    {
                        int length = num;
                        if ((num2 + length) > cb)
                        {
                            length = (int) (cb - num2);
                        }
                        int len = this.Read(buf, length);
                        if (len == 0)
                        {
                            goto Label_0076;
                        }
                        if (pstm.Write(buf, len) != len)
                        {
                            throw EFail(Res.GetString("IncorrectNumberOfBytes"));
                        }
                        num2 += len;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            Label_0076:
                if ((pcbRead != null) && (pcbRead.Length > 0))
                {
                    pcbRead[0] = num2;
                }
                return num2;
            }

            protected static ExternalException EFail(string msg)
            {
                ExternalException exception = new ExternalException(msg, -2147467259);
                throw exception;
            }

            public Stream GetDataStream()
            {
                return this.dataStream;
            }

            public void LockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            protected static void NotImplemented()
            {
                ExternalException exception = new ExternalException(Res.GetString("NotImplemented"), -2147467263);
                throw exception;
            }

            public int Read(IntPtr buf, int length)
            {
                byte[] buffer = new byte[length];
                int num = this.Read(buffer, length);
                Marshal.Copy(buffer, 0, buf, length);
                return num;
            }

            public int Read(byte[] buffer, int length)
            {
                this.ActualizeVirtualPosition();
                return this.dataStream.Read(buffer, 0, length);
            }

            public void Revert()
            {
                NotImplemented();
            }

            public long Seek(long offset, int origin)
            {
                long virtualPosition = this.virtualPosition;
                if (this.virtualPosition == -1L)
                {
                    virtualPosition = this.dataStream.Position;
                }
                long length = this.dataStream.Length;
                switch (origin)
                {
                    case 0:
                        if (offset > length)
                        {
                            this.virtualPosition = offset;
                            break;
                        }
                        this.dataStream.Position = offset;
                        this.virtualPosition = -1L;
                        break;

                    case 1:
                        if ((offset + virtualPosition) > length)
                        {
                            this.virtualPosition = offset + virtualPosition;
                            break;
                        }
                        this.dataStream.Position = virtualPosition + offset;
                        this.virtualPosition = -1L;
                        break;

                    case 2:
                        if (offset > 0L)
                        {
                            this.virtualPosition = length + offset;
                            break;
                        }
                        this.dataStream.Position = length + offset;
                        this.virtualPosition = -1L;
                        break;
                }
                if (this.virtualPosition != -1L)
                {
                    return this.virtualPosition;
                }
                return this.dataStream.Position;
            }

            public void SetSize(long value)
            {
                this.dataStream.SetLength(value);
            }

            public void Stat(IntPtr pstatstg, int grfStatFlag)
            {
                NotImplemented();
            }

            public void UnlockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public int Write(IntPtr buf, int length)
            {
                byte[] destination = new byte[length];
                Marshal.Copy(buf, destination, 0, length);
                return this.Write(destination, length);
            }

            public int Write(byte[] buffer, int length)
            {
                this.ActualizeVirtualPosition();
                this.dataStream.Write(buffer, 0, length);
                return length;
            }
        }
    }
}

