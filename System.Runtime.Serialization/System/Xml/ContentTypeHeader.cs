namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    internal class ContentTypeHeader : MimeHeader
    {
        public static readonly ContentTypeHeader Default = new ContentTypeHeader("application/octet-stream");
        private string mediaType;
        private Dictionary<string, string> parameters;
        private string subType;

        public ContentTypeHeader(string value) : base("content-type", value)
        {
        }

        private void ParseValue()
        {
            if (this.parameters == null)
            {
                int offset = 0;
                this.parameters = new Dictionary<string, string>();
                this.mediaType = MailBnfHelper.ReadToken(base.Value, ref offset, null);
                if ((offset >= base.Value.Length) || (base.Value[offset++] != '/'))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeContentTypeHeaderInvalid")));
                }
                this.subType = MailBnfHelper.ReadToken(base.Value, ref offset, null);
                while (MailBnfHelper.SkipCFWS(base.Value, ref offset))
                {
                    if ((offset >= base.Value.Length) || (base.Value[offset++] != ';'))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeContentTypeHeaderInvalid")));
                    }
                    if (!MailBnfHelper.SkipCFWS(base.Value, ref offset))
                    {
                        break;
                    }
                    string str = MailBnfHelper.ReadParameterAttribute(base.Value, ref offset, null);
                    if (((str == null) || (offset >= base.Value.Length)) || (base.Value[offset++] != '='))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeContentTypeHeaderInvalid")));
                    }
                    string str2 = MailBnfHelper.ReadParameterValue(base.Value, ref offset, null);
                    this.parameters.Add(str.ToLowerInvariant(), str2);
                }
                if (this.parameters.ContainsKey(MtomGlobals.StartInfoParam))
                {
                    string data = this.parameters[MtomGlobals.StartInfoParam];
                    int index = data.IndexOf(';');
                    if (index > -1)
                    {
                        while (MailBnfHelper.SkipCFWS(data, ref index))
                        {
                            if (data[index] != ';')
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeContentTypeHeaderInvalid")));
                            }
                            index++;
                            string str4 = MailBnfHelper.ReadParameterAttribute(data, ref index, null);
                            if (((str4 == null) || (index >= data.Length)) || (data[index++] != '='))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeContentTypeHeaderInvalid")));
                            }
                            string str5 = MailBnfHelper.ReadParameterValue(data, ref index, null);
                            if (str4 == MtomGlobals.ActionParam)
                            {
                                this.parameters[MtomGlobals.ActionParam] = str5;
                            }
                        }
                    }
                }
            }
        }

        public string MediaSubtype
        {
            get
            {
                if ((this.subType == null) && (base.Value != null))
                {
                    this.ParseValue();
                }
                return this.subType;
            }
        }

        public string MediaType
        {
            get
            {
                if ((this.mediaType == null) && (base.Value != null))
                {
                    this.ParseValue();
                }
                return this.mediaType;
            }
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    if (base.Value != null)
                    {
                        this.ParseValue();
                    }
                    else
                    {
                        this.parameters = new Dictionary<string, string>();
                    }
                }
                return this.parameters;
            }
        }
    }
}

