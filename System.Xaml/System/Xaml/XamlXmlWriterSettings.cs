namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public class XamlXmlWriterSettings : XamlWriterSettings
    {
        public XamlXmlWriterSettings Copy()
        {
            return new XamlXmlWriterSettings { AssumeValidInput = this.AssumeValidInput, CloseOutput = this.CloseOutput };
        }

        public bool AssumeValidInput { get; set; }

        public bool CloseOutput { get; set; }
    }
}

