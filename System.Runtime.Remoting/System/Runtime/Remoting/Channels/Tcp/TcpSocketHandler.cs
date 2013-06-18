namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Text;

    internal abstract class TcpSocketHandler : SocketHandler
    {
        private static byte[] s_protocolPreamble = Encoding.ASCII.GetBytes(".NET");
        private static byte[] s_protocolVersion1_0;

        static TcpSocketHandler()
        {
            byte[] buffer = new byte[2];
            buffer[0] = 1;
            s_protocolVersion1_0 = buffer;
        }

        public TcpSocketHandler(Socket socket, Stream stream) : this(socket, null, stream)
        {
        }

        public TcpSocketHandler(Socket socket, RequestQueue requestQueue, Stream stream) : base(socket, requestQueue, stream)
        {
        }

        private void ReadAndMatchPreamble()
        {
            if (!base.ReadAndMatchFourBytes(s_protocolPreamble))
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_ExpectingPreamble"));
            }
        }

        private void ReadAndVerifyHeaderFormat(string headerName, byte expectedFormat)
        {
            byte num = (byte) base.ReadByte();
            if (num != expectedFormat)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_IncorrectHeaderFormat"), new object[] { expectedFormat, headerName }));
            }
        }

        protected void ReadContentLength(out bool chunked, out int contentLength)
        {
            contentLength = -1;
            ushort num = base.ReadUInt16();
            switch (num)
            {
                case 1:
                    chunked = true;
                    return;

                case 0:
                    chunked = false;
                    contentLength = base.ReadInt32();
                    return;
            }
            throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_ExpectingContentLengthHeader"), new object[] { num.ToString(CultureInfo.CurrentCulture) }));
        }

        protected string ReadCountedString()
        {
            byte num = (byte) base.ReadByte();
            int count = base.ReadInt32();
            if (count <= 0)
            {
                return null;
            }
            byte[] buffer = new byte[count];
            base.Read(buffer, 0, count);
            switch (num)
            {
                case 0:
                    return Encoding.Unicode.GetString(buffer);

                case 1:
                    return Encoding.UTF8.GetString(buffer);
            }
            throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_UnrecognizedStringFormat"), new object[] { num.ToString(CultureInfo.CurrentCulture) }));
        }

        protected void ReadToEndOfHeaders(BaseTransportHeaders headers)
        {
            bool flag = false;
            string str = null;
            for (ushort i = base.ReadUInt16(); i != 0; i = base.ReadUInt16())
            {
                switch (i)
                {
                    case 1:
                    {
                        string str2 = this.ReadCountedString();
                        string str3 = this.ReadCountedString();
                        headers[str2] = str3;
                        break;
                    }
                    case 4:
                    {
                        string str6;
                        this.ReadAndVerifyHeaderFormat("RequestUri", 1);
                        string url = this.ReadCountedString();
                        if (TcpChannelHelper.ParseURL(url, out str6) == null)
                        {
                            str6 = url;
                        }
                        headers.RequestUri = str6;
                        break;
                    }
                    case 2:
                        this.ReadAndVerifyHeaderFormat("StatusCode", 3);
                        if (base.ReadUInt16() != 0)
                        {
                            flag = true;
                        }
                        break;

                    case 3:
                        this.ReadAndVerifyHeaderFormat("StatusPhrase", 1);
                        str = this.ReadCountedString();
                        break;

                    case 6:
                    {
                        this.ReadAndVerifyHeaderFormat("Content-Type", 1);
                        string str7 = this.ReadCountedString();
                        headers.ContentType = str7;
                        break;
                    }
                    default:
                    {
                        byte num3 = (byte) base.ReadByte();
                        switch (num3)
                        {
                            case 1:
                            {
                                this.ReadCountedString();
                                continue;
                            }
                            case 2:
                            {
                                base.ReadByte();
                                continue;
                            }
                            case 3:
                            {
                                base.ReadUInt16();
                                continue;
                            }
                            case 4:
                            {
                                base.ReadInt32();
                                continue;
                            }
                        }
                        throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_UnknownHeaderType"), new object[] { i, num3 }));
                    }
                }
            }
            if (flag)
            {
                if (str == null)
                {
                    str = "";
                }
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_GenericServerError"), new object[] { str }));
            }
        }

        protected void ReadVersionAndOperation(out ushort operation)
        {
            this.ReadAndMatchPreamble();
            byte num = (byte) base.ReadByte();
            byte num2 = (byte) base.ReadByte();
            if ((num != 1) || (num2 != 0))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_UnknownProtocolVersion"), new object[] { num.ToString(CultureInfo.CurrentCulture) + "." + num2.ToString(CultureInfo.CurrentCulture) }));
            }
            operation = base.ReadUInt16();
        }

        private void WriteContentTypeHeader(string value, Stream outputStream)
        {
            base.WriteUInt16(6, outputStream);
            base.WriteByte(1, outputStream);
            this.WriteCountedString(value, outputStream);
        }

        protected void WriteCountedString(string str, Stream outputStream)
        {
            int length = 0;
            if (str != null)
            {
                length = str.Length;
            }
            if (length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                base.WriteByte(1, outputStream);
                base.WriteInt32(bytes.Length, outputStream);
                outputStream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                base.WriteByte(0, outputStream);
                base.WriteInt32(0, outputStream);
            }
        }

        private void WriteCustomHeader(string name, string value, Stream outputStream)
        {
            base.WriteUInt16(1, outputStream);
            this.WriteCountedString(name, outputStream);
            this.WriteCountedString(value, outputStream);
        }

        protected void WriteHeaders(ITransportHeaders headers, Stream outputStream)
        {
            IEnumerator otherHeadersEnumerator = null;
            BaseTransportHeaders headers2 = headers as BaseTransportHeaders;
            if (headers2 != null)
            {
                if (headers2.ContentType != null)
                {
                    this.WriteContentTypeHeader(headers2.ContentType, outputStream);
                }
                otherHeadersEnumerator = headers2.GetOtherHeadersEnumerator();
            }
            else
            {
                otherHeadersEnumerator = headers.GetEnumerator();
            }
            if (otherHeadersEnumerator != null)
            {
                while (otherHeadersEnumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry) otherHeadersEnumerator.Current;
                    string key = (string) current.Key;
                    if (!StringHelper.StartsWithDoubleUnderscore(key))
                    {
                        string str2 = current.Value.ToString();
                        if ((headers2 == null) && (string.Compare(key, "Content-Type", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            this.WriteContentTypeHeader(str2, outputStream);
                        }
                        else
                        {
                            this.WriteCustomHeader(key, str2, outputStream);
                        }
                    }
                }
            }
            base.WriteUInt16(0, outputStream);
        }

        protected void WritePreambleAndVersion(Stream outputStream)
        {
            outputStream.Write(s_protocolPreamble, 0, s_protocolPreamble.Length);
            outputStream.Write(s_protocolVersion1_0, 0, s_protocolVersion1_0.Length);
        }
    }
}

