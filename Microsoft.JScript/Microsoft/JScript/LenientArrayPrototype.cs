namespace Microsoft.JScript
{
    using System;

    public sealed class LenientArrayPrototype : ArrayPrototype
    {
        public object concat;
        public object constructor;
        public object join;
        public object pop;
        public object push;
        public object reverse;
        public object shift;
        public object slice;
        public object sort;
        public object splice;
        public object toLocaleString;
        public object toString;
        public object unshift;

        internal LenientArrayPrototype(FunctionPrototype funcprot, ObjectPrototype parent) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(ArrayPrototype);
            this.concat = new BuiltinFunction("concat", this, type.GetMethod("concat"), funcprot);
            this.join = new BuiltinFunction("join", this, type.GetMethod("join"), funcprot);
            this.pop = new BuiltinFunction("pop", this, type.GetMethod("pop"), funcprot);
            this.push = new BuiltinFunction("push", this, type.GetMethod("push"), funcprot);
            this.reverse = new BuiltinFunction("reverse", this, type.GetMethod("reverse"), funcprot);
            this.shift = new BuiltinFunction("shift", this, type.GetMethod("shift"), funcprot);
            this.slice = new BuiltinFunction("slice", this, type.GetMethod("slice"), funcprot);
            this.sort = new BuiltinFunction("sort", this, type.GetMethod("sort"), funcprot);
            this.splice = new BuiltinFunction("splice", this, type.GetMethod("splice"), funcprot);
            this.unshift = new BuiltinFunction("unshift", this, type.GetMethod("unshift"), funcprot);
            this.toLocaleString = new BuiltinFunction("toLocaleString", this, type.GetMethod("toLocaleString"), funcprot);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
        }
    }
}

