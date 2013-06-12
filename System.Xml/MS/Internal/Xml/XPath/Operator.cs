namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class Operator : AstNode
    {
        private static Op[] invertOp;
        private AstNode opnd1;
        private AstNode opnd2;
        private Op opType;

        static Operator()
        {
            Op[] opArray = new Op[9];
            opArray[3] = Op.EQ;
            opArray[4] = Op.NE;
            opArray[5] = Op.GT;
            opArray[6] = Op.GE;
            opArray[7] = Op.LT;
            opArray[8] = Op.LE;
            invertOp = opArray;
        }

        public Operator(Op op, AstNode opnd1, AstNode opnd2)
        {
            this.opType = op;
            this.opnd1 = opnd1;
            this.opnd2 = opnd2;
        }

        public static Op InvertOperator(Op op)
        {
            return invertOp[(int) op];
        }

        public AstNode Operand1
        {
            get
            {
                return this.opnd1;
            }
        }

        public AstNode Operand2
        {
            get
            {
                return this.opnd2;
            }
        }

        public Op OperatorType
        {
            get
            {
                return this.opType;
            }
        }

        public override XPathResultType ReturnType
        {
            get
            {
                if (this.opType <= Op.GE)
                {
                    return XPathResultType.Boolean;
                }
                if (this.opType <= Op.MOD)
                {
                    return XPathResultType.Number;
                }
                return XPathResultType.NodeSet;
            }
        }

        public override AstNode.AstType Type
        {
            get
            {
                return AstNode.AstType.Operator;
            }
        }

        public enum Op
        {
            INVALID,
            OR,
            AND,
            EQ,
            NE,
            LT,
            LE,
            GT,
            GE,
            PLUS,
            MINUS,
            MUL,
            DIV,
            MOD,
            UNION
        }
    }
}

