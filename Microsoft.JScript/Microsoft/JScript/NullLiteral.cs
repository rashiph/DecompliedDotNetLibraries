namespace Microsoft.JScript
{
    using System;

    internal sealed class NullLiteral : ConstantWrapper
    {
        internal NullLiteral(Context context) : base(DBNull.Value, context)
        {
        }
    }
}

