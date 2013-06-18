namespace Microsoft.JScript
{
    using System;

    public class LenientStringConstructor : StringConstructor
    {
        public object fromCharCode;

        internal LenientStringConstructor(LenientFunctionPrototype parent, LenientStringPrototype prototypeProp) : base(parent, prototypeProp)
        {
            base.noExpando = false;
            Type type = typeof(StringConstructor);
            this.fromCharCode = new BuiltinFunction("fromCharCode", this, type.GetMethod("fromCharCode"), parent);
        }
    }
}

