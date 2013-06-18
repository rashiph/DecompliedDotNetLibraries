namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    internal class ISAPIWorkerRequestOutOfProc : ISAPIWorkerRequest
    {
        private const int _numServerVars = 0x20;
        private static string[] _serverVarNames = new string[] { 
            "APPL_MD_PATH", "ALL_RAW", "AUTH_PASSWORD", "AUTH_TYPE", "CERT_COOKIE", "CERT_FLAGS", "CERT_ISSUER", "CERT_KEYSIZE", "CERT_SECRETKEYSIZE", "CERT_SERIALNUMBER", "CERT_SERVER_ISSUER", "CERT_SERVER_SUBJECT", "CERT_SUBJECT", "GATEWAY_INTERFACE", "HTTP_COOKIE", "HTTP_USER_AGENT", 
            "HTTPS", "HTTPS_KEYSIZE", "HTTPS_SECRETKEYSIZE", "HTTPS_SERVER_ISSUER", "HTTPS_SERVER_SUBJECT", "INSTANCE_ID", "INSTANCE_META_PATH", "LOCAL_ADDR", "LOGON_USER", "REMOTE_ADDR", "REMOTE_HOST", "SERVER_NAME", "SERVER_PORT", "SERVER_PROTOCOL", "SERVER_SOFTWARE", "REMOTE_PORT"
         };
        private IDictionary _serverVars;
        private bool _useBaseTime;
        private const int PM_FLUSH_THRESHOLD = 0x7c00;

        internal ISAPIWorkerRequestOutOfProc(IntPtr ecb) : base(ecb)
        {
            UnsafeNativeMethods.PMGetTraceContextId(ecb, out this._traceId);
        }

        internal override int AppendLogParameterCore(string logParam)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMAppendLogParameter(base._ecb, logParam);
        }

        internal override int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte[] bufIn, byte[] bufOut)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMCallISAPI(base._ecb, iFunction, bufIn, bufIn.Length, bufOut, bufOut.Length);
        }

        internal override int CloseConnectionCore()
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMCloseConnection(base._ecb);
        }

        internal override void FlushCore(byte[] status, byte[] header, int keepConnected, int totalBodySize, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int doneWithSession, int finalStatus, out bool async)
        {
            async = false;
            if (base._ecb != IntPtr.Zero)
            {
                if (numBodyFragments > 1)
                {
                    int num3;
                    for (int i = 0; i < numBodyFragments; i = num3)
                    {
                        bool flag = i == 0;
                        int num2 = bodyFragmentLengths[i];
                        bool flag2 = bodyFragmentLengths[i] < 0;
                        num3 = i + 1;
                        if (!flag2)
                        {
                            while (((num3 < numBodyFragments) && ((num2 + bodyFragmentLengths[num3]) < 0x7c00)) && (bodyFragmentLengths[num3] >= 0))
                            {
                                num2 += bodyFragmentLengths[num3];
                                num3++;
                            }
                        }
                        bool flag3 = num3 == numBodyFragments;
                        if (flag2)
                        {
                            num2 = -num2;
                        }
                        UnsafeNativeMethods.PMFlushCore(base._ecb, flag ? status : null, flag ? header : null, keepConnected, num2, i, num3 - i, bodyFragments, bodyFragmentLengths, flag3 ? doneWithSession : 0, flag3 ? finalStatus : 0);
                    }
                }
                else
                {
                    UnsafeNativeMethods.PMFlushCore(base._ecb, status, header, keepConnected, totalBodySize, 0, numBodyFragments, bodyFragments, bodyFragmentLengths, doneWithSession, finalStatus);
                }
            }
        }

        internal override int GetAdditionalPostedContentCore(byte[] bytes, int offset, int bufferSize)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            int delta = UnsafeNativeMethods.PMGetAdditionalPostedContent(base._ecb, bytes, offset, bufferSize);
            if (delta > 0)
            {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, delta);
            }
            return delta;
        }

        private void GetAllServerVars()
        {
            if (base._ecb != IntPtr.Zero)
            {
                RecyclableByteBuffer buffer = new RecyclableByteBuffer();
                int num = UnsafeNativeMethods.PMGetAllServerVariables(base._ecb, buffer.Buffer, buffer.Buffer.Length);
                while (num < 0)
                {
                    buffer.Resize(-num);
                    num = UnsafeNativeMethods.PMGetAllServerVariables(base._ecb, buffer.Buffer, buffer.Buffer.Length);
                }
                if (num == 0)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_retrieve_request_data"));
                }
                string[] strArray = buffer.GetDecodedTabSeparatedStrings(Encoding.Default, 0x1f, 1);
                buffer.Dispose();
                this._serverVars = new Hashtable(0x20, StringComparer.OrdinalIgnoreCase);
                this._serverVars.Add("APPL_MD_PATH", HttpRuntime.AppDomainAppIdInternal);
                for (int i = 1; i < 0x20; i++)
                {
                    this._serverVars.Add(_serverVarNames[i], strArray[i - 1]);
                }
            }
        }

        internal override int GetBasicsCore(byte[] buffer, int size, int[] contentInfo)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMGetBasics(base._ecb, buffer, size, contentInfo);
        }

        internal override int GetClientCertificateCore(byte[] buffer, int[] pInts, long[] pDates)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMGetClientCertificate(base._ecb, buffer, buffer.Length, pInts, pDates);
        }

        internal override int GetPreloadedPostedContentCore(byte[] bytes, int offset, int numBytesToRead)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            int delta = UnsafeNativeMethods.PMGetPreloadedPostedContent(base._ecb, bytes, offset, numBytesToRead);
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
            return UnsafeNativeMethods.PMGetQueryString(base._ecb, encode, buffer, size);
        }

        internal override int GetQueryStringRawBytesCore(byte[] buffer, int size)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMGetQueryStringRawBytes(base._ecb, buffer, size);
        }

        public override string GetServerVariable(string name)
        {
            if (name.Equals("PATH_TRANSLATED"))
            {
                return this.GetFilePathTranslated();
            }
            if (this._serverVars == null)
            {
                this.GetAllServerVars();
            }
            return (string) this._serverVars[name];
        }

        internal override DateTime GetStartTime()
        {
            if (!(base._ecb == IntPtr.Zero) && !this._useBaseTime)
            {
                return DateTimeUtil.FromFileTimeToUtc(UnsafeNativeMethods.PMGetStartTimeStamp(base._ecb));
            }
            return base.GetStartTime();
        }

        internal override IntPtr GetUserTokenCore()
        {
            if ((base._token == IntPtr.Zero) && (base._ecb != IntPtr.Zero))
            {
                base._token = UnsafeNativeMethods.PMGetImpersonationToken(base._ecb);
            }
            return base._token;
        }

        internal override IntPtr GetVirtualPathTokenCore()
        {
            if ((base._token == IntPtr.Zero) && (base._ecb != IntPtr.Zero))
            {
                base._token = UnsafeNativeMethods.PMGetVirtualPathToken(base._ecb);
            }
            return base._token;
        }

        internal override int IsClientConnectedCore()
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMIsClientConnected(base._ecb);
        }

        internal override int MapUrlToPathCore(string url, byte[] buffer, int size)
        {
            if (base._ecb == IntPtr.Zero)
            {
                return 0;
            }
            return UnsafeNativeMethods.PMMapUrlToPath(base._ecb, url, buffer, size);
        }

        internal override MemoryBytes PackageFile(string filename, long offset64, long length64, bool isImpersonating)
        {
            int num = Convert.ToInt32(offset64);
            int num2 = Convert.ToInt32(length64);
            byte[] bytes = BitConverter.GetBytes(num);
            byte[] src = BitConverter.GetBytes(num2);
            byte[] buffer3 = Encoding.Unicode.GetBytes(filename);
            byte[] dst = new byte[(((4 + bytes.Length) + src.Length) + buffer3.Length) + 2];
            if (isImpersonating)
            {
                dst[0] = 0x31;
            }
            else
            {
                dst[0] = 0x30;
            }
            Buffer.BlockCopy(bytes, 0, dst, 4, bytes.Length);
            Buffer.BlockCopy(src, 0, dst, 4 + bytes.Length, src.Length);
            Buffer.BlockCopy(buffer3, 0, dst, (4 + bytes.Length) + src.Length, buffer3.Length);
            return new MemoryBytes(dst, dst.Length, true, (long) num2);
        }

        internal override void ResetStartTime()
        {
            base.ResetStartTime();
            this._useBaseTime = true;
        }

        internal override void SendEmptyResponse()
        {
            if (base._ecb != IntPtr.Zero)
            {
                UnsafeNativeMethods.PMEmptyResponse(base._ecb);
            }
        }
    }
}

