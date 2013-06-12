namespace System.Web
{
    using System;
    using System.Reflection;
    using System.Text;

    public sealed class HttpCacheVaryByHeaders
    {
        private HttpDictionary _headers;
        private bool _isModified;
        private bool _varyStar;

        public HttpCacheVaryByHeaders()
        {
            this.Reset();
        }

        internal string[] GetHeaders()
        {
            string[] strArray = null;
            if (this._varyStar)
            {
                return new string[] { "*" };
            }
            if (this._headers != null)
            {
                int num;
                int size = this._headers.Size;
                int num3 = 0;
                for (num = 0; num < size; num++)
                {
                    if (this._headers.GetValue(num) != null)
                    {
                        num3++;
                    }
                }
                if (num3 <= 0)
                {
                    return strArray;
                }
                strArray = new string[num3];
                int index = 0;
                for (num = 0; num < size; num++)
                {
                    object obj2 = this._headers.GetValue(num);
                    if (obj2 != null)
                    {
                        strArray[index] = (string) obj2;
                        index++;
                    }
                }
            }
            return strArray;
        }

        internal bool GetVaryByUnspecifiedParameters()
        {
            return this._varyStar;
        }

        internal bool IsModified()
        {
            return this._isModified;
        }

        internal void Reset()
        {
            this._isModified = false;
            this._varyStar = false;
            this._headers = null;
        }

        internal void ResetFromHeaders(string[] headers)
        {
            if (headers == null)
            {
                this._isModified = false;
                this._varyStar = false;
                this._headers = null;
            }
            else
            {
                this._isModified = true;
                if (headers[0].Equals("*"))
                {
                    this._varyStar = true;
                    this._headers = null;
                }
                else
                {
                    this._varyStar = false;
                    this._headers = new HttpDictionary();
                    int index = 0;
                    int length = headers.Length;
                    while (index < length)
                    {
                        this._headers.SetValue(headers[index], headers[index]);
                        index++;
                    }
                }
            }
        }

        internal string ToHeaderString()
        {
            if (this._varyStar)
            {
                return "*";
            }
            if (this._headers != null)
            {
                StringBuilder s = new StringBuilder();
                int index = 0;
                int size = this._headers.Size;
                while (index < size)
                {
                    object obj2 = this._headers.GetValue(index);
                    if (obj2 != null)
                    {
                        HttpCachePolicy.AppendValueToHeader(s, (string) obj2);
                    }
                    index++;
                }
                if (s.Length > 0)
                {
                    return s.ToString();
                }
            }
            return null;
        }

        public void VaryByUnspecifiedParameters()
        {
            this._isModified = true;
            this._varyStar = true;
            this._headers = null;
        }

        public bool AcceptTypes
        {
            get
            {
                return this["Accept"];
            }
            set
            {
                this._isModified = true;
                this["Accept"] = value;
            }
        }

        public bool this[string header]
        {
            get
            {
                if (header == null)
                {
                    throw new ArgumentNullException("header");
                }
                if (header.Equals("*"))
                {
                    return this._varyStar;
                }
                return ((this._headers != null) && (this._headers.GetValue(header) != null));
            }
            set
            {
                if (header == null)
                {
                    throw new ArgumentNullException("header");
                }
                if (value)
                {
                    this._isModified = true;
                    if (header.Equals("*"))
                    {
                        this.VaryByUnspecifiedParameters();
                    }
                    else if (!this._varyStar)
                    {
                        if (this._headers == null)
                        {
                            this._headers = new HttpDictionary();
                        }
                        this._headers.SetValue(header, header);
                    }
                }
            }
        }

        public bool UserAgent
        {
            get
            {
                return this["User-Agent"];
            }
            set
            {
                this._isModified = true;
                this["User-Agent"] = value;
            }
        }

        public bool UserCharSet
        {
            get
            {
                return this["Accept-Charset"];
            }
            set
            {
                this._isModified = true;
                this["Accept-Charset"] = value;
            }
        }

        public bool UserLanguage
        {
            get
            {
                return this["Accept-Language"];
            }
            set
            {
                this._isModified = true;
                this["Accept-Language"] = value;
            }
        }
    }
}

