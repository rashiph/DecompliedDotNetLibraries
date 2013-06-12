namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class Group : AstNode
    {
        private AstNode groupNode;

        public Group(AstNode groupNode)
        {
            this.groupNode = groupNode;
        }

        public AstNode GroupNode
        {
            get
            {
                return this.groupNode;
            }
        }

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
                return AstNode.AstType.Group;
            }
        }
    }
}

