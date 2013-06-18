namespace Microsoft.JScript
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal class TokenColorInfo : ITokenColorInfo
    {
        private TokenColor _color;
        internal TokenColorInfo _next;
        private Context _token;

        internal TokenColorInfo(Context token)
        {
            this._token = token.Clone();
            this._color = ColorFromToken(this._token);
            this._next = this;
        }

        internal TokenColorInfo Clone()
        {
            return (TokenColorInfo) base.MemberwiseClone();
        }

        internal static TokenColor ColorFromToken(Context context)
        {
            JSToken token = context.GetToken();
            if (JSScanner.IsKeyword(token))
            {
                return TokenColor.COLOR_KEYWORD;
            }
            if (JSToken.Identifier == token)
            {
                if (context.Equals("eval"))
                {
                    return TokenColor.COLOR_KEYWORD;
                }
                return TokenColor.COLOR_IDENTIFIER;
            }
            if (JSToken.StringLiteral == token)
            {
                return TokenColor.COLOR_STRING;
            }
            if ((JSToken.NumericLiteral == token) || (JSToken.IntegerLiteral == token))
            {
                return TokenColor.COLOR_NUMBER;
            }
            if ((JSToken.Comment == token) || (JSToken.UnterminatedComment == token))
            {
                return TokenColor.COLOR_COMMENT;
            }
            if (JSScanner.IsOperator(token))
            {
                return TokenColor.COLOR_OPERATOR;
            }
            return TokenColor.COLOR_TEXT;
        }

        internal void UpdateToken(Context token)
        {
            this._token = token.Clone();
            this._color = ColorFromToken(this._token);
        }

        public TokenColor Color
        {
            get
            {
                return this._color;
            }
        }

        public int EndPosition
        {
            get
            {
                return this._token.EndPosition;
            }
        }

        public int StartPosition
        {
            get
            {
                return this._token.StartPosition;
            }
        }
    }
}

