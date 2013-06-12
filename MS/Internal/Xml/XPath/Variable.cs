namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class Variable : AstNode
    {
        private string localname;
        private string prefix;

        public Variable(string name, string prefix)
        {
            this.localname = name;
            this.prefix = prefix;
        }

        public string Localname
        {
            get
            {
                return this.localname;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public override XPathResultType ReturnType
        {
            get
            {
                return XPathResultType.Any;
            }
        }

        public override AstNode.AstType Type
        {
            get
            {
                return AstNode.AstType.Variable;
            }
        }
    }
}

