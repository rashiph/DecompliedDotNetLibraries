namespace Microsoft.JScript
{
    using System;

    public sealed class LenientRegExpPrototype : RegExpPrototype
    {
        public object compile;
        public object constructor;
        public object exec;
        public object test;
        public object toString;

        internal LenientRegExpPrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(RegExpPrototype);
            this.compile = new BuiltinFunction("compile", this, type.GetMethod("compile"), funcprot);
            this.exec = new BuiltinFunction("exec", this, type.GetMethod("exec"), funcprot);
            this.test = new BuiltinFunction("test", this, type.GetMethod("test"), funcprot);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
        }
    }
}

