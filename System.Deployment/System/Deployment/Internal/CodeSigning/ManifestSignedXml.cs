namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.Security.Cryptography.Xml;
    using System.Xml;

    internal class ManifestSignedXml : SignedXml
    {
        private bool m_verify;

        internal ManifestSignedXml()
        {
        }

        internal ManifestSignedXml(XmlDocument document) : base(document)
        {
        }

        internal ManifestSignedXml(XmlElement elem) : base(elem)
        {
        }

        internal ManifestSignedXml(XmlDocument document, bool verify) : base(document)
        {
            this.m_verify = verify;
        }

        private static XmlElement FindIdElement(XmlElement context, string idValue)
        {
            if (context == null)
            {
                return null;
            }
            XmlElement element = context.SelectSingleNode("//*[@Id=\"" + idValue + "\"]") as XmlElement;
            if (element != null)
            {
                return element;
            }
            element = context.SelectSingleNode("//*[@id=\"" + idValue + "\"]") as XmlElement;
            if (element != null)
            {
                return element;
            }
            return (context.SelectSingleNode("//*[@ID=\"" + idValue + "\"]") as XmlElement);
        }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (this.m_verify)
            {
                return base.GetIdElement(document, idValue);
            }
            KeyInfo keyInfo = base.KeyInfo;
            if (keyInfo.Id != idValue)
            {
                return null;
            }
            return keyInfo.GetXml();
        }
    }
}

