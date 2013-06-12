namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal abstract class AstNode
    {
        protected AstNode()
        {
        }

        public abstract XPathResultType ReturnType { get; }

        public abstract AstType Type { get; }

        public enum AstType
        {
            Axis,
            Operator,
            Filter,
            ConstantOperand,
            Function,
            Group,
            Root,
            Variable,
            Error
        }
    }
}

