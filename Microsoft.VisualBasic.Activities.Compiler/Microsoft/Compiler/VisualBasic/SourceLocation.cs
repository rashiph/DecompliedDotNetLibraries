namespace Microsoft.Compiler.VisualBasic
{
    using System;

    internal sealed class SourceLocation
    {
        private int m_columnBegin;
        private int m_columnEnd;
        private int m_lineBegin;
        private int m_lineEnd;

        internal SourceLocation(int columnBegin, int columnEnd, int lineBegin, int lineEnd)
        {
            this.m_columnBegin = columnBegin;
            this.m_columnEnd = columnEnd;
            this.m_lineBegin = lineBegin;
            this.m_lineEnd = lineEnd;
        }

        public int ColumnBegin
        {
            get
            {
                return this.m_columnBegin;
            }
        }

        public int ColumnEnd
        {
            get
            {
                return this.m_columnEnd;
            }
        }

        public int LineBegin
        {
            get
            {
                return this.m_lineBegin;
            }
        }

        public int LineEnd
        {
            get
            {
                return this.m_lineEnd;
            }
        }
    }
}

