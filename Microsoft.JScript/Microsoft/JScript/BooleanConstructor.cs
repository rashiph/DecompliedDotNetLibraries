namespace Microsoft.JScript
{
    using System;

    public sealed class BooleanConstructor : ScriptFunction
    {
        internal static readonly BooleanConstructor ob = new BooleanConstructor();
        private BooleanPrototype originalPrototype;

        internal BooleanConstructor() : base(FunctionPrototype.ob, "Boolean", 1)
        {
            this.originalPrototype = BooleanPrototype.ob;
            BooleanPrototype._constructor = this;
            base.proto = BooleanPrototype.ob;
        }

        internal BooleanConstructor(LenientFunctionPrototype parent, LenientBooleanPrototype prototypeProp) : base(parent, "Boolean", 1)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            if (args.Length == 0)
            {
                return false;
            }
            return Microsoft.JScript.Convert.ToBoolean(args[0]);
        }

        internal BooleanObject Construct()
        {
            return new BooleanObject(this.originalPrototype, false, false);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        internal BooleanObject ConstructImplicitWrapper(bool arg)
        {
            return new BooleanObject(this.originalPrototype, arg, true);
        }

        internal BooleanObject ConstructWrapper(bool arg)
        {
            return new BooleanObject(this.originalPrototype, arg, false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public BooleanObject CreateInstance(params object[] args)
        {
            return new BooleanObject(this.originalPrototype, (args.Length != 0) && Microsoft.JScript.Convert.ToBoolean(args[0]), false);
        }

        public bool Invoke(object arg)
        {
            return Microsoft.JScript.Convert.ToBoolean(arg);
        }
    }
}

