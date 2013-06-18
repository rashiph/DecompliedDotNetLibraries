namespace Microsoft.JScript
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal class JSColorizer : IColorizeText
    {
        private JSScanner _scanner = new JSScanner();
        private SourceState _state;

        internal JSColorizer()
        {
            this._scanner.SetAuthoringMode(true);
            this._state = SourceState.STATE_COLOR_NORMAL;
        }

        public virtual ITokenEnumerator Colorize(string sourceCode, SourceState state)
        {
            TokenColorInfoList list = new TokenColorInfoList();
            this._state = SourceState.STATE_COLOR_NORMAL;
            if (sourceCode.Length > 0)
            {
                Context sourceContext = new Context(null, sourceCode);
                this._scanner.SetSource(sourceContext);
                try
                {
                    if (SourceState.STATE_COLOR_COMMENT == state)
                    {
                        int length = this._scanner.SkipMultiLineComment();
                        if (length > sourceCode.Length)
                        {
                            this._state = SourceState.STATE_COLOR_COMMENT;
                            length = sourceCode.Length;
                        }
                        list.Add(sourceContext);
                        if (length == sourceCode.Length)
                        {
                            return list;
                        }
                    }
                    this._scanner.GetNextToken();
                    JSToken none = JSToken.None;
                    while (sourceContext.GetToken() != JSToken.EndOfFile)
                    {
                        list.Add(sourceContext);
                        none = sourceContext.GetToken();
                        this._scanner.GetNextToken();
                    }
                    if (JSToken.UnterminatedComment == none)
                    {
                        this._state = SourceState.STATE_COLOR_COMMENT;
                    }
                }
                catch (ScannerException)
                {
                }
            }
            return list;
        }

        public virtual SourceState GetStateForText(string sourceCode, SourceState state)
        {
            if (sourceCode != null)
            {
                this._state = SourceState.STATE_COLOR_NORMAL;
                Context sourceContext = new Context(null, sourceCode);
                this._scanner.SetSource(sourceContext);
                if ((SourceState.STATE_COLOR_COMMENT == state) && (this._scanner.SkipMultiLineComment() > sourceCode.Length))
                {
                    this._state = SourceState.STATE_COLOR_COMMENT;
                    return this._state;
                }
                this._scanner.GetNextToken();
                JSToken none = JSToken.None;
                while (sourceContext.GetToken() != JSToken.EndOfFile)
                {
                    none = sourceContext.GetToken();
                    this._scanner.GetNextToken();
                }
                if (JSToken.UnterminatedComment == none)
                {
                    this._state = SourceState.STATE_COLOR_COMMENT;
                }
            }
            return this._state;
        }
    }
}

