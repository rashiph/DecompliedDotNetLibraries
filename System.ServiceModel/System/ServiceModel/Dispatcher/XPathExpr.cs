namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathExpr
    {
        private bool castRequired;
        private bool negate;
        private ValueDataType returnType;
        private XPathExprList subExpr;
        private XPathExprType type;

        internal XPathExpr(XPathExprType type, ValueDataType returnType)
        {
            this.type = type;
            this.returnType = returnType;
        }

        internal XPathExpr(XPathExprType type, ValueDataType returnType, XPathExprList subExpr) : this(type, returnType)
        {
            this.subExpr = subExpr;
        }

        internal void Add(XPathExpr expr)
        {
            this.SubExpr.Add(expr);
        }

        internal void AddBooleanExpression(XPathExprType boolExprType, XPathExpr expr)
        {
            if (boolExprType == expr.Type)
            {
                XPathExprList subExpr = expr.SubExpr;
                for (int i = 0; i < subExpr.Count; i++)
                {
                    this.AddBooleanExpression(boolExprType, subExpr[i]);
                }
            }
            else
            {
                this.Add(expr);
            }
        }

        internal virtual bool IsLiteral
        {
            get
            {
                return false;
            }
        }

        internal bool Negate
        {
            get
            {
                return this.negate;
            }
            set
            {
                this.negate = value;
            }
        }

        internal ValueDataType ReturnType
        {
            get
            {
                return this.returnType;
            }
            set
            {
                this.returnType = value;
            }
        }

        internal XPathExprList SubExpr
        {
            get
            {
                if (this.subExpr == null)
                {
                    this.subExpr = new XPathExprList();
                }
                return this.subExpr;
            }
        }

        internal int SubExprCount
        {
            get
            {
                if (this.subExpr != null)
                {
                    return this.subExpr.Count;
                }
                return 0;
            }
        }

        internal XPathExprType Type
        {
            get
            {
                return this.type;
            }
        }

        internal bool TypecastRequired
        {
            get
            {
                return this.castRequired;
            }
            set
            {
                this.castRequired = value;
            }
        }
    }
}

