namespace System.Web
{
    using System;
    using System.Reflection;

    public sealed class HttpCacheVaryByParams
    {
        private int _ignoreParams;
        private bool _isModified;
        private HttpDictionary _parameters;
        private bool _paramsStar;

        public HttpCacheVaryByParams()
        {
            this.Reset();
        }

        internal bool AcceptsParams()
        {
            if ((this._ignoreParams != 1) && !this._paramsStar)
            {
                return (this._parameters != null);
            }
            return true;
        }

        internal string[] GetParams()
        {
            string[] strArray = null;
            if (this._ignoreParams == 1)
            {
                return new string[] { string.Empty };
            }
            if (this._paramsStar)
            {
                return new string[] { "*" };
            }
            if (this._parameters != null)
            {
                int num;
                int size = this._parameters.Size;
                int num3 = 0;
                for (num = 0; num < size; num++)
                {
                    if (this._parameters.GetValue(num) != null)
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
                    object obj2 = this._parameters.GetValue(num);
                    if (obj2 != null)
                    {
                        strArray[index] = (string) obj2;
                        index++;
                    }
                }
            }
            return strArray;
        }

        internal bool IsModified()
        {
            return this._isModified;
        }

        internal void Reset()
        {
            this._isModified = false;
            this._paramsStar = false;
            this._parameters = null;
            this._ignoreParams = -1;
        }

        internal void ResetFromParams(string[] parameters)
        {
            this.Reset();
            if (parameters != null)
            {
                this._isModified = true;
                if (parameters[0].Length == 0)
                {
                    this.IgnoreParams = true;
                }
                else if (parameters[0].Equals("*"))
                {
                    this._paramsStar = true;
                }
                else
                {
                    this._parameters = new HttpDictionary();
                    int index = 0;
                    int length = parameters.Length;
                    while (index < length)
                    {
                        this._parameters.SetValue(parameters[index], parameters[index]);
                        index++;
                    }
                }
            }
        }

        public bool IgnoreParams
        {
            get
            {
                return (this._ignoreParams == 1);
            }
            set
            {
                if ((!this._paramsStar && (this._parameters == null)) && ((this._ignoreParams == -1) || (this._ignoreParams == 1)))
                {
                    this._ignoreParams = value ? 1 : 0;
                    this._isModified = true;
                }
            }
        }

        internal bool IsVaryByStar
        {
            get
            {
                return this._paramsStar;
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
                if (header.Length == 0)
                {
                    return (this._ignoreParams == 1);
                }
                return (this._paramsStar || ((this._parameters != null) && (this._parameters.GetValue(header) != null)));
            }
            set
            {
                if (header == null)
                {
                    throw new ArgumentNullException("header");
                }
                if (header.Length == 0)
                {
                    this.IgnoreParams = value;
                }
                else if (value)
                {
                    this._isModified = true;
                    this._ignoreParams = 0;
                    if (header.Equals("*"))
                    {
                        this._paramsStar = true;
                        this._parameters = null;
                    }
                    else if (!this._paramsStar)
                    {
                        if (this._parameters == null)
                        {
                            this._parameters = new HttpDictionary();
                        }
                        this._parameters.SetValue(header, header);
                    }
                }
            }
        }
    }
}

