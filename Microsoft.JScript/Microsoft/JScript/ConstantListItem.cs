namespace Microsoft.JScript
{
    using System;

    internal class ConstantListItem
    {
        internal ConstantListItem prev;
        internal object term;

        internal ConstantListItem(object term, ConstantListItem prev)
        {
            this.prev = prev;
            this.term = term;
        }
    }
}

