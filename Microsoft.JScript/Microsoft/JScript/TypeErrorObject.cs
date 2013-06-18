namespace Microsoft.JScript
{
    using System;

    public sealed class TypeErrorObject : ErrorObject
    {
        internal TypeErrorObject(ErrorPrototype parent, object[] args) : base(parent, args)
        {
        }

        internal TypeErrorObject(ErrorPrototype parent, object e) : base(parent, e)
        {
        }
    }
}

