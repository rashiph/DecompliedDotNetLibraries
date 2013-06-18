namespace System.Activities.Debugger
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal class XamlNamespaceNode : System.Activities.Debugger.XamlNode
    {
        public XamlNamespaceNode()
        {
        }

        public XamlNamespaceNode(NamespaceDeclaration @namespace)
        {
            this.Namespace = @namespace;
        }

        public NamespaceDeclaration Namespace { get; set; }

        public sealed override XamlNodeType NodeType
        {
            get
            {
                return XamlNodeType.NamespaceDeclaration;
            }
        }
    }
}

