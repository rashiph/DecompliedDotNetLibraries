namespace System.Net.Mime
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public class ContentType
    {
        internal static readonly string Default = "application/octet-stream";
        private bool isChanged;
        private bool isPersisted;
        private string mediaType;
        private TrackingStringDictionary parameters;
        private string subType;
        private string type;

        public ContentType() : this(Default)
        {
        }

        public ContentType(string contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            if (contentType == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "contentType" }), "contentType");
            }
            this.isChanged = true;
            this.type = contentType;
            this.ParseValue();
        }

        public override bool Equals(object rparam)
        {
            if (rparam == null)
            {
                return false;
            }
            return (string.Compare(this.ToString(), rparam.ToString(), StringComparison.OrdinalIgnoreCase) == 0);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        private void ParseValue()
        {
            int offset = 0;
            Exception exception = null;
            this.parameters = new TrackingStringDictionary();
            try
            {
                this.mediaType = MailBnfHelper.ReadToken(this.type, ref offset, null);
                if (((this.mediaType == null) || (this.mediaType.Length == 0)) || ((offset >= this.type.Length) || (this.type[offset++] != '/')))
                {
                    exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                }
                if (exception == null)
                {
                    this.subType = MailBnfHelper.ReadToken(this.type, ref offset, null);
                    if ((this.subType == null) || (this.subType.Length == 0))
                    {
                        exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                    }
                }
                if (exception == null)
                {
                    while (MailBnfHelper.SkipCFWS(this.type, ref offset))
                    {
                        string str2;
                        if (this.type[offset++] != ';')
                        {
                            exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                            break;
                        }
                        if (!MailBnfHelper.SkipCFWS(this.type, ref offset))
                        {
                            break;
                        }
                        string key = MailBnfHelper.ReadParameterAttribute(this.type, ref offset, null);
                        if ((key == null) || (key.Length == 0))
                        {
                            exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                            break;
                        }
                        if ((offset >= this.type.Length) || (this.type[offset++] != '='))
                        {
                            exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                            break;
                        }
                        if (!MailBnfHelper.SkipCFWS(this.type, ref offset))
                        {
                            exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                            break;
                        }
                        if (this.type[offset] == '"')
                        {
                            str2 = MailBnfHelper.ReadQuotedString(this.type, ref offset, null);
                        }
                        else
                        {
                            str2 = MailBnfHelper.ReadToken(this.type, ref offset, null);
                        }
                        if (str2 == null)
                        {
                            exception = new FormatException(SR.GetString("ContentTypeInvalid"));
                            break;
                        }
                        this.parameters.Add(key, str2);
                    }
                }
                this.parameters.IsChanged = false;
            }
            catch (FormatException)
            {
                throw new FormatException(SR.GetString("ContentTypeInvalid"));
            }
            if (exception != null)
            {
                throw new FormatException(SR.GetString("ContentTypeInvalid"));
            }
        }

        internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
        {
            if ((this.IsChanged || !this.isPersisted) || forcePersist)
            {
                headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), this.ToString());
                this.isPersisted = true;
            }
        }

        internal void Set(string contentType, HeaderCollection headers)
        {
            this.type = contentType;
            this.ParseValue();
            headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), this.ToString());
            this.isPersisted = true;
        }

        public override string ToString()
        {
            if ((this.type == null) || this.IsChanged)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.mediaType);
                builder.Append('/');
                builder.Append(this.subType);
                foreach (string str in this.Parameters.Keys)
                {
                    builder.Append("; ");
                    builder.Append(str);
                    builder.Append('=');
                    MailBnfHelper.GetTokenOrQuotedString(this.parameters[str], builder);
                }
                this.type = builder.ToString();
                this.isChanged = false;
                this.parameters.IsChanged = false;
                this.isPersisted = false;
            }
            return this.type;
        }

        public string Boundary
        {
            get
            {
                return this.Parameters["boundary"];
            }
            set
            {
                if ((value == null) || (value == string.Empty))
                {
                    this.Parameters.Remove("boundary");
                }
                else
                {
                    this.Parameters["boundary"] = value;
                }
            }
        }

        public string CharSet
        {
            get
            {
                return this.Parameters["charset"];
            }
            set
            {
                if ((value == null) || (value == string.Empty))
                {
                    this.Parameters.Remove("charset");
                }
                else
                {
                    this.Parameters["charset"] = value;
                }
            }
        }

        internal bool IsChanged
        {
            get
            {
                return (this.isChanged || ((this.parameters != null) && this.parameters.IsChanged));
            }
        }

        public string MediaType
        {
            get
            {
                return (this.mediaType + "/" + this.subType);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value == string.Empty)
                {
                    throw new ArgumentException(SR.GetString("net_emptystringset"), "value");
                }
                int offset = 0;
                this.mediaType = MailBnfHelper.ReadToken(value, ref offset, null);
                if (((this.mediaType.Length == 0) || (offset >= value.Length)) || (value[offset++] != '/'))
                {
                    throw new FormatException(SR.GetString("MediaTypeInvalid"));
                }
                this.subType = MailBnfHelper.ReadToken(value, ref offset, null);
                if ((this.subType.Length == 0) || (offset < value.Length))
                {
                    throw new FormatException(SR.GetString("MediaTypeInvalid"));
                }
                this.isChanged = true;
                this.isPersisted = false;
            }
        }

        public string Name
        {
            get
            {
                string str = this.Parameters["name"];
                if (MimeBasePart.DecodeEncoding(str) != null)
                {
                    str = MimeBasePart.DecodeHeaderValue(str);
                }
                return str;
            }
            set
            {
                if ((value == null) || (value == string.Empty))
                {
                    this.Parameters.Remove("name");
                }
                else if (MimeBasePart.IsAscii(value, false))
                {
                    this.Parameters["name"] = value;
                }
                else
                {
                    Encoding encoding = Encoding.GetEncoding("utf-8");
                    this.Parameters["name"] = MimeBasePart.EncodeHeaderValue(value, encoding, MimeBasePart.ShouldUseBase64Encoding(encoding));
                }
            }
        }

        public StringDictionary Parameters
        {
            get
            {
                if ((this.parameters == null) && (this.type == null))
                {
                    this.parameters = new TrackingStringDictionary();
                }
                return this.parameters;
            }
        }
    }
}

