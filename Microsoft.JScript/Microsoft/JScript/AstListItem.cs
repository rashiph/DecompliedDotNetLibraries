namespace Microsoft.JScript
{
    using System;

    internal class AstListItem
    {
        internal AstListItem _prev;
        internal AST _term;

        internal AstListItem(AST term, AstListItem prev)
        {
            this._prev = prev;
            this._term = term;
        }
    }
}

