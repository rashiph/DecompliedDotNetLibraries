namespace System.Linq.Expressions
{
    using System;

    internal sealed class SpanDebugInfoExpression : DebugInfoExpression
    {
        private readonly int _endColumn;
        private readonly int _endLine;
        private readonly int _startColumn;
        private readonly int _startLine;

        internal SpanDebugInfoExpression(SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn) : base(document)
        {
            this._startLine = startLine;
            this._startColumn = startColumn;
            this._endLine = endLine;
            this._endColumn = endColumn;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitDebugInfo(this);
        }

        public override int EndColumn
        {
            get
            {
                return this._endColumn;
            }
        }

        public override int EndLine
        {
            get
            {
                return this._endLine;
            }
        }

        public override bool IsClear
        {
            get
            {
                return false;
            }
        }

        public override int StartColumn
        {
            get
            {
                return this._startColumn;
            }
        }

        public override int StartLine
        {
            get
            {
                return this._startLine;
            }
        }
    }
}

