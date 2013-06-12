namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;

    public sealed class HttpListenerContext
    {
        private HttpListener m_Listener;
        private string m_MutualAuthentication;
        private bool m_PromoteCookiesToRfc2965;
        private HttpListenerRequest m_Request;
        private HttpListenerResponse m_Response;
        private IPrincipal m_User;
        internal const string NTLM = "NTLM";

        internal unsafe HttpListenerContext(HttpListener httpListener, RequestContextBase memoryBlob)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpListener#" + ValidationHelper.HashString(httpListener) + " requestBlob=" + ValidationHelper.HashString((IntPtr) memoryBlob.RequestBlob));
            }
            this.m_Listener = httpListener;
            this.m_Request = new HttpListenerRequest(this, memoryBlob);
        }

        internal void Abort()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Abort", "");
            }
            CancelRequest(this.RequestQueueHandle, this.m_Request.RequestId);
            try
            {
                this.m_Request.Close();
            }
            finally
            {
                IDisposable disposable = (this.m_User == null) ? null : (this.m_User.Identity as IDisposable);
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "Abort", "");
            }
        }

        internal static unsafe void CancelRequest(CriticalHandle requestQueueHandle, ulong requestId)
        {
            UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK http_data_chunk;
            http_data_chunk = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK {
                DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory,
                pBuffer = (byte*) &http_data_chunk
            };
            UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(requestQueueHandle, requestId, 1, 1, &http_data_chunk, null, SafeLocalFree.Zero, 0, null, null);
        }

        internal void Close()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Close()", "");
            }
            try
            {
                if (this.m_Response != null)
                {
                    this.m_Response.Close();
                }
            }
            finally
            {
                try
                {
                    this.m_Request.Close();
                }
                finally
                {
                    IDisposable disposable = (this.m_User == null) ? null : (this.m_User.Identity as IDisposable);
                    if (((disposable != null) && (this.m_User.Identity.AuthenticationType != "NTLM")) && !this.m_Listener.UnsafeConnectionNtlmAuthentication)
                    {
                        disposable.Dispose();
                    }
                }
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "Close", "");
            }
        }

        internal void EnsureBoundHandle()
        {
            this.m_Listener.EnsureBoundHandle();
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_VERB GetKnownMethod()
        {
            return UnsafeNclNativeMethods.HttpApi.GetKnownVerb(this.Request.RequestBuffer, this.Request.OriginalBlobAddress);
        }

        internal void SetIdentity(IPrincipal principal, string mutualAuthentication)
        {
            this.m_MutualAuthentication = mutualAuthentication;
            this.m_User = principal;
        }

        internal HttpListener Listener
        {
            get
            {
                return this.m_Listener;
            }
        }

        internal string MutualAuthentication
        {
            get
            {
                return this.m_MutualAuthentication;
            }
        }

        internal bool PromoteCookiesToRfc2965
        {
            get
            {
                return this.m_PromoteCookiesToRfc2965;
            }
        }

        public HttpListenerRequest Request
        {
            get
            {
                return this.m_Request;
            }
        }

        internal ulong RequestId
        {
            get
            {
                return this.Request.RequestId;
            }
        }

        internal CriticalHandle RequestQueueHandle
        {
            get
            {
                return this.m_Listener.RequestQueueHandle;
            }
        }

        public HttpListenerResponse Response
        {
            get
            {
                if (Logging.On)
                {
                    Logging.Enter(Logging.HttpListener, this, "Response", "");
                }
                if (this.m_Response == null)
                {
                    this.m_Response = new HttpListenerResponse(this);
                }
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "Response", "");
                }
                return this.m_Response;
            }
        }

        public IPrincipal User
        {
            get
            {
                if (this.m_User is WindowsPrincipal)
                {
                    new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Demand();
                }
                return this.m_User;
            }
        }
    }
}

