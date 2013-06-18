namespace Microsoft.JScript
{
    using System;

    public sealed class ReferenceErrorObject : ErrorObject
    {
        internal ReferenceErrorObject(ErrorPrototype parent, object[] args) : base(parent, args)
        {
        }

        internal ReferenceErrorObject(ErrorPrototype parent, object e) : base(parent, e)
        {
        }
    }
}

