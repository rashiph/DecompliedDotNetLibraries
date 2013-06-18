namespace System.Web.UI
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web;

    [Serializable]
    public sealed class ViewStateException : Exception, ISerializable
    {
        private const string _format = "\r\n\tClient IP: {0}\r\n\tPort: {1}\r\n\tReferer: {2}\r\n\tPath: {3}\r\n\tUser-Agent: {4}\r\n\tViewState: {5}";
        private bool _isConnected;
        internal bool _macValidationError;
        private string _message;
        private string _path;
        private string _persistedState;
        private string _referer;
        private string _remoteAddr;
        private string _remotePort;
        private string _userAgent;

        public ViewStateException()
        {
            this._isConnected = true;
        }

        private ViewStateException(string message)
        {
            this._isConnected = true;
        }

        private ViewStateException(Exception innerException, string persistedState) : base(null, innerException)
        {
            this._isConnected = true;
            this.Initialize(persistedState);
        }

        private ViewStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._isConnected = true;
        }

        private ViewStateException(string message, Exception e)
        {
            this._isConnected = true;
        }

        private static string GetCorrectErrorPageMessage(ViewStateException vse, string message)
        {
            if (!vse.IsConnected)
            {
                return System.Web.SR.GetString("ViewState_ClientDisconnected");
            }
            return System.Web.SR.GetString(message);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        private void Initialize(string persistedState)
        {
            this._persistedState = persistedState;
            HttpContext current = HttpContext.Current;
            HttpRequest request = (current != null) ? current.Request : null;
            HttpResponse response = (current != null) ? current.Response : null;
            if (((request == null) || (response == null)) || !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low))
            {
                this._message = this.ShortMessage;
            }
            else
            {
                this._isConnected = response.IsClientConnected;
                this._remoteAddr = request.ServerVariables["REMOTE_ADDR"];
                this._remotePort = request.ServerVariables["REMOTE_PORT"];
                this._userAgent = request.ServerVariables["HTTP_USER_AGENT"];
                this._referer = request.ServerVariables["HTTP_REFERER"];
                this._path = request.ServerVariables["PATH_INFO"];
                string str = string.Format(CultureInfo.InvariantCulture, "\r\n\tClient IP: {0}\r\n\tPort: {1}\r\n\tReferer: {2}\r\n\tPath: {3}\r\n\tUser-Agent: {4}\r\n\tViewState: {5}", new object[] { this._remoteAddr, this._remotePort, this._referer, this._path, this._userAgent, this._persistedState });
                this._message = System.Web.SR.GetString("ViewState_InvalidViewStatePlus", new object[] { str });
            }
        }

        private static void ThrowError(Exception inner, string persistedState, string errorPageMessage, bool macValidationError)
        {
            ViewStateException innerException = new ViewStateException(inner, persistedState) {
                _macValidationError = macValidationError
            };
            HttpException e = new HttpException(GetCorrectErrorPageMessage(innerException, errorPageMessage), innerException);
            e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
            throw e;
        }

        internal static void ThrowMacValidationError(Exception inner, string persistedState)
        {
            ThrowError(inner, persistedState, "ViewState_AuthenticationFailed", true);
        }

        internal static void ThrowViewStateError(Exception inner, string persistedState)
        {
            ThrowError(inner, persistedState, "Invalid_ControlState", false);
        }

        public bool IsConnected
        {
            get
            {
                return this._isConnected;
            }
        }

        public override string Message
        {
            get
            {
                return this._message;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        public string PersistedState
        {
            get
            {
                return this._persistedState;
            }
        }

        public string Referer
        {
            get
            {
                return this._referer;
            }
        }

        public string RemoteAddress
        {
            get
            {
                return this._remoteAddr;
            }
        }

        public string RemotePort
        {
            get
            {
                return this._remotePort;
            }
        }

        internal string ShortMessage
        {
            get
            {
                return "ViewState_InvalidViewState";
            }
        }

        public string UserAgent
        {
            get
            {
                return this._userAgent;
            }
        }
    }
}

