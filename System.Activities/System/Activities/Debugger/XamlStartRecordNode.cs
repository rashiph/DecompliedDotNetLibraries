namespace System.Activities.Debugger
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xml.Linq;

    internal class XamlStartRecordNode : System.Activities.Debugger.XamlNode
    {
        public override string ToString()
        {
            string str = (this.TypeName != null) ? this.TypeName.ToString() : string.Empty;
            return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", new object[] { this.NodeType, str });
        }

        public sealed override XamlNodeType NodeType
        {
            get
            {
                return XamlNodeType.StartObject;
            }
        }

        public XamlType RecordType { get; set; }

        public XName TypeName { get; set; }
    }
}

