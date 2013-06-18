namespace Microsoft.JScript
{
    using System;

    public sealed class LenientFunctionPrototype : FunctionPrototype
    {
        public object apply;
        public object call;
        public object constructor;
        public object toString;

        internal LenientFunctionPrototype(ScriptObject parent) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(FunctionPrototype);
            this.apply = new BuiltinFunction("apply", this, type.GetMethod("apply"), this);
            this.call = new BuiltinFunction("call", this, type.GetMethod("call"), this);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), this);
        }
    }
}

