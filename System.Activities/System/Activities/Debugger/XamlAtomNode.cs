namespace System.Activities.Debugger
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal class XamlAtomNode : System.Activities.Debugger.XamlNode
    {
        public override string ToString()
        {
            object obj2 = this.Value ?? string.Empty;
            return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", new object[] { this.NodeType, obj2.ToString() });
        }

        public sealed override XamlNodeType NodeType
        {
            get
            {
                return XamlNodeType.Value;
            }
        }

        public virtual object Value { get; set; }
    }
}

