namespace System.Web
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Util;

    internal class HttpServerVarsCollection : HttpValueCollection
    {
        private IIS7WorkerRequest _iis7workerRequest;
        private bool _populated;
        private HttpRequest _request;

        internal HttpServerVarsCollection(HttpWorkerRequest wr, HttpRequest request) : base(0x3b)
        {
            this._iis7workerRequest = wr as IIS7WorkerRequest;
            this._request = request;
            this._populated = false;
        }

        public override void Add(string name, string value)
        {
            throw new NotSupportedException();
        }

        internal void AddDynamic(string name, DynamicServerVariable var)
        {
            base.InvalidateCachedArrays();
            base.BaseAdd(name, new HttpServerVarsCollectionEntry(name, var));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal void AddStatic(string name, string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            base.InvalidateCachedArrays();
            base.BaseAdd(name, new HttpServerVarsCollectionEntry(name, value));
        }

        public override void Clear()
        {
            throw new NotSupportedException();
        }

        internal void Dispose()
        {
            this._request = null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override string Get(int index)
        {
            this.Populate();
            return this.GetServerVar(base.BaseGet(index));
        }

        public override string Get(string name)
        {
            if (!this._populated)
            {
                string simpleServerVar = this.GetSimpleServerVar(name);
                if (simpleServerVar != null)
                {
                    return simpleServerVar;
                }
                this.Populate();
            }
            if (this._iis7workerRequest == null)
            {
                return this.GetServerVar(base.BaseGet(name));
            }
            string serverVar = this.GetServerVar(base.BaseGet(name));
            if (string.IsNullOrEmpty(serverVar))
            {
                serverVar = this._request.FetchServerVariable(name);
            }
            return serverVar;
        }

        public override IEnumerator GetEnumerator()
        {
            this.Populate();
            return base.GetEnumerator();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override string GetKey(int index)
        {
            this.Populate();
            return base.GetKey(index);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new SerializationException();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private string GetServerVar(object e)
        {
            HttpServerVarsCollectionEntry entry = (HttpServerVarsCollectionEntry) e;
            if (entry == null)
            {
                return null;
            }
            return entry.GetValue(this._request);
        }

        private string GetSimpleServerVar(string name)
        {
            if (((name == null) || (name.Length <= 1)) || (this._request == null))
            {
                goto Label_019C;
            }
            char ch = name[0];
            if (ch <= 'S')
            {
                switch (ch)
                {
                    case 'P':
                        goto Label_00E9;

                    case 'Q':
                        goto Label_00CD;

                    case 'R':
                        goto Label_011E;

                    case 'S':
                        goto Label_0183;

                    case 'H':
                        goto Label_00B1;

                    case 'A':
                        goto Label_007A;
                }
                goto Label_019C;
            }
            if (ch != 'a')
            {
                switch (ch)
                {
                    case 'p':
                        goto Label_00E9;

                    case 'q':
                        goto Label_00CD;

                    case 'r':
                        goto Label_011E;

                    case 's':
                        goto Label_0183;

                    case 'h':
                        goto Label_00B1;
                }
                goto Label_019C;
            }
        Label_007A:
            if (StringUtil.EqualsIgnoreCase(name, "AUTH_TYPE"))
            {
                return this._request.CalcDynamicServerVariable(DynamicServerVariable.AUTH_TYPE);
            }
            if (!StringUtil.EqualsIgnoreCase(name, "AUTH_USER"))
            {
                goto Label_019C;
            }
            return this._request.CalcDynamicServerVariable(DynamicServerVariable.AUTH_USER);
        Label_00B1:
            if (!StringUtil.EqualsIgnoreCase(name, "HTTP_USER_AGENT"))
            {
                goto Label_019C;
            }
            return this._request.UserAgent;
        Label_00CD:
            if (!StringUtil.EqualsIgnoreCase(name, "QUERY_STRING"))
            {
                goto Label_019C;
            }
            return this._request.QueryStringText;
        Label_00E9:
            if (StringUtil.EqualsIgnoreCase(name, "PATH_INFO"))
            {
                return this._request.Path;
            }
            if (!StringUtil.EqualsIgnoreCase(name, "PATH_TRANSLATED"))
            {
                goto Label_019C;
            }
            return this._request.PhysicalPath;
        Label_011E:
            if (StringUtil.EqualsIgnoreCase(name, "REQUEST_METHOD"))
            {
                return this._request.HttpMethod;
            }
            if (StringUtil.EqualsIgnoreCase(name, "REMOTE_USER"))
            {
                return this._request.CalcDynamicServerVariable(DynamicServerVariable.AUTH_USER);
            }
            if (StringUtil.EqualsIgnoreCase(name, "REMOTE_HOST"))
            {
                return this._request.UserHostName;
            }
            if (!StringUtil.EqualsIgnoreCase(name, "REMOTE_ADDRESS"))
            {
                goto Label_019C;
            }
            return this._request.UserHostAddress;
        Label_0183:
            if (StringUtil.EqualsIgnoreCase(name, "SCRIPT_NAME"))
            {
                return this._request.FilePath;
            }
        Label_019C:
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override string[] GetValues(int index)
        {
            string str = this.Get(index);
            if (str == null)
            {
                return null;
            }
            return new string[] { str };
        }

        public override string[] GetValues(string name)
        {
            string str = this.Get(name);
            if (str == null)
            {
                return null;
            }
            return new string[] { str };
        }

        private void Populate()
        {
            if (!this._populated)
            {
                if (this._request != null)
                {
                    base.MakeReadWrite();
                    this._request.FillInServerVariablesCollection();
                    if (this._iis7workerRequest == null)
                    {
                        base.MakeReadOnly();
                    }
                }
                this._populated = true;
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        public override void Remove(string name)
        {
            if (this._iis7workerRequest == null)
            {
                throw new PlatformNotSupportedException();
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.RemoveNoDemand(name);
        }

        internal void RemoveNoDemand(string name)
        {
            this._iis7workerRequest.SetServerVariable(name, null);
            base.Remove(name);
            this.SynchronizeHeader(name, null);
            this._request.InvalidateParams();
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        public override void Set(string name, string value)
        {
            if (this._iis7workerRequest == null)
            {
                throw new PlatformNotSupportedException();
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.SetNoDemand(name, value);
        }

        internal void SetNoDemand(string name, string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            this._iis7workerRequest.SetServerVariable(name, value);
            this.SetServerVariableManagedOnly(name, value);
            this.SynchronizeHeader(name, value);
            this._request.InvalidateParams();
        }

        private void SetServerVariableManagedOnly(string name, string value)
        {
            this.Populate();
            HttpServerVarsCollectionEntry entry = (HttpServerVarsCollectionEntry) base.BaseGet(name);
            if ((entry != null) && entry.IsDynamic)
            {
                throw new HttpException(System.Web.SR.GetString("Server_variable_cannot_be_modified"));
            }
            base.InvalidateCachedArrays();
            base.BaseSet(name, new HttpServerVarsCollectionEntry(name, value));
        }

        private void SynchronizeHeader(string name, string value)
        {
            if (StringUtil.StringStartsWith(name, "HTTP_"))
            {
                string header = name.Substring("HTTP_".Length).Replace('_', '-');
                int knownRequestHeaderIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(header);
                if (knownRequestHeaderIndex > -1)
                {
                    header = HttpWorkerRequest.GetKnownRequestHeaderName(knownRequestHeaderIndex);
                }
                HttpHeaderCollection headers = this._request.Headers as HttpHeaderCollection;
                if (headers != null)
                {
                    headers.SynchronizeHeader(header, value);
                }
            }
        }

        internal void SynchronizeServerVariable(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value != null)
            {
                this.SetServerVariableManagedOnly(name, value);
            }
            else
            {
                base.Remove(name);
            }
            this._request.InvalidateParams();
        }

        internal override string ToString(bool urlencoded)
        {
            this.Populate();
            StringBuilder builder = new StringBuilder();
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    builder.Append('&');
                }
                string key = this.GetKey(i);
                if (urlencoded)
                {
                    key = HttpUtility.UrlEncodeUnicode(key);
                }
                builder.Append(key);
                builder.Append('=');
                string str = this.Get(i);
                if (urlencoded)
                {
                    str = HttpUtility.UrlEncodeUnicode(str);
                }
                builder.Append(str);
            }
            return builder.ToString();
        }

        public override string[] AllKeys
        {
            get
            {
                this.Populate();
                return base.AllKeys;
            }
        }

        public override int Count
        {
            get
            {
                this.Populate();
                return base.Count;
            }
        }
    }
}

