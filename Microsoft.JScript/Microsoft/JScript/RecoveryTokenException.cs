namespace Microsoft.JScript
{
    using System;

    internal class RecoveryTokenException : ParserException
    {
        internal AST _partiallyComputedNode;
        internal JSToken _token;

        internal RecoveryTokenException(JSToken token, AST partialAST)
        {
            this._token = token;
            this._partiallyComputedNode = partialAST;
        }
    }
}

