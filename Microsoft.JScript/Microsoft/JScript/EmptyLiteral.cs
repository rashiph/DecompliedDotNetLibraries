namespace Microsoft.JScript
{
    using System;

    internal sealed class EmptyLiteral : ConstantWrapper
    {
        internal EmptyLiteral(Context context) : base(null, context)
        {
        }
    }
}

