namespace Microsoft.JScript
{
    using System;

    internal class OpListItem
    {
        internal JSToken _operator;
        internal OpPrec _prec;
        internal OpListItem _prev;

        internal OpListItem(JSToken op, OpPrec prec, OpListItem prev)
        {
            this._prev = prev;
            this._operator = op;
            this._prec = prec;
        }
    }
}

