namespace System.Activities.Debugger
{
    using System.Xaml;

    internal class XamlGetObjectNode : System.Activities.Debugger.XamlNode
    {
        public sealed override XamlNodeType NodeType
        {
            get
            {
                return XamlNodeType.GetObject;
            }
        }
    }
}

