namespace Microsoft.JScript
{
    using System;

    public sealed class LenientNumberPrototype : NumberPrototype
    {
        public object constructor;
        public object toExponential;
        public object toFixed;
        public object toLocaleString;
        public object toPrecision;
        public object toString;
        public object valueOf;

        internal LenientNumberPrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(NumberPrototype);
            this.toExponential = new BuiltinFunction("toExponential", this, type.GetMethod("toExponential"), funcprot);
            this.toFixed = new BuiltinFunction("toFixed", this, type.GetMethod("toFixed"), funcprot);
            this.toLocaleString = new BuiltinFunction("toLocaleString", this, type.GetMethod("toLocaleString"), funcprot);
            this.toPrecision = new BuiltinFunction("toPrecision", this, type.GetMethod("toPrecision"), funcprot);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
            this.valueOf = new BuiltinFunction("valueOf", this, type.GetMethod("valueOf"), funcprot);
        }
    }
}

