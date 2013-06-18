namespace Microsoft.JScript
{
    using System;

    public sealed class LenientBooleanPrototype : BooleanPrototype
    {
        public object constructor;
        public object toString;
        public object valueOf;

        internal LenientBooleanPrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(parent, typeof(LenientBooleanPrototype))
        {
            base.noExpando = false;
            Type type = typeof(BooleanPrototype);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
            this.valueOf = new BuiltinFunction("valueOf", this, type.GetMethod("valueOf"), funcprot);
        }
    }
}

