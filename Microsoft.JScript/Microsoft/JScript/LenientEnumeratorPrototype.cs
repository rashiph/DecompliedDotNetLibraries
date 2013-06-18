namespace Microsoft.JScript
{
    using System;

    public sealed class LenientEnumeratorPrototype : EnumeratorPrototype
    {
        public object atEnd;
        public object constructor;
        public object item;
        public object moveFirst;
        public object moveNext;

        internal LenientEnumeratorPrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(EnumeratorPrototype);
            this.atEnd = new BuiltinFunction("atEnd", this, type.GetMethod("atEnd"), funcprot);
            this.item = new BuiltinFunction("item", this, type.GetMethod("item"), funcprot);
            this.moveFirst = new BuiltinFunction("moveFirst", this, type.GetMethod("moveFirst"), funcprot);
            this.moveNext = new BuiltinFunction("moveNext", this, type.GetMethod("moveNext"), funcprot);
        }
    }
}

