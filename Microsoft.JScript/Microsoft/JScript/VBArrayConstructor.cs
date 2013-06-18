namespace Microsoft.JScript
{
    using System;

    public sealed class VBArrayConstructor : ScriptFunction
    {
        internal static readonly VBArrayConstructor ob = new VBArrayConstructor();
        private VBArrayPrototype originalPrototype;

        internal VBArrayConstructor() : base(FunctionPrototype.ob, "VBArray", 1)
        {
            this.originalPrototype = VBArrayPrototype.ob;
            VBArrayPrototype._constructor = this;
            base.proto = VBArrayPrototype.ob;
        }

        internal VBArrayConstructor(LenientFunctionPrototype parent, LenientVBArrayPrototype prototypeProp) : base(parent, "VBArray", 1)
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

        internal VBArrayObject Construct()
        {
            return new VBArrayObject(this.originalPrototype, null);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public object CreateInstance(params object[] args)
        {
            if ((args.Length < 1) || !typeof(Array).IsAssignableFrom(args[0].GetType()))
            {
                throw new JScriptException(JSError.VBArrayExpected);
            }
            return new VBArrayObject(this.originalPrototype, (Array) args[0]);
        }
    }
}

