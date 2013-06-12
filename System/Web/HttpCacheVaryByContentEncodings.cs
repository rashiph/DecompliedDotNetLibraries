namespace System.Web
{
    using System;
    using System.Reflection;

    public sealed class HttpCacheVaryByContentEncodings
    {
        private string[] _contentEncodings;
        private bool _isModified;

        public HttpCacheVaryByContentEncodings()
        {
            this.Reset();
        }

        internal string[] GetContentEncodings()
        {
            return this._contentEncodings;
        }

        internal bool IsCacheableEncoding(string coding)
        {
            if (this._contentEncodings == null)
            {
                return true;
            }
            if (coding == null)
            {
                return true;
            }
            for (int i = 0; i < this._contentEncodings.Length; i++)
            {
                if (this._contentEncodings[i] == coding)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsModified()
        {
            return this._isModified;
        }

        internal void Reset()
        {
            this._isModified = false;
            this._contentEncodings = null;
        }

        internal void ResetFromContentEncodings(string[] contentEncodings)
        {
            this.Reset();
            if (contentEncodings != null)
            {
                this._isModified = true;
                this._contentEncodings = new string[contentEncodings.Length];
                for (int i = 0; i < contentEncodings.Length; i++)
                {
                    this._contentEncodings[i] = contentEncodings[i];
                }
            }
        }

        public bool this[string contentEncoding]
        {
            get
            {
                if (string.IsNullOrEmpty(contentEncoding))
                {
                    throw new ArgumentNullException(System.Web.SR.GetString("Parameter_NullOrEmpty", new object[] { "contentEncoding" }));
                }
                if (this._contentEncodings != null)
                {
                    for (int i = 0; i < this._contentEncodings.Length; i++)
                    {
                        if (this._contentEncodings[i] == contentEncoding)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            set
            {
                if (string.IsNullOrEmpty(contentEncoding))
                {
                    throw new ArgumentNullException(System.Web.SR.GetString("Parameter_NullOrEmpty", new object[] { "contentEncoding" }));
                }
                if (value)
                {
                    this._isModified = true;
                    if (this._contentEncodings != null)
                    {
                        string[] strArray = new string[this._contentEncodings.Length + 1];
                        for (int i = 0; i < this._contentEncodings.Length; i++)
                        {
                            strArray[i] = this._contentEncodings[i];
                        }
                        strArray[strArray.Length - 1] = contentEncoding;
                        this._contentEncodings = strArray;
                    }
                    else
                    {
                        this._contentEncodings = new string[] { contentEncoding };
                    }
                }
            }
        }
    }
}

