namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;

    internal class ActiveXSerializer
    {
        private object bufferLock = new object();
        private byte[] byteBuffer;
        private char[] charBuffer;

        public object Deserialize(MemoryStream stream, int bodyType)
        {
            byte[] buffer;
            int length;
            bool flag;
            if (stream == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            }
            switch (((VarEnum) bodyType))
            {
                case VarEnum.VT_NULL:
                    return null;

                case VarEnum.VT_I2:
                    buffer = new byte[2];
                    if (stream.Read(buffer, 0, 2) != 2)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToInt16(buffer, 0);

                case VarEnum.VT_I4:
                    buffer = new byte[4];
                    if (stream.Read(buffer, 0, 4) != 4)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToInt32(buffer, 0);

                case VarEnum.VT_R4:
                    buffer = new byte[4];
                    if (stream.Read(buffer, 0, 4) != 4)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToSingle(buffer, 0);

                case VarEnum.VT_R8:
                    buffer = new byte[8];
                    if (stream.Read(buffer, 0, 8) != 8)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToDouble(buffer, 0);

                case VarEnum.VT_CY:
                    buffer = new byte[8];
                    if (stream.Read(buffer, 0, 8) != 8)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return decimal.FromOACurrency(BitConverter.ToInt64(buffer, 0));

                case VarEnum.VT_DATE:
                    buffer = new byte[8];
                    if (stream.Read(buffer, 0, 8) != 8)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return new DateTime(BitConverter.ToInt64(buffer, 0));

                case VarEnum.VT_BSTR:
                case VarEnum.VT_LPWSTR:
                    break;

                case VarEnum.VT_BOOL:
                    buffer = new byte[1];
                    if (stream.Read(buffer, 0, 1) != 1)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return (buffer[0] != 0);

                case VarEnum.VT_I1:
                case VarEnum.VT_UI1:
                    buffer = new byte[1];
                    if (stream.Read(buffer, 0, 1) != 1)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return buffer[0];

                case VarEnum.VT_UI2:
                    buffer = new byte[2];
                    if (stream.Read(buffer, 0, 2) != 2)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToUInt16(buffer, 0);

                case VarEnum.VT_UI4:
                    buffer = new byte[4];
                    if (stream.Read(buffer, 0, 4) != 4)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToUInt32(buffer, 0);

                case VarEnum.VT_I8:
                    buffer = new byte[8];
                    if (stream.Read(buffer, 0, 8) != 8)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToInt64(buffer, 0);

                case VarEnum.VT_UI8:
                    buffer = new byte[8];
                    if (stream.Read(buffer, 0, 8) != 8)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return BitConverter.ToUInt64(buffer, 0);

                case VarEnum.VT_LPSTR:
                    buffer = stream.ToArray();
                    length = buffer.Length;
                    flag = false;
                    try
                    {
                        char[] chars = this.TakeLockedBuffer<char>(out flag, length);
                        Encoding.ASCII.GetChars(buffer, 0, length, chars, 0);
                        return new string(chars, 0, length);
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.ReleaseLockedBuffer();
                        }
                    }
                    break;

                case VarEnum.VT_CLSID:
                    buffer = new byte[0x10];
                    if (stream.Read(buffer, 0, 0x10) != 0x10)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeActiveXMessage")));
                    }
                    return new Guid(buffer);

                case (VarEnum.VT_VECTOR | VarEnum.VT_UI1):
                    goto Label_014A;

                default:
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqInvalidTypeDeserialization")));
            }
            buffer = stream.ToArray();
            length = buffer.Length / 2;
            flag = false;
            try
            {
                char[] chArray2 = this.TakeLockedBuffer<char>(out flag, length);
                Encoding.Unicode.GetChars(buffer, 0, length * 2, chArray2, 0);
                return new string(chArray2, 0, length);
            }
            finally
            {
                if (flag)
                {
                    this.ReleaseLockedBuffer();
                }
            }
        Label_014A:
            buffer = stream.ToArray();
            byte[] destinationArray = new byte[buffer.Length];
            Array.Copy(buffer, destinationArray, buffer.Length);
            return destinationArray;
        }

        private void ReleaseLockedBuffer()
        {
            Monitor.Exit(this.bufferLock);
        }

        public void Serialize(Stream stream, object obj, ref int bodyType)
        {
            VarEnum enum2;
            if (stream == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            }
            if (obj is string)
            {
                int size = ((string) obj).Length * 2;
                bool lockHeld = false;
                try
                {
                    byte[] bytes = this.TakeLockedBuffer<byte>(out lockHeld, size);
                    Encoding.Unicode.GetBytes(((string) obj).ToCharArray(), 0, size / 2, bytes, 0);
                    stream.Write(bytes, 0, size);
                }
                finally
                {
                    if (lockHeld)
                    {
                        this.ReleaseLockedBuffer();
                    }
                }
                enum2 = VarEnum.VT_LPWSTR;
            }
            else if (obj is byte[])
            {
                byte[] buffer = (byte[]) obj;
                stream.Write(buffer, 0, buffer.Length);
                enum2 = VarEnum.VT_VECTOR | VarEnum.VT_UI1;
            }
            else if (obj is char[])
            {
                char[] chars = (char[]) obj;
                int num2 = chars.Length * 2;
                bool flag2 = false;
                try
                {
                    byte[] buffer3 = this.TakeLockedBuffer<byte>(out flag2, num2);
                    Encoding.Unicode.GetBytes(chars, 0, num2 / 2, buffer3, 0);
                    stream.Write(buffer3, 0, num2);
                }
                finally
                {
                    if (flag2)
                    {
                        this.ReleaseLockedBuffer();
                    }
                }
                enum2 = VarEnum.VT_LPWSTR;
            }
            else if (obj is byte)
            {
                stream.Write(new byte[] { (byte) obj }, 0, 1);
                enum2 = VarEnum.VT_UI1;
            }
            else if (obj is bool)
            {
                if ((bool) obj)
                {
                    stream.Write(new byte[] { 0xff }, 0, 1);
                }
                else
                {
                    byte[] buffer18 = new byte[1];
                    stream.Write(buffer18, 0, 1);
                }
                enum2 = VarEnum.VT_BOOL;
            }
            else if (obj is char)
            {
                byte[] buffer4 = BitConverter.GetBytes((char) obj);
                stream.Write(buffer4, 0, 2);
                enum2 = VarEnum.VT_UI2;
            }
            else if (obj is decimal)
            {
                byte[] buffer5 = BitConverter.GetBytes(decimal.ToOACurrency((decimal) obj));
                stream.Write(buffer5, 0, 8);
                enum2 = VarEnum.VT_CY;
            }
            else if (obj is DateTime)
            {
                DateTime time = (DateTime) obj;
                byte[] buffer6 = BitConverter.GetBytes(time.Ticks);
                stream.Write(buffer6, 0, 8);
                enum2 = VarEnum.VT_DATE;
            }
            else if (obj is double)
            {
                byte[] buffer7 = BitConverter.GetBytes((double) obj);
                stream.Write(buffer7, 0, 8);
                enum2 = VarEnum.VT_R8;
            }
            else if (obj is Guid)
            {
                byte[] buffer8 = ((Guid) obj).ToByteArray();
                stream.Write(buffer8, 0, 0x10);
                enum2 = VarEnum.VT_CLSID;
            }
            else if (obj is short)
            {
                byte[] buffer9 = BitConverter.GetBytes((short) obj);
                stream.Write(buffer9, 0, 2);
                enum2 = VarEnum.VT_I2;
            }
            else if (obj is ushort)
            {
                byte[] buffer10 = BitConverter.GetBytes((ushort) obj);
                stream.Write(buffer10, 0, 2);
                enum2 = VarEnum.VT_UI2;
            }
            else if (obj is int)
            {
                byte[] buffer11 = BitConverter.GetBytes((int) obj);
                stream.Write(buffer11, 0, 4);
                enum2 = VarEnum.VT_I4;
            }
            else if (obj is uint)
            {
                byte[] buffer12 = BitConverter.GetBytes((uint) obj);
                stream.Write(buffer12, 0, 4);
                enum2 = VarEnum.VT_UI4;
            }
            else if (obj is long)
            {
                byte[] buffer13 = BitConverter.GetBytes((long) obj);
                stream.Write(buffer13, 0, 8);
                enum2 = VarEnum.VT_I8;
            }
            else if (obj is ulong)
            {
                byte[] buffer14 = BitConverter.GetBytes((ulong) obj);
                stream.Write(buffer14, 0, 8);
                enum2 = VarEnum.VT_UI8;
            }
            else
            {
                if (!(obj is float))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqInvalidTypeSerialization")));
                }
                byte[] buffer15 = BitConverter.GetBytes((float) obj);
                stream.Write(buffer15, 0, 4);
                enum2 = VarEnum.VT_R4;
            }
            bodyType = (int) enum2;
        }

        private TKind[] TakeLockedBuffer<TKind>(out bool lockHeld, int size)
        {
            lockHeld = false;
            try
            {
            }
            finally
            {
                Monitor.Enter(this.bufferLock);
                lockHeld = true;
            }
            if (typeof(byte) == typeof(TKind))
            {
                if ((this.byteBuffer == null) || (size > this.byteBuffer.Length))
                {
                    this.byteBuffer = new byte[size];
                }
                return (this.byteBuffer as TKind[]);
            }
            if (!(typeof(char) == typeof(TKind)))
            {
                return null;
            }
            if ((this.charBuffer == null) || (size > this.charBuffer.Length))
            {
                this.charBuffer = new char[size];
            }
            return (this.charBuffer as TKind[]);
        }
    }
}

