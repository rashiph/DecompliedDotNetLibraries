namespace System.Web.Compilation
{
    using System;

    [Serializable]
    public sealed class LinePragmaCodeInfo
    {
        internal int _codeLength;
        internal bool _isCodeNugget;
        internal int _startColumn;
        internal int _startGeneratedColumn;
        internal int _startLine;

        public LinePragmaCodeInfo()
        {
        }

        public LinePragmaCodeInfo(int startLine, int startColumn, int startGeneratedColumn, int codeLength, bool isCodeNugget)
        {
            this._startLine = startLine;
            this._startColumn = startColumn;
            this._startGeneratedColumn = startGeneratedColumn;
            this._codeLength = codeLength;
            this._isCodeNugget = isCodeNugget;
        }

        public int CodeLength
        {
            get
            {
                return this._codeLength;
            }
        }

        public bool IsCodeNugget
        {
            get
            {
                return this._isCodeNugget;
            }
        }

        public int StartColumn
        {
            get
            {
                return this._startColumn;
            }
        }

        public int StartGeneratedColumn
        {
            get
            {
                return this._startGeneratedColumn;
            }
        }

        public int StartLine
        {
            get
            {
                return this._startLine;
            }
        }
    }
}

