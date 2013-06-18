namespace Microsoft.JScript
{
    using System;

    public sealed class LenientErrorPrototype : ErrorPrototype
    {
        public object constructor;
        public object name;
        public object toString;

        internal LenientErrorPrototype(LenientFunctionPrototype funcprot, ScriptObject parent, string name) : base(parent, name)
        {
            base.noExpando = false;
            this.name = name;
            Type type = typeof(ErrorPrototype);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
        }
    }
}

