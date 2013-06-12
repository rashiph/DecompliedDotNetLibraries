namespace System.Net.Mime
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public class ContentDisposition
    {
        private const string creationDate = "creation-date";
        private static readonly TrackingValidationObjectDictionary.ValidateAndParseValue dateParser = ((TrackingValidationObjectDictionary.ValidateAndParseValue) (value => new SmtpDateTime(value.ToString())));
        private string disposition;
        private string dispositionType;
        private const string fileName = "filename";
        private bool isChanged;
        private bool isPersisted;
        private static readonly TrackingValidationObjectDictionary.ValidateAndParseValue longParser;
        private const string modificationDate = "modification-date";
        private TrackingValidationObjectDictionary parameters;
        private const string readDate = "read-date";
        private const string size = "size";
        private static readonly IDictionary<string, TrackingValidationObjectDictionary.ValidateAndParseValue> validators;

        static ContentDisposition()
        {
            longParser = delegate (object value) {
                long num;
                if (!long.TryParse(value.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out num))
                {
                    throw new FormatException(SR.GetString("ContentDispositionInvalid"));
                }
                return num;
            };
            validators = new Dictionary<string, TrackingValidationObjectDictionary.ValidateAndParseValue>();
            validators.Add("creation-date", dateParser);
            validators.Add("modification-date", dateParser);
            validators.Add("read-date", dateParser);
            validators.Add("size", longParser);
        }

        public ContentDisposition()
        {
            this.isChanged = true;
            this.dispositionType = "attachment";
            this.disposition = this.dispositionType;
        }

        public ContentDisposition(string disposition)
        {
            if (disposition == null)
            {
                throw new ArgumentNullException("disposition");
            }
            this.isChanged = true;
            this.disposition = disposition;
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

        internal DateTime GetDateParameter(string parameterName)
        {
            SmtpDateTime time = ((TrackingValidationObjectDictionary) this.Parameters).InternalGet(parameterName) as SmtpDateTime;
            if (time == null)
            {
                return DateTime.MinValue;
            }
            return time.Date;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        private void ParseValue()
        {
            int offset = 0;
            try
            {
                this.dispositionType = MailBnfHelper.ReadToken(this.disposition, ref offset, null);
                if (string.IsNullOrEmpty(this.dispositionType))
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldMalformedHeader"));
                }
                if (this.parameters == null)
                {
                    this.parameters = new TrackingValidationObjectDictionary(validators);
                }
                else
                {
                    this.parameters.Clear();
                }
                while (MailBnfHelper.SkipCFWS(this.disposition, ref offset))
                {
                    string str2;
                    if (this.disposition[offset++] != ';')
                    {
                        throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { this.disposition[offset - 1] }));
                    }
                    if (!MailBnfHelper.SkipCFWS(this.disposition, ref offset))
                    {
                        goto Label_018C;
                    }
                    string str = MailBnfHelper.ReadParameterAttribute(this.disposition, ref offset, null);
                    if (this.disposition[offset++] != '=')
                    {
                        throw new FormatException(SR.GetString("MailHeaderFieldMalformedHeader"));
                    }
                    if (!MailBnfHelper.SkipCFWS(this.disposition, ref offset))
                    {
                        throw new FormatException(SR.GetString("ContentDispositionInvalid"));
                    }
                    if (this.disposition[offset] == '"')
                    {
                        str2 = MailBnfHelper.ReadQuotedString(this.disposition, ref offset, null);
                    }
                    else
                    {
                        str2 = MailBnfHelper.ReadToken(this.disposition, ref offset, null);
                    }
                    if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str2))
                    {
                        throw new FormatException(SR.GetString("ContentDispositionInvalid"));
                    }
                    this.Parameters.Add(str, str2);
                }
            }
            catch (FormatException exception)
            {
                throw new FormatException(SR.GetString("ContentDispositionInvalid"), exception);
            }
        Label_018C:
            this.parameters.IsChanged = false;
        }

        internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
        {
            if ((this.IsChanged || !this.isPersisted) || forcePersist)
            {
                headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), this.ToString());
                this.isPersisted = true;
            }
        }

        internal void Set(string contentDisposition, HeaderCollection headers)
        {
            this.disposition = contentDisposition;
            this.ParseValue();
            headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), this.ToString());
            this.isPersisted = true;
        }

        public override string ToString()
        {
            if (((this.disposition == null) || this.isChanged) || ((this.parameters != null) && this.parameters.IsChanged))
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.dispositionType);
                foreach (string str in this.Parameters.Keys)
                {
                    builder.Append("; ");
                    builder.Append(str);
                    builder.Append('=');
                    MailBnfHelper.GetTokenOrQuotedString(this.parameters[str], builder);
                }
                this.disposition = builder.ToString();
                this.isChanged = false;
                this.parameters.IsChanged = false;
                this.isPersisted = false;
            }
            return this.disposition;
        }

        public DateTime CreationDate
        {
            get
            {
                return this.GetDateParameter("creation-date");
            }
            set
            {
                SmtpDateTime time = new SmtpDateTime(value);
                ((TrackingValidationObjectDictionary) this.Parameters).InternalSet("creation-date", time);
            }
        }

        public string DispositionType
        {
            get
            {
                return this.dispositionType;
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
                this.isChanged = true;
                this.dispositionType = value;
            }
        }

        public string FileName
        {
            get
            {
                return this.Parameters["filename"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Parameters.Remove("filename");
                }
                else
                {
                    this.Parameters["filename"] = value;
                }
            }
        }

        public bool Inline
        {
            get
            {
                return (this.dispositionType == "inline");
            }
            set
            {
                this.isChanged = true;
                if (value)
                {
                    this.dispositionType = "inline";
                }
                else
                {
                    this.dispositionType = "attachment";
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

        public DateTime ModificationDate
        {
            get
            {
                return this.GetDateParameter("modification-date");
            }
            set
            {
                SmtpDateTime time = new SmtpDateTime(value);
                ((TrackingValidationObjectDictionary) this.Parameters).InternalSet("modification-date", time);
            }
        }

        public StringDictionary Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new TrackingValidationObjectDictionary(validators);
                }
                return this.parameters;
            }
        }

        public DateTime ReadDate
        {
            get
            {
                return this.GetDateParameter("read-date");
            }
            set
            {
                SmtpDateTime time = new SmtpDateTime(value);
                ((TrackingValidationObjectDictionary) this.Parameters).InternalSet("read-date", time);
            }
        }

        public long Size
        {
            get
            {
                object obj2 = ((TrackingValidationObjectDictionary) this.Parameters).InternalGet("size");
                if (obj2 == null)
                {
                    return -1L;
                }
                return (long) obj2;
            }
            set
            {
                ((TrackingValidationObjectDictionary) this.Parameters).InternalSet("size", value);
            }
        }
    }
}

