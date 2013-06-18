namespace Microsoft.JScript
{
    using System;

    public sealed class EvalErrorObject : ErrorObject
    {
        internal EvalErrorObject(ErrorPrototype parent, object[] args) : base(parent, args)
        {
        }

        internal EvalErrorObject(ErrorPrototype parent, object e) : base(parent, e)
        {
        }
    }
}

