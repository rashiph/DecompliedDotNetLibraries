namespace Microsoft.JScript
{
    using System;

    public sealed class SyntaxErrorObject : ErrorObject
    {
        internal SyntaxErrorObject(ErrorPrototype parent, object[] args) : base(parent, args)
        {
        }

        internal SyntaxErrorObject(ErrorPrototype parent, object e) : base(parent, e)
        {
        }
    }
}

