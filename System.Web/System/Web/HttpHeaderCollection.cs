namespace System.Web
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web.Hosting;
    using System.Web.Util;

    [Serializable]
    internal class HttpHeaderCollection : HttpValueCollection
    {
        private IIS7WorkerRequest _iis7WorkerRequest;
        private HttpRequest _request;
        private HttpResponse _response;

        internal HttpHeaderCollection(HttpWorkerRequest wr, HttpRequest request, int capacity) : base(capacity)
        {
            this._iis7WorkerRequest = wr as IIS7WorkerRequest;
            this._request = request;
        }

        internal HttpHeaderCollection(HttpWorkerRequest wr, HttpResponse response, int capacity) : base(capacity)
        {
            this._iis7WorkerRequest = wr as IIS7WorkerRequest;
            this._response = response;
        }

        public override void Add(string name, string value)
        {
            if (this._iis7WorkerRequest == null)
            {
                throw new PlatformNotSupportedException();
            }
            this.SetHeader(name, value, false);
        }

        public override void Clear()
        {
            throw new NotSupportedException();
        }

        internal void ClearInternal()
        {
            if (this._request != null)
            {
                throw new NotSupportedException();
            }
            base.Clear();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.SetType(typeof(HttpValueCollection));
        }

        public override void Remove(string name)
        {
            if (this._iis7WorkerRequest == null)
            {
                throw new PlatformNotSupportedException();
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this._request != null)
            {
                this._iis7WorkerRequest.SetRequestHeader(name, null, false);
            }
            else
            {
                this._iis7WorkerRequest.SetResponseHeader(name, null, false);
            }
            base.Remove(name);
            if (this._request != null)
            {
                HttpServerVarsCollection serverVariables = this._request.ServerVariables as HttpServerVarsCollection;
                if (serverVariables != null)
                {
                    serverVariables.SynchronizeServerVariable("HTTP_" + name.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_'), null);
                }
                this._request.InvalidateParams();
            }
        }

        public override void Set(string name, string value)
        {
            if (this._iis7WorkerRequest == null)
            {
                throw new PlatformNotSupportedException();
            }
            this.SetHeader(name, value, true);
        }

        internal void SetHeader(string name, string value, bool replace)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this._request != null)
            {
                this._iis7WorkerRequest.SetRequestHeader(name, value, replace);
            }
            else
            {
                if (this._response.HeadersWritten)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_append_header_after_headers_sent"));
                }
                string encodedHeaderName = name;
                string encodedHeaderValue = value;
                if (HttpRuntime.EnableHeaderChecking)
                {
                    HttpEncoder.Current.HeaderNameValueEncode(name, value, out encodedHeaderName, out encodedHeaderValue);
                }
                this._iis7WorkerRequest.SetHeaderEncoding(this._response.HeaderEncoding);
                this._iis7WorkerRequest.SetResponseHeader(encodedHeaderName, encodedHeaderValue, replace);
                if (this._response.HasCachePolicy && StringUtil.EqualsIgnoreCase("Set-Cookie", name))
                {
                    this._response.Cache.SetHasSetCookieHeader();
                }
            }
            if (replace)
            {
                base.Set(name, value);
            }
            else
            {
                base.Add(name, value);
            }
            if (this._request != null)
            {
                string str3 = replace ? value : base.Get(name);
                HttpServerVarsCollection serverVariables = this._request.ServerVariables as HttpServerVarsCollection;
                if (serverVariables != null)
                {
                    serverVariables.SynchronizeServerVariable("HTTP_" + name.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_'), str3);
                }
                this._request.InvalidateParams();
            }
        }

        internal void SynchronizeHeader(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value != null)
            {
                base.Set(name, value);
            }
            else
            {
                base.Remove(name);
            }
            if (this._request != null)
            {
                this._request.InvalidateParams();
            }
        }
    }
}

