namespace Microsoft.Transactions.Wsat.Recovery
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    internal static class SerializationUtils
    {
        public static void AlignPosition(MemoryStream mem, int alignment)
        {
            int num = alignment - 1;
            long num2 = (mem.Position + num) & ~num;
            if ((mem.Position > num2) || (mem.Length < num2))
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            mem.Position = num2;
        }

        public static void IncrementPosition(MemoryStream mem, long increment)
        {
            if ((mem.Length < increment) || ((mem.Length - increment) < mem.Position))
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            mem.Position += increment;
        }

        public static byte ReadByte(MemoryStream mem)
        {
            int num = mem.ReadByte();
            if (num == -1)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            return (byte) num;
        }

        public static byte[] ReadBytes(MemoryStream mem, int bytes)
        {
            if ((mem.Length < bytes) || ((mem.Length - bytes) < mem.Position))
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            byte[] buffer = new byte[bytes];
            mem.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static EndpointAddress ReadEndpointAddress(MemoryStream mem, ProtocolVersion protocolVersion)
        {
            EndpointAddress address;
            try
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(mem, BinaryMessageEncoderFactory.XmlDictionary, XmlDictionaryReaderQuotas.Max);
                address = EndpointAddress.ReadFrom(MessagingVersionHelper.AddressingVersion(protocolVersion), reader);
            }
            catch (XmlException exception)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt"), exception));
            }
            return address;
        }

        public static Guid ReadGuid(MemoryStream mem)
        {
            if ((mem.Length - 0x10L) < mem.Position)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            byte[] destinationArray = new byte[0x10];
            Array.Copy(mem.GetBuffer(), mem.Position, destinationArray, 0L, 0x10L);
            IncrementPosition(mem, 0x10L);
            return new Guid(destinationArray);
        }

        public static int ReadInt(MemoryStream mem)
        {
            if ((mem.Length - 4L) < mem.Position)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            int num = BitConverter.ToInt32(mem.GetBuffer(), (int) mem.Position);
            IncrementPosition(mem, 4L);
            return num;
        }

        public static string ReadString(MemoryStream mem)
        {
            string str;
            long increment = ReadUShort(mem);
            if ((mem.Length - increment) < mem.Position)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            try
            {
                str = Encoding.UTF8.GetString(mem.GetBuffer(), (int) mem.Position, (int) increment);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt"), exception));
            }
            IncrementPosition(mem, increment);
            return str;
        }

        public static TimeSpan ReadTimeout(MemoryStream mem)
        {
            return TimeSpan.FromSeconds((double) ReadUInt(mem));
        }

        public static uint ReadUInt(MemoryStream mem)
        {
            if ((mem.Length - 4L) < mem.Position)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            uint num = BitConverter.ToUInt32(mem.GetBuffer(), (int) mem.Position);
            IncrementPosition(mem, 4L);
            return num;
        }

        public static ushort ReadUShort(MemoryStream mem)
        {
            if ((mem.Length - 2L) < mem.Position)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("DeserializationDataCorrupt")));
            }
            ushort num = BitConverter.ToUInt16(mem.GetBuffer(), (int) mem.Position);
            IncrementPosition(mem, 2L);
            return num;
        }

        public static void WriteEndpointAddress(MemoryStream mem, EndpointAddress address, ProtocolVersion protocolVersion)
        {
            try
            {
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(mem, BinaryMessageEncoderFactory.XmlDictionary);
                address.WriteTo(MessagingVersionHelper.AddressingVersion(protocolVersion), writer);
                writer.Flush();
            }
            catch (XmlException exception)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("EndpointReferenceSerializationFailed"), exception));
            }
        }

        public static void WriteGuid(MemoryStream mem, ref Guid value)
        {
            byte[] buffer = value.ToByteArray();
            mem.Write(buffer, 0, buffer.Length);
        }

        public static void WriteInt(MemoryStream mem, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mem.Write(bytes, 0, bytes.Length);
        }

        public static void WriteString(MemoryStream mem, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > 0xffff)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Serializing a string that is too long");
            }
            WriteUShort(mem, (ushort) bytes.Length);
            mem.Write(bytes, 0, bytes.Length);
        }

        public static void WriteTimeout(MemoryStream mem, TimeSpan span)
        {
            WriteUInt(mem, (uint) span.TotalSeconds);
        }

        public static void WriteUInt(MemoryStream mem, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mem.Write(bytes, 0, bytes.Length);
        }

        public static void WriteUShort(MemoryStream mem, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mem.Write(bytes, 0, bytes.Length);
        }
    }
}

