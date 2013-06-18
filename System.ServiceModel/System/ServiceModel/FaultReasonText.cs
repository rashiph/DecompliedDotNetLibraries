namespace System.ServiceModel
{
    using System;
    using System.Globalization;

    public class FaultReasonText
    {
        private string text;
        private string xmlLang;

        public FaultReasonText(string text)
        {
            if (text == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("text"));
            }
            this.text = text;
            this.xmlLang = CultureInfo.CurrentCulture.Name;
        }

        public FaultReasonText(string text, CultureInfo cultureInfo)
        {
            if (text == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("text"));
            }
            if (cultureInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("cultureInfo"));
            }
            this.text = text;
            this.xmlLang = cultureInfo.Name;
        }

        public FaultReasonText(string text, string xmlLang)
        {
            if (text == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("text"));
            }
            if (xmlLang == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("xmlLang"));
            }
            this.text = text;
            this.xmlLang = xmlLang;
        }

        public bool Matches(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("cultureInfo"));
            }
            return (this.xmlLang == cultureInfo.Name);
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public string XmlLang
        {
            get
            {
                return this.xmlLang;
            }
        }
    }
}

