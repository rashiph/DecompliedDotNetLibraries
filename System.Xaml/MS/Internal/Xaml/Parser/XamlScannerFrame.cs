namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal class XamlScannerFrame
    {
        public XamlScannerFrame(System.Xaml.XamlType xamlType, string ns)
        {
            this.XamlType = xamlType;
            this.TypeNamespace = ns;
        }

        public bool InContent { get; set; }

        public string TypeNamespace { get; set; }

        public XamlMember XamlProperty { get; set; }

        public System.Xaml.XamlType XamlType { get; set; }

        public bool XmlSpacePreserve { get; set; }
    }
}

