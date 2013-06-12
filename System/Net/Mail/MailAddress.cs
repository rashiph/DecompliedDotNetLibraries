namespace System.Net.Mail
{
    using System;
    using System.Globalization;
    using System.Net.Mime;
    using System.Text;

    public class MailAddress
    {
        private readonly string displayName;
        private readonly Encoding displayNameEncoding;
        private static EncodedStreamFactory encoderFactory = new EncodedStreamFactory();
        private readonly string host;
        private readonly string userName;

        public MailAddress(string address) : this(address, null, (Encoding) null)
        {
        }

        public MailAddress(string address, string displayName) : this(address, displayName, (Encoding) null)
        {
        }

        internal MailAddress(string displayName, string userName, string domain)
        {
            this.host = domain;
            this.userName = userName;
            this.displayName = displayName;
            this.displayNameEncoding = Encoding.GetEncoding("utf-8");
        }

        public MailAddress(string address, string displayName, Encoding displayNameEncoding)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "address" }), "address");
            }
            this.displayNameEncoding = displayNameEncoding ?? Encoding.GetEncoding("utf-8");
            this.displayName = displayName ?? string.Empty;
            if ((!string.IsNullOrEmpty(this.displayName) && (this.displayName.Length >= 2)) && ((this.displayName[0] == '"') && (this.displayName[this.displayName.Length - 1] == '"')))
            {
                this.displayName = this.displayName.Substring(1, this.displayName.Length - 2);
            }
            MailAddress address2 = MailAddressParser.ParseAddress(address);
            this.host = address2.host;
            this.userName = address2.userName;
            if (string.IsNullOrEmpty(this.displayName))
            {
                this.displayName = address2.displayName;
            }
        }

        internal string Encode(int charsConsumed)
        {
            string encodedString = string.Empty;
            if (!string.IsNullOrEmpty(this.displayName))
            {
                if (MimeBasePart.IsAscii(this.displayName, false))
                {
                    encodedString = string.Format("\"{0}\"", this.displayName);
                }
                else
                {
                    IEncodableStream stream = encoderFactory.GetEncoderForHeader(this.displayNameEncoding, false, charsConsumed);
                    byte[] bytes = this.displayNameEncoding.GetBytes(this.displayName);
                    stream.EncodeBytes(bytes, 0, bytes.Length);
                    encodedString = stream.GetEncodedString();
                }
                encodedString = encodedString + "\r\n ";
            }
            if (!string.IsNullOrEmpty(encodedString))
            {
                return (encodedString + this.SmtpAddress);
            }
            return this.Address;
        }

        public override bool Equals(object value)
        {
            if (value == null)
            {
                return false;
            }
            return this.ToString().Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.DisplayName))
            {
                return this.Address;
            }
            return string.Format("\"{0}\" {1}", this.DisplayName, this.SmtpAddress);
        }

        public string Address
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}@{1}", new object[] { this.userName, this.host });
            }
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
        }

        public string Host
        {
            get
            {
                return this.host;
            }
        }

        internal string SmtpAddress
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "<{0}>", new object[] { this.Address });
            }
        }

        public string User
        {
            get
            {
                return this.userName;
            }
        }
    }
}

