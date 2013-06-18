namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.Collections;

    internal abstract class TokenEnumerator : IEnumerator
    {
        protected Token current;

        protected TokenEnumerator()
        {
        }

        internal abstract bool FindNextToken();
        public bool MoveNext()
        {
            if (this.Reader.EndOfLines)
            {
                return false;
            }
            int currentLine = this.Reader.CurrentLine;
            int position = this.Reader.Position;
            bool flag = this.FindNextToken();
            if (flag && (this.current != null))
            {
                this.current.Line = currentLine;
                if (this.current.InnerText == null)
                {
                    this.current.InnerText = this.Reader.GetCurrentMatchedString(position);
                }
            }
            return flag;
        }

        public void Reset()
        {
            this.Reader.Reset();
            this.current = null;
        }

        public object Current
        {
            get
            {
                return this.current;
            }
        }

        internal abstract TokenCharReader Reader { get; }
    }
}

