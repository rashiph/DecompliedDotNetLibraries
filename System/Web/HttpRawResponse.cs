namespace System.Web
{
    using System;
    using System.Collections;

    internal class HttpRawResponse
    {
        private ArrayList _buffers;
        private bool _hasSubstBlocks;
        private ArrayList _headers;
        private int _statusCode;
        private string _statusDescr;

        internal HttpRawResponse(int statusCode, string statusDescription, ArrayList headers, ArrayList buffers, bool hasSubstBlocks)
        {
            this._statusCode = statusCode;
            this._statusDescr = statusDescription;
            this._headers = headers;
            this._buffers = buffers;
            this._hasSubstBlocks = hasSubstBlocks;
        }

        internal ArrayList Buffers
        {
            get
            {
                return this._buffers;
            }
        }

        internal bool HasSubstBlocks
        {
            get
            {
                return this._hasSubstBlocks;
            }
        }

        internal ArrayList Headers
        {
            get
            {
                return this._headers;
            }
        }

        internal int StatusCode
        {
            get
            {
                return this._statusCode;
            }
        }

        internal string StatusDescription
        {
            get
            {
                return this._statusDescr;
            }
        }
    }
}

