namespace MS.Internal.Xml.XPath
{
    using System.Xml.XPath;

    internal class Root : AstNode
    {
        public override XPathResultType ReturnType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }

        public override AstNode.AstType Type
        {
            get
            {
                return AstNode.AstType.Root;
            }
        }
    }
}

