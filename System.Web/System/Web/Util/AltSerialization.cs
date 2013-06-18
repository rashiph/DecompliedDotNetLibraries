namespace System.Web.Util
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Web;

    internal static class AltSerialization
    {
        internal static object ReadValueFromStream(BinaryReader reader)
        {
            switch (reader.ReadByte())
            {
                case 1:
                    return reader.ReadString();

                case 2:
                    return reader.ReadInt32();

                case 3:
                    return reader.ReadBoolean();

                case 4:
                    return new DateTime(reader.ReadInt64());

                case 5:
                {
                    int[] bits = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        bits[i] = reader.ReadInt32();
                    }
                    return new decimal(bits);
                }
                case 6:
                    return reader.ReadByte();

                case 7:
                    return reader.ReadChar();

                case 8:
                    return reader.ReadSingle();

                case 9:
                    return reader.ReadDouble();

                case 10:
                    return reader.ReadSByte();

                case 11:
                    return reader.ReadInt16();

                case 12:
                    return reader.ReadInt64();

                case 13:
                    return reader.ReadUInt16();

                case 14:
                    return reader.ReadUInt32();

                case 15:
                    return reader.ReadUInt64();

                case 0x10:
                    return new TimeSpan(reader.ReadInt64());

                case 0x11:
                    return new Guid(reader.ReadBytes(0x10));

                case 0x12:
                    if (IntPtr.Size != 4)
                    {
                        return new IntPtr(reader.ReadInt64());
                    }
                    return new IntPtr(reader.ReadInt32());

                case 0x13:
                    if (UIntPtr.Size != 4)
                    {
                        return new UIntPtr(reader.ReadUInt64());
                    }
                    return new UIntPtr(reader.ReadUInt32());

                case 20:
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return formatter.Deserialize(reader.BaseStream);
                }
                case 0x15:
                    return null;
            }
            return null;
        }

        internal static void WriteValueToStream(object value, BinaryWriter writer)
        {
            if (value == null)
            {
                writer.Write((byte) 0x15);
            }
            else if (value is string)
            {
                writer.Write((byte) 1);
                writer.Write((string) value);
            }
            else if (value is int)
            {
                writer.Write((byte) 2);
                writer.Write((int) value);
            }
            else if (value is bool)
            {
                writer.Write((byte) 3);
                writer.Write((bool) value);
            }
            else if (value is DateTime)
            {
                writer.Write((byte) 4);
                DateTime time = (DateTime) value;
                writer.Write(time.Ticks);
            }
            else if (value is decimal)
            {
                writer.Write((byte) 5);
                int[] bits = decimal.GetBits((decimal) value);
                for (int i = 0; i < 4; i++)
                {
                    writer.Write(bits[i]);
                }
            }
            else if (value is byte)
            {
                writer.Write((byte) 6);
                writer.Write((byte) value);
            }
            else if (value is char)
            {
                writer.Write((byte) 7);
                writer.Write((char) value);
            }
            else if (value is float)
            {
                writer.Write((byte) 8);
                writer.Write((float) value);
            }
            else if (value is double)
            {
                writer.Write((byte) 9);
                writer.Write((double) value);
            }
            else if (value is sbyte)
            {
                writer.Write((byte) 10);
                writer.Write((sbyte) value);
            }
            else if (value is short)
            {
                writer.Write((byte) 11);
                writer.Write((short) value);
            }
            else if (value is long)
            {
                writer.Write((byte) 12);
                writer.Write((long) value);
            }
            else if (value is ushort)
            {
                writer.Write((byte) 13);
                writer.Write((ushort) value);
            }
            else if (value is uint)
            {
                writer.Write((byte) 14);
                writer.Write((uint) value);
            }
            else if (value is ulong)
            {
                writer.Write((byte) 15);
                writer.Write((ulong) value);
            }
            else if (value is TimeSpan)
            {
                writer.Write((byte) 0x10);
                TimeSpan span = (TimeSpan) value;
                writer.Write(span.Ticks);
            }
            else if (value is Guid)
            {
                writer.Write((byte) 0x11);
                byte[] buffer = ((Guid) value).ToByteArray();
                writer.Write(buffer);
            }
            else if (value is IntPtr)
            {
                writer.Write((byte) 0x12);
                IntPtr ptr = (IntPtr) value;
                if (IntPtr.Size == 4)
                {
                    writer.Write(ptr.ToInt32());
                }
                else
                {
                    writer.Write(ptr.ToInt64());
                }
            }
            else if (value is UIntPtr)
            {
                writer.Write((byte) 0x13);
                UIntPtr ptr2 = (UIntPtr) value;
                if (UIntPtr.Size == 4)
                {
                    writer.Write(ptr2.ToUInt32());
                }
                else
                {
                    writer.Write(ptr2.ToUInt64());
                }
            }
            else
            {
                writer.Write((byte) 20);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(writer.BaseStream, value);
                }
                catch (Exception exception)
                {
                    HttpException e = new HttpException(System.Web.SR.GetString("Cant_serialize_session_state"), exception);
                    e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
                    throw e;
                }
            }
        }

        private enum TypeID : byte
        {
            Boolean = 3,
            Byte = 6,
            Char = 7,
            DateTime = 4,
            Decimal = 5,
            Double = 9,
            Guid = 0x11,
            Int16 = 11,
            Int32 = 2,
            Int64 = 12,
            IntPtr = 0x12,
            Null = 0x15,
            Object = 20,
            SByte = 10,
            Single = 8,
            String = 1,
            TimeSpan = 0x10,
            UInt16 = 13,
            UInt32 = 14,
            UInt64 = 15,
            UIntPtr = 0x13
        }
    }
}

