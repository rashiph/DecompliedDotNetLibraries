namespace System.Data.Odbc
{
    using System;

    [Serializable]
    public sealed class OdbcError
    {
        internal string _message;
        internal int _nativeerror;
        internal string _source;
        internal string _state;

        internal OdbcError(string source, string message, string state, int nativeerror)
        {
            this._source = source;
            this._message = message;
            this._state = state;
            this._nativeerror = nativeerror;
        }

        internal void SetSource(string Source)
        {
            this._source = Source;
        }

        public override string ToString()
        {
            return this.Message;
        }

        public string Message
        {
            get
            {
                if (this._message == null)
                {
                    return string.Empty;
                }
                return this._message;
            }
        }

        public int NativeError
        {
            get
            {
                return this._nativeerror;
            }
        }

        public string Source
        {
            get
            {
                if (this._source == null)
                {
                    return string.Empty;
                }
                return this._source;
            }
        }

        public string SQLState
        {
            get
            {
                return this._state;
            }
        }
    }
}

