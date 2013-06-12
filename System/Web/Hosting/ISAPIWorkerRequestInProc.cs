namespace System.Web.Hosting
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    internal class ISAPIWorkerRequestInProc : ISAPIWorkerRequest
    {
        protected string[] _additionalServerVars;
        protected string[] _basicServerVars;
        private ChannelBinding _channelBindingToken;
        protected const int ALL_RAW = 11;
        protected const int APPL_PHYSICAL_PATH = 2;
        protected const int AUTH_PASSWORD = 13;
        protected const int AUTH_TYPE = 1;
        protected const int CACHE_URL = 7;
        protected const int CERT_COOKIE = 14;
        protected const int CERT_FLAGS = 15;
        protected const int CERT_ISSUER = 0x10;
        protected const int CERT_KEYSIZE = 0x11;
        protected const int CERT_SECRETKEYSIZE = 0x12;
        protected const int CERT_SERIALNUMBER = 0x13;
        protected const int CERT_SERVER_ISSUER = 20;
        protected const int CERT_SERVER_SUBJECT = 0x15;
        protected const int CERT_SUBJECT = 0x16;
        protected const int GATEWAY_INTERFACE = 0x17;
        protected const int HTTPS = 10;
        protected const int HTTPS_KEYSIZE = 0x18;
        protected const int HTTPS_SECRETKEYSIZE = 0x19;
        protected const int HTTPS_SERVER_ISSUER = 0x1a;
        protected const int HTTPS_SERVER_SUBJECT = 0x1b;
        protected const int INSTANCE_ID = 0x1c;
        protected const int INSTANCE_META_PATH = 0x1d;
        protected const int LOCAL_ADDR = 30;
        protected const int LOGON_USER = 0;
        protected const int NUM_ADDITIONAL_SERVER_VARIABLES = 0x17;
        protected const int NUM_BASIC_SERVER_VARIABLES = 12;
        protected const int NUM_SERVER_VARIABLES = 0x23;
        protected const int PATH_INFO = 4;
        protected const int PATH_TRANSLATED = 5;
        protected const int REMOTE_ADDR = 12;
        protected const int REMOTE_HOST = 0x1f;
        protected const int REMOTE_PORT = 0x20;
        protected const int REQUEST_METHOD = 3;
        protected const int SERVER_NAME = 8;
        protected const int SERVER_PORT = 9;
        protected const int SERVER_PROTOCOL = 0x21;
        protected const int SERVER_SOFTWARE = 0x22;
        protected const int URL = 6;

        internal ISAPIWorkerRequestInProc(IntPtr ecb) : base(ecb)
        {
            if ((ecb == IntPtr.Zero) || (UnsafeNativeMethods.EcbGetTraceContextId(ecb, out this._traceId) != 1))
            {
                base._traceId = Guid.Empty;
            }
        }

        internal override int AppendLogParameterCore(string logParam)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbAppendLogParameter(base._ecb, logParam);
        }

        internal override int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte[] bufIn, byte[] bufOut)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbCallISAPI(base._ecb, iFunction, bufIn, bufIn.Length, bufOut, bufOut.Length);
        }

        internal override void Close()
        {
            if ((this._channelBindingToken != null) && !this._channelBindingToken.IsInvalid)
            {
                this._channelBindingToken.Dispose();
            }
        }

        internal override int CloseConnectionCore()
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbCloseConnection(base._ecb);
        }

        internal override void FlushCore(byte[] status, byte[] header, int keepConnected, int totalBodySize, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int doneWithSession, int finalStatus, out bool async)
        {
            async = false;
            if (base._ecb != IntPtr.Zero)
            {
                UnsafeNativeMethods.EcbFlushCore(base._ecb, status, header, keepConnected, totalBodySize, numBodyFragments, bodyFragments, bodyFragmentLengths, doneWithSession, finalStatus, 0, 0, null);
            }
        }

        internal override int GetAdditionalPostedContentCore(byte[] bytes, int offset, int bufferSize)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            int delta = UnsafeNativeMethods.EcbGetAdditionalPostedContent(base._ecb, bytes, offset, bufferSize);
            if (delta > 0)
            {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, delta);
            }
            return delta;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private string GetAdditionalServerVar(int index)
        {
            if (this._additionalServerVars == null)
            {
                this.GetAdditionalServerVariables();
            }
            return this._additionalServerVars[index - 12];
        }

        protected virtual void GetAdditionalServerVariables()
        {
            if ((base._ecb != IntPtr.Zero) && (this._additionalServerVars == null))
            {
                this._additionalServerVars = new string[0x17];
                for (int i = 0; i < this._additionalServerVars.Length; i++)
                {
                    int nameIndex = i + 12;
                    RecyclableByteBuffer buffer = new RecyclableByteBuffer();
                    int len = UnsafeNativeMethods.EcbGetServerVariableByIndex(base._ecb, nameIndex, buffer.Buffer, buffer.Buffer.Length);
                    while (len < 0)
                    {
                        buffer.Resize(-len);
                        len = UnsafeNativeMethods.EcbGetServerVariableByIndex(base._ecb, nameIndex, buffer.Buffer, buffer.Buffer.Length);
                    }
                    if (len > 0)
                    {
                        this._additionalServerVars[i] = buffer.GetDecodedString(Encoding.UTF8, len);
                    }
                    buffer.Dispose();
                }
            }
        }

        internal override int GetBasicsCore(byte[] buffer, int size, int[] contentInfo)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbGetBasics(base._ecb, buffer, size, contentInfo);
        }

        internal override int GetClientCertificateCore(byte[] buffer, int[] pInts, long[] pDates)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbGetClientCertificate(base._ecb, buffer, buffer.Length, pInts, pDates);
        }

        internal override int GetPreloadedPostedContentCore(byte[] bytes, int offset, int numBytesToRead)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            int delta = UnsafeNativeMethods.EcbGetPreloadedPostedContent(base._ecb, bytes, offset, numBytesToRead);
            if (delta > 0)
            {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, delta);
            }
            return delta;
        }

        internal override int GetQueryStringCore(int encode, StringBuilder buffer, int size)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbGetQueryString(base._ecb, encode, buffer, size);
        }

        internal override int GetQueryStringRawBytesCore(byte[] buffer, int size)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbGetQueryStringRawBytes(base._ecb, buffer, size);
        }

        public override string GetServerVariable(string name)
        {
            if (name != null)
            {
                switch (name.Length)
                {
                    case 5:
                        if (!(name == "HTTPS"))
                        {
                            break;
                        }
                        return this._basicServerVars[10];

                    case 7:
                        if (!(name == "ALL_RAW"))
                        {
                            break;
                        }
                        return this._basicServerVars[11];

                    case 9:
                        if (!(name == "AUTH_TYPE"))
                        {
                            break;
                        }
                        return this._basicServerVars[1];

                    case 10:
                        if (!(name == "LOGON_USER"))
                        {
                            if (name == "LOCAL_ADDR")
                            {
                                return this.GetAdditionalServerVar(30);
                            }
                            if (name == "CERT_FLAGS")
                            {
                                return this.GetAdditionalServerVar(15);
                            }
                            break;
                        }
                        return this._basicServerVars[0];

                    case 11:
                        if (!(name == "SERVER_NAME"))
                        {
                            if (name == "SERVER_PORT")
                            {
                                return this._basicServerVars[9];
                            }
                            if (name == "REMOTE_HOST")
                            {
                                return this.GetAdditionalServerVar(0x1f);
                            }
                            if (name == "REMOTE_PORT")
                            {
                                return this.GetAdditionalServerVar(0x20);
                            }
                            if (name == "REMOTE_ADDR")
                            {
                                return this.GetAdditionalServerVar(12);
                            }
                            if (name == "CERT_COOKIE")
                            {
                                return this.GetAdditionalServerVar(14);
                            }
                            if (name == "CERT_ISSUER")
                            {
                                return this.GetAdditionalServerVar(0x10);
                            }
                            if (!(name == "INSTANCE_ID"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(0x1c);
                        }
                        return this._basicServerVars[8];

                    case 12:
                        if (!(name == "CERT_KEYSIZE"))
                        {
                            if (!(name == "CERT_SUBJECT"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(0x16);
                        }
                        return this.GetAdditionalServerVar(0x11);

                    case 13:
                        if (!(name == "AUTH_PASSWORD"))
                        {
                            if (!(name == "HTTPS_KEYSIZE"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(0x18);
                        }
                        return this.GetAdditionalServerVar(13);

                    case 15:
                        if (!(name == "HTTP_USER_AGENT"))
                        {
                            if (name == "SERVER_PROTOCOL")
                            {
                                return this.GetAdditionalServerVar(0x21);
                            }
                            if (!(name == "SERVER_SOFTWARE"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(0x22);
                        }
                        return this.GetKnownRequestHeader(0x27);

                    case 0x11:
                        if (!(name == "CERT_SERIALNUMBER"))
                        {
                            if (!(name == "GATEWAY_INTERFACE"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(0x17);
                        }
                        return this.GetAdditionalServerVar(0x13);

                    case 0x12:
                        if (!(name == "INSTANCE_META_PATH"))
                        {
                            if (name == "CERT_SECRETKEYSIZE")
                            {
                                return this.GetAdditionalServerVar(0x12);
                            }
                            if (!(name == "CERT_SERVER_ISSUER"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(20);
                        }
                        return this.GetAdditionalServerVar(0x1d);

                    case 0x13:
                        if (!(name == "HTTPS_SECRETKEYSIZE"))
                        {
                            if (name == "CERT_SERVER_SUBJECT")
                            {
                                return this.GetAdditionalServerVar(0x15);
                            }
                            if (!(name == "HTTPS_SERVER_ISSUER"))
                            {
                                break;
                            }
                            return this.GetAdditionalServerVar(0x1a);
                        }
                        return this.GetAdditionalServerVar(0x19);

                    case 20:
                        if (!(name == "HTTPS_SERVER_SUBJECT"))
                        {
                            break;
                        }
                        return this.GetAdditionalServerVar(0x1b);
                }
            }
            return this.GetServerVariableCore(name);
        }

        protected virtual string GetServerVariableCore(string name)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return null;
            }
            string decodedString = null;
            RecyclableByteBuffer buffer = new RecyclableByteBuffer();
            int len = UnsafeNativeMethods.EcbGetServerVariable(base._ecb, name, buffer.Buffer, buffer.Buffer.Length);
            while (len < 0)
            {
                buffer.Resize(-len);
                len = UnsafeNativeMethods.EcbGetServerVariable(base._ecb, name, buffer.Buffer, buffer.Buffer.Length);
            }
            if (len > 0)
            {
                decodedString = buffer.GetDecodedString(Encoding.UTF8, len);
            }
            buffer.Dispose();
            return decodedString;
        }

        internal override IntPtr GetUserTokenCore()
        {
            if ((base._token == IntPtr.Zero) && (base._ecb != IntPtr.Zero))
            {
                base._token = UnsafeNativeMethods.EcbGetImpersonationToken(base._ecb, IntPtr.Zero);
            }
            return base._token;
        }

        internal override IntPtr GetVirtualPathTokenCore()
        {
            if ((base._token == IntPtr.Zero) && (base._ecb != IntPtr.Zero))
            {
                base._token = UnsafeNativeMethods.EcbGetVirtualPathToken(base._ecb, IntPtr.Zero);
            }
            return base._token;
        }

        internal override int IsClientConnectedCore()
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbIsClientConnected(base._ecb);
        }

        internal override int MapUrlToPathCore(string url, byte[] buffer, int size)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.EcbMapUrlToPath(base._ecb, url, buffer, size);
        }

        internal ChannelBinding HttpChannelBindingToken
        {
            get
            {
                if (this._channelBindingToken == null)
                {
                    IntPtr zero = IntPtr.Zero;
                    int tokenSize = 0;
                    int hresult = 0;
                    hresult = UnsafeNativeMethods.EcbGetChannelBindingToken(base._ecb, out zero, out tokenSize);
                    if (hresult == -2147467263)
                    {
                        throw new PlatformNotSupportedException();
                    }
                    Misc.ThrowIfFailedHr(hresult);
                    this._channelBindingToken = new System.Web.HttpChannelBindingToken(zero, tokenSize);
                }
                return this._channelBindingToken;
            }
        }
    }
}

