namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Text;

    internal abstract class HttpSocketHandler : SocketHandler
    {
        private static byte[] s_endOfLine = new byte[] { 13, 10 };
        private static byte[] s_headerSeparator = new byte[] { 0x3a, 0x20 };
        private static byte[] s_httpVersion = Encoding.ASCII.GetBytes("HTTP/1.1");
        private static byte[] s_httpVersionAndSpace = Encoding.ASCII.GetBytes("HTTP/1.1 ");

        public HttpSocketHandler(Socket socket, RequestQueue requestQueue, Stream stream) : base(socket, requestQueue, stream)
        {
        }

        protected void ReadToEndOfHeaders(BaseTransportHeaders headers, out bool bChunked, out int contentLength, ref bool bKeepAlive, ref bool bSendContinue)
        {
            bChunked = false;
            contentLength = 0;
            while (true)
            {
                string str = base.ReadToEndOfLine();
                if (str.Length == 0)
                {
                    return;
                }
                int index = str.IndexOf(":");
                string strA = str.Substring(0, index);
                string str3 = str.Substring((index + 1) + 1);
                if (string.Compare(strA, "Transfer-Encoding", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.Compare(str3, "chunked", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bChunked = true;
                    }
                }
                else if (string.Compare(strA, "Connection", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.Compare(str3, "Keep-Alive", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bKeepAlive = true;
                    }
                    else if (string.Compare(str3, "Close", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bKeepAlive = false;
                    }
                }
                else if (string.Compare(strA, "Expect", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.Compare(str3, "100-continue", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bSendContinue = true;
                    }
                }
                else if (string.Compare(strA, "Content-Length", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    contentLength = int.Parse(str3, CultureInfo.InvariantCulture);
                }
                else
                {
                    headers[strA] = str3;
                }
            }
        }

        private void WriteHeader(string name, string value, Stream outputStream)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(name);
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Write(s_headerSeparator, 0, s_headerSeparator.Length);
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Write(s_endOfLine, 0, s_endOfLine.Length);
        }

        protected void WriteHeaders(ITransportHeaders headers, Stream outputStream)
        {
            if (headers != null)
            {
                foreach (DictionaryEntry entry in headers)
                {
                    string key = (string) entry.Key;
                    if (!key.StartsWith("__", StringComparison.Ordinal))
                    {
                        this.WriteHeader(key, (string) entry.Value, outputStream);
                    }
                }
                outputStream.Write(s_endOfLine, 0, s_endOfLine.Length);
            }
        }

        protected void WriteResponseFirstLine(string statusCode, string reasonPhrase, Stream outputStream)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(statusCode);
            byte[] buffer = Encoding.ASCII.GetBytes(reasonPhrase);
            outputStream.Write(s_httpVersionAndSpace, 0, s_httpVersionAndSpace.Length);
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.WriteByte(0x20);
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Write(s_endOfLine, 0, s_endOfLine.Length);
        }
    }
}

