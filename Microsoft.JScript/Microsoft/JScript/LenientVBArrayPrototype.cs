namespace Microsoft.JScript
{
    using System;

    public sealed class LenientVBArrayPrototype : VBArrayPrototype
    {
        public object constructor;
        public object dimensions;
        public object getItem;
        public object lbound;
        public object toArray;
        public object ubound;

        internal LenientVBArrayPrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(funcprot, parent)
        {
            base.noExpando = false;
            Type type = typeof(VBArrayPrototype);
            this.dimensions = new BuiltinFunction("dimensions", this, type.GetMethod("dimensions"), funcprot);
            this.getItem = new BuiltinFunction("getItem", this, type.GetMethod("getItem"), funcprot);
            this.lbound = new BuiltinFunction("lbound", this, type.GetMethod("lbound"), funcprot);
            this.toArray = new BuiltinFunction("toArray", this, type.GetMethod("toArray"), funcprot);
            this.ubound = new BuiltinFunction("ubound", this, type.GetMethod("ubound"), funcprot);
        }
    }
}

