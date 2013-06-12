namespace System.Web
{
    using System;
    using System.Web.Util;

    [Serializable]
    internal class HttpResponseHeader
    {
        private int _knownHeaderIndex;
        private string _unknownHeader;
        private string _value;

        internal HttpResponseHeader(int knownHeaderIndex, string value) : this(knownHeaderIndex, value, HttpRuntime.EnableHeaderChecking)
        {
        }

        internal HttpResponseHeader(string unknownHeader, string value) : this(unknownHeader, value, HttpRuntime.EnableHeaderChecking)
        {
        }

        internal HttpResponseHeader(int knownHeaderIndex, string value, bool enableHeaderChecking)
        {
            this._unknownHeader = null;
            this._knownHeaderIndex = knownHeaderIndex;
            if (enableHeaderChecking)
            {
                string str;
                HttpEncoder.Current.HeaderNameValueEncode(this.Name, value, out str, out this._value);
            }
            else
            {
                this._value = value;
            }
        }

        internal HttpResponseHeader(string unknownHeader, string value, bool enableHeaderChecking)
        {
            if (enableHeaderChecking)
            {
                HttpEncoder.Current.HeaderNameValueEncode(unknownHeader, value, out this._unknownHeader, out this._value);
                this._knownHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(this._unknownHeader);
            }
            else
            {
                this._unknownHeader = unknownHeader;
                this._knownHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(this._unknownHeader);
                this._value = value;
            }
        }

        internal void Send(HttpWorkerRequest wr)
        {
            if (this._knownHeaderIndex >= 0)
            {
                wr.SendKnownResponseHeader(this._knownHeaderIndex, this._value);
            }
            else
            {
                wr.SendUnknownResponseHeader(this._unknownHeader, this._value);
            }
        }

        internal string Name
        {
            get
            {
                if (this._unknownHeader != null)
                {
                    return this._unknownHeader;
                }
                return HttpWorkerRequest.GetKnownResponseHeaderName(this._knownHeaderIndex);
            }
        }

        internal string Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

