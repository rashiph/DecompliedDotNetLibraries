namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class ExtendedResponse : DirectoryResponse
    {
        internal string name;
        internal byte[] value;

        internal ExtendedResponse(XmlNode node) : base(node)
        {
        }

        internal ExtendedResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
        {
        }

        public string ResponseName
        {
            get
            {
                if (base.dsmlRequest && (this.name == null))
                {
                    XmlElement element = (XmlElement) base.dsmlNode.SelectSingleNode("dsml:responseName", base.dsmlNS);
                    if (element != null)
                    {
                        this.name = element.InnerText;
                    }
                }
                return this.name;
            }
        }

        public byte[] ResponseValue
        {
            get
            {
                if (base.dsmlRequest && (this.value == null))
                {
                    XmlElement element = (XmlElement) base.dsmlNode.SelectSingleNode("dsml:response", base.dsmlNS);
                    if (element != null)
                    {
                        string innerText = element.InnerText;
                        try
                        {
                            this.value = Convert.FromBase64String(innerText);
                        }
                        catch (FormatException)
                        {
                            throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadBase64Value"));
                        }
                    }
                }
                if (this.value == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.value.Length];
                for (int i = 0; i < this.value.Length; i++)
                {
                    buffer[i] = this.value[i];
                }
                return buffer;
            }
        }
    }
}

