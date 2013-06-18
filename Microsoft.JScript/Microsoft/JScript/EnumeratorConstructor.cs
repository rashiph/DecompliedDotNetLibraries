namespace Microsoft.JScript
{
    using System;
    using System.Collections;

    public sealed class EnumeratorConstructor : ScriptFunction
    {
        internal static readonly EnumeratorConstructor ob = new EnumeratorConstructor();
        private EnumeratorPrototype originalPrototype;

        internal EnumeratorConstructor() : base(FunctionPrototype.ob, "Enumerator", 1)
        {
            this.originalPrototype = EnumeratorPrototype.ob;
            EnumeratorPrototype._constructor = this;
            base.proto = EnumeratorPrototype.ob;
        }

        internal EnumeratorConstructor(LenientFunctionPrototype parent, LenientEnumeratorPrototype prototypeProp) : base(parent, "Enumerator", 1)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            return null;
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public EnumeratorObject CreateInstance(params object[] args)
        {
            if (args.Length == 0)
            {
                return new EnumeratorObject(this.originalPrototype, null);
            }
            object obj2 = args[0];
            if (!(obj2 is IEnumerable))
            {
                throw new JScriptException(JSError.NotCollection);
            }
            return new EnumeratorObject(this.originalPrototype, (IEnumerable) obj2);
        }

        public object Invoke()
        {
            return null;
        }
    }
}

