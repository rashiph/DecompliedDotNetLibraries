namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class XamlXmlReaderSettings : XamlReaderSettings
    {
        internal Dictionary<string, string> _xmlnsDictionary;

        public XamlXmlReaderSettings()
        {
        }

        public XamlXmlReaderSettings(XamlXmlReaderSettings settings) : base(settings)
        {
            if (settings != null)
            {
                if (settings._xmlnsDictionary != null)
                {
                    this._xmlnsDictionary = new Dictionary<string, string>(settings._xmlnsDictionary);
                }
                this.XmlLang = settings.XmlLang;
                this.XmlSpacePreserve = settings.XmlSpacePreserve;
                this.SkipXmlCompatibilityProcessing = settings.SkipXmlCompatibilityProcessing;
            }
        }

        public bool CloseInput { get; set; }

        public bool SkipXmlCompatibilityProcessing { get; set; }

        public string XmlLang { get; set; }

        public bool XmlSpacePreserve { get; set; }
    }
}

