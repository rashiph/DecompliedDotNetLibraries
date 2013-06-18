namespace Microsoft.JScript
{
    using System;

    public sealed class LenientDateConstructor : DateConstructor
    {
        public object parse;
        public object UTC;

        internal LenientDateConstructor(LenientFunctionPrototype parent, LenientDatePrototype prototypeProp) : base(parent, prototypeProp)
        {
            base.noExpando = false;
            Type type = typeof(DateConstructor);
            this.parse = new BuiltinFunction("parse", this, type.GetMethod("parse"), parent);
            this.UTC = new BuiltinFunction("UTC", this, type.GetMethod("UTC"), parent);
        }
    }
}

