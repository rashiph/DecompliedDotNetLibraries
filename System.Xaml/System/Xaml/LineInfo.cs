namespace System.Xaml
{
    using System;

    internal class LineInfo
    {
        private int _lineNumber;
        private int _linePosition;

        internal LineInfo(int lineNumber, int linePosition)
        {
            this._lineNumber = lineNumber;
            this._linePosition = linePosition;
        }

        public int LineNumber
        {
            get
            {
                return this._lineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return this._linePosition;
            }
        }
    }
}

