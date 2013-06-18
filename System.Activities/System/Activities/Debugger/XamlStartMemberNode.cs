namespace System.Activities.Debugger
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal class XamlStartMemberNode : System.Activities.Debugger.XamlNode
    {
        public override string ToString()
        {
            string str = (this.Member != null) ? this.Member.ToString() : string.Empty;
            return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", new object[] { this.NodeType, str });
        }

        public XamlMember Member { get; set; }

        public sealed override XamlNodeType NodeType
        {
            get
            {
                return XamlNodeType.StartMember;
            }
        }

        public XamlType RecordType { get; set; }
    }
}

