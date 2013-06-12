namespace System.Linq.Expressions
{
    using System;

    internal sealed class ClearDebugInfoExpression : DebugInfoExpression
    {
        internal ClearDebugInfoExpression(SymbolDocumentInfo document) : base(document)
        {
        }

        public override int EndColumn
        {
            get
            {
                return 0;
            }
        }

        public override int EndLine
        {
            get
            {
                return 0xfeefee;
            }
        }

        public override bool IsClear
        {
            get
            {
                return true;
            }
        }

        public override int StartColumn
        {
            get
            {
                return 0;
            }
        }

        public override int StartLine
        {
            get
            {
                return 0xfeefee;
            }
        }
    }
}

