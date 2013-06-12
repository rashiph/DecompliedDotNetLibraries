namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class Operand : AstNode
    {
        private XPathResultType type;
        private object val;

        public Operand(bool val)
        {
            this.type = XPathResultType.Boolean;
            this.val = val;
        }

        public Operand(double val)
        {
            this.type = XPathResultType.Number;
            this.val = val;
        }

        public Operand(string val)
        {
            this.type = XPathResultType.String;
            this.val = val;
        }

        public object OperandValue
        {
            get
            {
                return this.val;
            }
        }

        public override XPathResultType ReturnType
        {
            get
            {
                return this.type;
            }
        }

        public override AstNode.AstType Type
        {
            get
            {
                return AstNode.AstType.ConstantOperand;
            }
        }
    }
}

