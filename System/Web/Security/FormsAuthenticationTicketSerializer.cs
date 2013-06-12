namespace System.Web.Security
{
    using System;
    using System.IO;

    internal static class FormsAuthenticationTicketSerializer
    {
        private const byte CURRENT_TICKET_SERIALIZED_VERSION = 1;

        public static FormsAuthenticationTicket Deserialize(byte[] serializedTicket, int serializedTicketLength)
        {
            FormsAuthenticationTicket ticket;
            try
            {
                using (MemoryStream stream = new MemoryStream(serializedTicket))
                {
                    using (SerializingBinaryReader reader = new SerializingBinaryReader(stream))
                    {
                        int num2;
                        DateTime time;
                        DateTime time2;
                        bool flag;
                        string str;
                        if (reader.ReadByte() == 1)
                        {
                            num2 = reader.ReadByte();
                            long ticks = reader.ReadInt64();
                            time = new DateTime(ticks, DateTimeKind.Utc);
                            time.ToLocalTime();
                            if (reader.ReadByte() != 0xfe)
                            {
                                return null;
                            }
                            long num5 = reader.ReadInt64();
                            time2 = new DateTime(num5, DateTimeKind.Utc);
                            time2.ToLocalTime();
                            switch (reader.ReadByte())
                            {
                                case 0:
                                    flag = false;
                                    goto Label_00A1;

                                case 1:
                                    flag = true;
                                    goto Label_00A1;
                            }
                        }
                        return null;
                    Label_00A1:
                        str = reader.ReadBinaryString();
                        string userData = reader.ReadBinaryString();
                        string cookiePath = reader.ReadBinaryString();
                        if (reader.ReadByte() != 0xff)
                        {
                            return null;
                        }
                        if (stream.Position != serializedTicketLength)
                        {
                            return null;
                        }
                        ticket = FormsAuthenticationTicket.FromUtc(num2, str, time, time2, flag, userData, cookiePath);
                    }
                }
            }
            catch
            {
                ticket = null;
            }
            return ticket;
        }

        public static byte[] Serialize(FormsAuthenticationTicket ticket)
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                using (SerializingBinaryWriter writer = new SerializingBinaryWriter(stream))
                {
                    writer.Write((byte) 1);
                    writer.Write((byte) ticket.Version);
                    writer.Write(ticket.IssueDateUtc.Ticks);
                    writer.Write((byte) 0xfe);
                    writer.Write(ticket.ExpirationUtc.Ticks);
                    writer.Write(ticket.IsPersistent);
                    writer.WriteBinaryString(ticket.Name);
                    writer.WriteBinaryString(ticket.UserData);
                    writer.WriteBinaryString(ticket.CookiePath);
                    writer.Write((byte) 0xff);
                    buffer = stream.ToArray();
                }
            }
            return buffer;
        }

        private sealed class SerializingBinaryReader : BinaryReader
        {
            public SerializingBinaryReader(Stream input) : base(input)
            {
            }

            public string ReadBinaryString()
            {
                int num = base.Read7BitEncodedInt();
                byte[] buffer = this.ReadBytes(num * 2);
                char[] chArray = new char[num];
                for (int i = 0; i < chArray.Length; i++)
                {
                    chArray[i] = (char) (buffer[2 * i] | (buffer[(2 * i) + 1] << 8));
                }
                return new string(chArray);
            }

            public override string ReadString()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class SerializingBinaryWriter : BinaryWriter
        {
            public SerializingBinaryWriter(Stream output) : base(output)
            {
            }

            public override void Write(string value)
            {
                throw new NotImplementedException();
            }

            public void WriteBinaryString(string value)
            {
                byte[] buffer = new byte[value.Length * 2];
                for (int i = 0; i < value.Length; i++)
                {
                    char ch = value[i];
                    buffer[2 * i] = (byte) ch;
                    buffer[(2 * i) + 1] = (byte) (ch >> 8);
                }
                base.Write7BitEncodedInt(value.Length);
                this.Write(buffer);
            }
        }
    }
}

