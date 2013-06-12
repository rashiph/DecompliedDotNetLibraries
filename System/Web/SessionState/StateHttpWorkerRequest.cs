namespace System.Web.SessionState
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web;

    internal class StateHttpWorkerRequest : HttpWorkerRequest
    {
        private byte[] _content;
        private int _contentLength;
        private UnsafeNativeMethods.StateProtocolExclusive _exclusive;
        private int _extraFlags;
        private StringBuilder _headers;
        private string _localAddress;
        private int _localPort;
        private int _lockCookie;
        private bool _lockCookieExists;
        private string _method;
        private UnsafeNativeMethods.StateProtocolVerb _methodIndex;
        private string _remoteAddress;
        private int _remotePort;
        private bool _sent;
        private StringBuilder _status;
        private int _statusCode;
        private int _timeout;
        private IntPtr _tracker;
        private IntPtr _unmanagedState;
        private string _uri;
        private const int ADDRESS_LENGTH_MAX = 15;

        internal StateHttpWorkerRequest(IntPtr tracker, UnsafeNativeMethods.StateProtocolVerb methodIndex, string uri, UnsafeNativeMethods.StateProtocolExclusive exclusive, int extraFlags, int timeout, int lockCookieExists, int lockCookie, int contentLength, IntPtr content)
        {
            this._tracker = tracker;
            this._methodIndex = methodIndex;
            switch (this._methodIndex)
            {
                case UnsafeNativeMethods.StateProtocolVerb.GET:
                    this._method = "GET";
                    break;

                case UnsafeNativeMethods.StateProtocolVerb.PUT:
                    this._method = "PUT";
                    break;

                case UnsafeNativeMethods.StateProtocolVerb.DELETE:
                    this._method = "DELETE";
                    break;

                case UnsafeNativeMethods.StateProtocolVerb.HEAD:
                    this._method = "HEAD";
                    break;
            }
            this._uri = uri;
            if (this._uri.StartsWith("//", StringComparison.Ordinal))
            {
                this._uri = this._uri.Substring(1);
            }
            this._exclusive = exclusive;
            this._extraFlags = extraFlags;
            this._timeout = timeout;
            this._lockCookie = lockCookie;
            this._lockCookieExists = lockCookieExists != 0;
            this._contentLength = contentLength;
            if (contentLength != 0)
            {
                uint num = (uint) ((int) content);
                this._content = new byte[] { (byte) (num & 0xff), (byte) ((num & 0xff00) >> 8), (byte) ((num & 0xff0000) >> 0x10), (byte) ((num & -16777216) >> 0x18) };
            }
            this._status = new StringBuilder(0x100);
            this._headers = new StringBuilder(0x100);
        }

        public override void CloseConnection()
        {
            UnsafeNativeMethods.STWNDCloseConnection(this._tracker);
        }

        public override void EndOfRequest()
        {
            this.SendResponse();
            UnsafeNativeMethods.STWNDEndOfRequest(this._tracker);
        }

        public override void FlushResponse(bool finalFlush)
        {
            this.SendResponse();
        }

        public override long GetBytesRead()
        {
            throw new NotSupportedException(System.Web.SR.GetString("Not_supported"));
        }

        public override string GetFilePath()
        {
            return null;
        }

        public override string GetHttpVerbName()
        {
            return this._method;
        }

        public override string GetHttpVersion()
        {
            return "HTTP/1.0";
        }

        public override string GetKnownRequestHeader(int index)
        {
            string str = null;
            if (index == 11)
            {
                str = this._contentLength.ToString(CultureInfo.InvariantCulture);
            }
            return str;
        }

        public override string GetLocalAddress()
        {
            if (this._localAddress == null)
            {
                StringBuilder buf = new StringBuilder(15);
                UnsafeNativeMethods.STWNDGetLocalAddress(this._tracker, buf);
                this._localAddress = buf.ToString();
            }
            return this._localAddress;
        }

        public override int GetLocalPort()
        {
            if (this._localPort == 0)
            {
                this._localPort = UnsafeNativeMethods.STWNDGetLocalPort(this._tracker);
            }
            return this._localPort;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return this._content;
        }

        public override string GetQueryString()
        {
            return null;
        }

        public override string GetRawUrl()
        {
            return this._uri;
        }

        public override string GetRemoteAddress()
        {
            if (this._remoteAddress == null)
            {
                StringBuilder buf = new StringBuilder(15);
                UnsafeNativeMethods.STWNDGetRemoteAddress(this._tracker, buf);
                this._remoteAddress = buf.ToString();
            }
            return this._remoteAddress;
        }

        public override int GetRemotePort()
        {
            if (this._remotePort == 0)
            {
                this._remotePort = UnsafeNativeMethods.STWNDGetRemotePort(this._tracker);
            }
            return this._remotePort;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            string str = null;
            if (name.Equals("Http_Exclusive"))
            {
                switch (this._exclusive)
                {
                    case UnsafeNativeMethods.StateProtocolExclusive.ACQUIRE:
                        return "acquire";

                    case UnsafeNativeMethods.StateProtocolExclusive.RELEASE:
                        return "release";
                }
                return str;
            }
            if (name.Equals("Http_Timeout"))
            {
                if (this._timeout != -1)
                {
                    str = this._timeout.ToString(CultureInfo.InvariantCulture);
                }
                return str;
            }
            if (name.Equals("Http_LockCookie"))
            {
                if (this._lockCookieExists)
                {
                    str = this._lockCookie.ToString(CultureInfo.InvariantCulture);
                }
                return str;
            }
            if (name.Equals("Http_ExtraFlags") && (this._extraFlags != -1))
            {
                str = this._extraFlags.ToString(CultureInfo.InvariantCulture);
            }
            return str;
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            int num = 0;
            if (this._exclusive != ~UnsafeNativeMethods.StateProtocolExclusive.NONE)
            {
                num++;
            }
            if (this._extraFlags != -1)
            {
                num++;
            }
            if (this._timeout != -1)
            {
                num++;
            }
            if (this._lockCookieExists)
            {
                num++;
            }
            if (num == 0)
            {
                return null;
            }
            string[][] strArray = new string[num][];
            int index = 0;
            if (this._exclusive != ~UnsafeNativeMethods.StateProtocolExclusive.NONE)
            {
                strArray[0] = new string[2];
                strArray[0][0] = "Http_Exclusive";
                if (this._exclusive == UnsafeNativeMethods.StateProtocolExclusive.ACQUIRE)
                {
                    strArray[0][1] = "acquire";
                }
                else
                {
                    strArray[0][1] = "release";
                }
                index++;
            }
            if (this._timeout != -1)
            {
                strArray[index] = new string[] { "Http_Timeout", this._timeout.ToString(CultureInfo.InvariantCulture) };
                index++;
            }
            if (this._lockCookieExists)
            {
                strArray[index] = new string[] { "Http_LockCookie", this._lockCookie.ToString(CultureInfo.InvariantCulture) };
                index++;
            }
            if (this._extraFlags != -1)
            {
                strArray[index] = new string[] { "Http_ExtraFlags", this._extraFlags.ToString(CultureInfo.InvariantCulture) };
                index++;
            }
            return strArray;
        }

        public override string GetUriPath()
        {
            return HttpUtility.UrlDecode(this._uri);
        }

        public override bool HeadersSent()
        {
            return this._sent;
        }

        public override bool IsClientConnected()
        {
            return UnsafeNativeMethods.STWNDIsClientConnected(this._tracker);
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return true;
        }

        public override string MapPath(string virtualPath)
        {
            return virtualPath;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return 0;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            this._headers.Append(HttpWorkerRequest.GetKnownResponseHeaderName(index));
            this._headers.Append(": ");
            this._headers.Append(value);
            this._headers.Append("\r\n");
        }

        private void SendResponse()
        {
            if (!this._sent)
            {
                this._sent = true;
                UnsafeNativeMethods.STWNDSendResponse(this._tracker, this._status, this._status.Length, this._headers, this._headers.Length, this._unmanagedState);
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            throw new NotSupportedException(System.Web.SR.GetString("Not_supported"));
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            throw new NotSupportedException(System.Web.SR.GetString("Not_supported"));
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (this._statusCode == 200)
            {
                if (IntPtr.Size == 4)
                {
                    this._unmanagedState = (IntPtr) (((data[0] | (data[1] << 8)) | (data[2] << 0x10)) | (data[3] << 0x18));
                }
                else
                {
                    this._unmanagedState = (IntPtr) (((((((data[0] | (data[1] << 8)) | (data[2] << 0x10)) | (data[3] << 0x18)) | (data[4] << 0x20)) | (data[5] << 40)) | (data[6] << 0x30)) | (data[7] << 0x38));
                }
            }
            this.SendResponse();
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            this._statusCode = statusCode;
            this._status.Append(statusCode.ToString(CultureInfo.InvariantCulture) + " " + statusDescription + "\r\n");
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            this._headers.Append(name);
            this._headers.Append(": ");
            this._headers.Append(value);
            this._headers.Append("\r\n");
        }
    }
}

