namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Threading;

    [TypeConverter(typeof(ManagementQueryConverter))]
    public abstract class ManagementQuery : ICloneable
    {
        internal const string DEFAULTQUERYLANGUAGE = "WQL";
        private string queryLanguage;
        private string queryString;
        internal static readonly string tokenSelect = "select ";

        internal event IdentifierChangedEventHandler IdentifierChanged;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ManagementQuery() : this("WQL", null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ManagementQuery(string query) : this("WQL", query)
        {
        }

        internal ManagementQuery(string language, string query)
        {
            this.QueryLanguage = language;
            this.QueryString = query;
        }

        public abstract object Clone();
        internal void FireIdentifierChanged()
        {
            if (this.IdentifierChanged != null)
            {
                this.IdentifierChanged(this, null);
            }
        }

        protected internal virtual void ParseQuery(string query)
        {
        }

        internal static void ParseToken(ref string q, string token, ref bool bTokenFound)
        {
            if (bTokenFound)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY_DUP_TOKEN"));
            }
            bTokenFound = true;
            q = q.Remove(0, token.Length).TrimStart(null);
        }

        internal static void ParseToken(ref string q, string token, string op, ref bool bTokenFound, ref string tokenValue)
        {
            int length;
            if (bTokenFound)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY_DUP_TOKEN"));
            }
            bTokenFound = true;
            q = q.Remove(0, token.Length).TrimStart(null);
            if (op != null)
            {
                if (q.IndexOf(op, StringComparison.Ordinal) != 0)
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"));
                }
                q = q.Remove(0, op.Length).TrimStart(null);
            }
            if (q.Length == 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY_NULL_TOKEN"));
            }
            if (-1 == (length = q.IndexOf(' ')))
            {
                length = q.Length;
            }
            tokenValue = q.Substring(0, length);
            q = q.Remove(0, tokenValue.Length).TrimStart(null);
        }

        internal void SetQueryString(string qString)
        {
            this.queryString = qString;
        }

        public virtual string QueryLanguage
        {
            get
            {
                if (this.queryLanguage == null)
                {
                    return string.Empty;
                }
                return this.queryLanguage;
            }
            set
            {
                if (this.queryLanguage != value)
                {
                    this.queryLanguage = value;
                    this.FireIdentifierChanged();
                }
            }
        }

        public virtual string QueryString
        {
            get
            {
                if (this.queryString == null)
                {
                    return string.Empty;
                }
                return this.queryString;
            }
            set
            {
                if (this.queryString != value)
                {
                    this.ParseQuery(value);
                    this.queryString = value;
                    this.FireIdentifierChanged();
                }
            }
        }
    }
}

