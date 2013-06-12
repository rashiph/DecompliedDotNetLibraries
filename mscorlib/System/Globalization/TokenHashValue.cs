namespace System.Globalization
{
    using System;

    internal class TokenHashValue
    {
        internal string tokenString;
        internal TokenType tokenType;
        internal int tokenValue;

        internal TokenHashValue(string tokenString, TokenType tokenType, int tokenValue)
        {
            this.tokenString = tokenString;
            this.tokenType = tokenType;
            this.tokenValue = tokenValue;
        }
    }
}

