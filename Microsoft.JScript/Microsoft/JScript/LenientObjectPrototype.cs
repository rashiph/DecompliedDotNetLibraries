namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;

    public class LenientObjectPrototype : ObjectPrototype
    {
        public object constructor;
        public object hasOwnProperty;
        public object isPrototypeOf;
        public object propertyIsEnumerable;
        public object toLocaleString;
        public object toString;
        public object valueOf;

        internal LenientObjectPrototype(VsaEngine engine)
        {
            base.engine = engine;
            base.noExpando = false;
        }

        internal void Initialize(LenientFunctionPrototype funcprot)
        {
            Type type = typeof(ObjectPrototype);
            this.hasOwnProperty = new BuiltinFunction("hasOwnProperty", this, type.GetMethod("hasOwnProperty"), funcprot);
            this.isPrototypeOf = new BuiltinFunction("isPrototypeOf", this, type.GetMethod("isPrototypeOf"), funcprot);
            this.propertyIsEnumerable = new BuiltinFunction("propertyIsEnumerable", this, type.GetMethod("propertyIsEnumerable"), funcprot);
            this.toLocaleString = new BuiltinFunction("toLocaleString", this, type.GetMethod("toLocaleString"), funcprot);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
            this.valueOf = new BuiltinFunction("valueOf", this, type.GetMethod("valueOf"), funcprot);
        }
    }
}

