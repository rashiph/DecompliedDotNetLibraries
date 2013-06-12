namespace System.Web.Management
{
    using System;
    using System.Security.Principal;
    using System.Web;

    public sealed class WebRequestInformation
    {
        private string _accountName;
        private IPrincipal _iprincipal;
        private string _requestPath;
        private string _requestUrl;
        private string _userHostAddress;

        internal WebRequestInformation()
        {
            InternalSecurityPermissions.ControlPrincipal.Assert();
            HttpContext current = HttpContext.Current;
            HttpRequest request = null;
            if (current != null)
            {
                bool hideRequestResponse = current.HideRequestResponse;
                current.HideRequestResponse = false;
                request = current.Request;
                current.HideRequestResponse = hideRequestResponse;
                this._iprincipal = current.User;
            }
            else
            {
                this._iprincipal = null;
            }
            if (request == null)
            {
                this._requestUrl = string.Empty;
                this._requestPath = string.Empty;
                this._userHostAddress = string.Empty;
            }
            else
            {
                this._requestUrl = request.UrlInternal;
                this._requestPath = request.Path;
                this._userHostAddress = request.UserHostAddress;
            }
            this._accountName = WindowsIdentity.GetCurrent().Name;
        }

        public void FormatToString(WebEventFormatter formatter)
        {
            string name;
            string authenticationType;
            bool isAuthenticated;
            if (this.Principal == null)
            {
                name = string.Empty;
                authenticationType = string.Empty;
                isAuthenticated = false;
            }
            else
            {
                IIdentity identity = this.Principal.Identity;
                name = identity.Name;
                isAuthenticated = identity.IsAuthenticated;
                authenticationType = identity.AuthenticationType;
            }
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_request_url", this.RequestUrl));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_request_path", this.RequestPath));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_user_host_address", this.UserHostAddress));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_user", name));
            if (isAuthenticated)
            {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_is_authenticated"));
            }
            else
            {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_is_not_authenticated"));
            }
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_authentication_type", authenticationType));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_thread_account_name", this.ThreadAccountName));
        }

        public IPrincipal Principal
        {
            get
            {
                return this._iprincipal;
            }
        }

        public string RequestPath
        {
            get
            {
                return this._requestPath;
            }
        }

        public string RequestUrl
        {
            get
            {
                return this._requestUrl;
            }
        }

        public string ThreadAccountName
        {
            get
            {
                return this._accountName;
            }
        }

        public string UserHostAddress
        {
            get
            {
                return this._userHostAddress;
            }
        }
    }
}

