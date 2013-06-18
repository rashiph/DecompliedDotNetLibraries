namespace System.Xml
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    internal class MimeVersionHeader : MimeHeader
    {
        public static readonly MimeVersionHeader Default = new MimeVersionHeader("1.0");
        private string version;

        public MimeVersionHeader(string value) : base("mime-version", value)
        {
        }

        private void ParseValue()
        {
            if (base.Value == "1.0")
            {
                this.version = "1.0";
            }
            else
            {
                int offset = 0;
                if (!MailBnfHelper.SkipCFWS(base.Value, ref offset))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeVersionHeaderInvalid")));
                }
                StringBuilder builder = new StringBuilder();
                MailBnfHelper.ReadDigits(base.Value, ref offset, builder);
                if ((!MailBnfHelper.SkipCFWS(base.Value, ref offset) || (offset >= base.Value.Length)) || ((base.Value[offset++] != '.') || !MailBnfHelper.SkipCFWS(base.Value, ref offset)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeVersionHeaderInvalid")));
                }
                builder.Append('.');
                MailBnfHelper.ReadDigits(base.Value, ref offset, builder);
                this.version = builder.ToString();
            }
        }

        public string Version
        {
            get
            {
                if ((this.version == null) && (base.Value != null))
                {
                    this.ParseValue();
                }
                return this.version;
            }
        }
    }
}

