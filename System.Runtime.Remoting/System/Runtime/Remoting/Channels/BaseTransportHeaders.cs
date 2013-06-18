namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Reflection;

    [Serializable]
    internal class BaseTransportHeaders : ITransportHeaders
    {
        private object _connectionId;
        private string _contentType;
        private object _ipAddress;
        private ITransportHeaders _otherHeaders = null;
        private string _requestUri;
        internal const int WellknownHeaderCount = 4;

        public IEnumerator GetEnumerator()
        {
            return new BaseTransportHeadersEnumerator(this);
        }

        internal IEnumerator GetOtherHeadersEnumerator()
        {
            if (this._otherHeaders == null)
            {
                return null;
            }
            return this._otherHeaders.GetEnumerator();
        }

        internal object GetValueFromHeaderIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return this._connectionId;

                case 1:
                    return this._ipAddress;

                case 2:
                    return this._requestUri;

                case 3:
                    return this._contentType;
            }
            return null;
        }

        internal string MapHeaderIndexToName(int index)
        {
            switch (index)
            {
                case 0:
                    return "__ConnectionId";

                case 1:
                    return "__IPAddress";

                case 2:
                    return "__RequestUri";

                case 3:
                    return "Content-Type";
            }
            return null;
        }

        internal int MapHeaderNameToIndex(string headerName)
        {
            if (string.Compare(headerName, "__ConnectionId", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return 0;
            }
            if (string.Compare(headerName, "__IPAddress", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return 1;
            }
            if (string.Compare(headerName, "__RequestUri", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return 2;
            }
            if (string.Compare(headerName, "Content-Type", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return 3;
            }
            return -1;
        }

        internal void SetValueFromHeaderIndex(int index, object value)
        {
            switch (index)
            {
                case 0:
                    this._connectionId = value;
                    return;

                case 1:
                    this._ipAddress = value;
                    return;

                case 2:
                    this._requestUri = (string) value;
                    return;

                case 3:
                    this._contentType = (string) value;
                    return;
            }
        }

        public object ConnectionId
        {
            set
            {
                this._connectionId = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                this._contentType = value;
            }
        }

        public System.Net.IPAddress IPAddress
        {
            set
            {
                this._ipAddress = value;
            }
        }

        public object this[object key]
        {
            get
            {
                string headerName = key as string;
                if (headerName != null)
                {
                    int index = this.MapHeaderNameToIndex(headerName);
                    if (index != -1)
                    {
                        return this.GetValueFromHeaderIndex(index);
                    }
                }
                if (this._otherHeaders != null)
                {
                    return this._otherHeaders[key];
                }
                return null;
            }
            set
            {
                bool flag = false;
                string headerName = key as string;
                if (headerName != null)
                {
                    int index = this.MapHeaderNameToIndex(headerName);
                    if (index != -1)
                    {
                        this.SetValueFromHeaderIndex(index, value);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    if (this._otherHeaders == null)
                    {
                        this._otherHeaders = new TransportHeaders();
                    }
                    this._otherHeaders[key] = value;
                }
            }
        }

        public string RequestUri
        {
            get
            {
                return this._requestUri;
            }
            set
            {
                this._requestUri = value;
            }
        }
    }
}

