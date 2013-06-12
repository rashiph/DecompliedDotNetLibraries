namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class UnescapedXmlDiagnosticData
    {
        private string _xmlString;

        public UnescapedXmlDiagnosticData(string xmlPayload)
        {
            this._xmlString = xmlPayload;
            if (this._xmlString == null)
            {
                this._xmlString = string.Empty;
            }
        }

        public override string ToString()
        {
            return this._xmlString;
        }

        public string UnescapedXml
        {
            get
            {
                return this._xmlString;
            }
            set
            {
                this._xmlString = value;
            }
        }
    }
}

