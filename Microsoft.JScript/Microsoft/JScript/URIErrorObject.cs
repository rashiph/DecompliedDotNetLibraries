namespace Microsoft.JScript
{
    using System;

    public sealed class URIErrorObject : ErrorObject
    {
        internal URIErrorObject(ErrorPrototype parent, object[] args) : base(parent, args)
        {
        }

        internal URIErrorObject(ErrorPrototype parent, object e) : base(parent, e)
        {
        }
    }
}

