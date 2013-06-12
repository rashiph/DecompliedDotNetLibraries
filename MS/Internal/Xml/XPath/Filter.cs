namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class Filter : AstNode
    {
        private AstNode condition;
        private AstNode input;

        public Filter(AstNode input, AstNode condition)
        {
            this.input = input;
            this.condition = condition;
        }

        public AstNode Condition
        {
            get
            {
                return this.condition;
            }
        }

        public AstNode Input
        {
            get
            {
                return this.input;
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
                return AstNode.AstType.Filter;
            }
        }
    }
}

