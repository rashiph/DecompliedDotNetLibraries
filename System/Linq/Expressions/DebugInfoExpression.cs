namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;
    using System.Dynamic.Utils;

    [DebuggerTypeProxy(typeof(Expression.DebugInfoExpressionProxy))]
    public class DebugInfoExpression : Expression
    {
        private readonly SymbolDocumentInfo _document;

        internal DebugInfoExpression(SymbolDocumentInfo document)
        {
            this._document = document;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitDebugInfo(this);
        }

        public SymbolDocumentInfo Document
        {
            get
            {
                return this._document;
            }
        }

        public virtual int EndColumn
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public virtual int EndLine
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public virtual bool IsClear
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.DebugInfo;
            }
        }

        public virtual int StartColumn
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public virtual int StartLine
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return typeof(void);
            }
        }
    }
}

