namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime;
    using System.Text;

    public sealed class CommaDelimitedStringCollection : StringCollection
    {
        private bool _Modified = false;
        private string _OriginalString;
        private bool _ReadOnly = false;

        public CommaDelimitedStringCollection()
        {
            this._OriginalString = this.ToString();
        }

        public void Add(string value)
        {
            this.ThrowIfReadOnly();
            this.ThrowIfContainsDelimiter(value);
            this._Modified = true;
            base.Add(value.Trim());
        }

        public void AddRange(string[] range)
        {
            this.ThrowIfReadOnly();
            this._Modified = true;
            foreach (string str in range)
            {
                this.ThrowIfContainsDelimiter(str);
                base.Add(str.Trim());
            }
        }

        public void Clear()
        {
            this.ThrowIfReadOnly();
            this._Modified = true;
            base.Clear();
        }

        public CommaDelimitedStringCollection Clone()
        {
            CommaDelimitedStringCollection strings = new CommaDelimitedStringCollection();
            foreach (string str in this)
            {
                strings.Add(str);
            }
            strings._Modified = false;
            strings._ReadOnly = this._ReadOnly;
            strings._OriginalString = this._OriginalString;
            return strings;
        }

        internal void FromString(string list)
        {
            char[] separator = new char[] { ',' };
            if (list != null)
            {
                foreach (string str in list.Split(separator))
                {
                    if (str.Trim().Length != 0)
                    {
                        this.Add(str.Trim());
                    }
                }
            }
            this._OriginalString = this.ToString();
            this._ReadOnly = false;
            this._Modified = false;
        }

        public void Insert(int index, string value)
        {
            this.ThrowIfReadOnly();
            this.ThrowIfContainsDelimiter(value);
            this._Modified = true;
            base.Insert(index, value.Trim());
        }

        public void Remove(string value)
        {
            this.ThrowIfReadOnly();
            this.ThrowIfContainsDelimiter(value);
            this._Modified = true;
            base.Remove(value.Trim());
        }

        public void SetReadOnly()
        {
            this._ReadOnly = true;
        }

        private void ThrowIfContainsDelimiter(string value)
        {
            if (value.Contains(","))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_value_cannot_contain", new object[] { "," }));
            }
        }

        private void ThrowIfReadOnly()
        {
            if (this.IsReadOnly)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
        }

        public override string ToString()
        {
            string str = null;
            if (base.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string str2 in this)
                {
                    this.ThrowIfContainsDelimiter(str2);
                    builder.Append(str2.Trim());
                    builder.Append(',');
                }
                str = builder.ToString();
                if (str.Length > 0)
                {
                    str = str.Substring(0, str.Length - 1);
                }
                if (str.Length == 0)
                {
                    str = null;
                }
            }
            return str;
        }

        public bool IsModified
        {
            get
            {
                if (!this._Modified)
                {
                    return (this.ToString() != this._OriginalString);
                }
                return true;
            }
        }

        public bool IsReadOnly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._ReadOnly;
            }
        }

        public string this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                this.ThrowIfReadOnly();
                this.ThrowIfContainsDelimiter(value);
                this._Modified = true;
                base[index] = value.Trim();
            }
        }
    }
}

