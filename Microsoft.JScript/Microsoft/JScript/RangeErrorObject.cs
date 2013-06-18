namespace Microsoft.JScript
{
    using System;

    public sealed class RangeErrorObject : ErrorObject
    {
        internal RangeErrorObject(ErrorPrototype parent, object[] args) : base(parent, args)
        {
        }

        internal RangeErrorObject(ErrorPrototype parent, object e) : base(parent, e)
        {
        }
    }
}

