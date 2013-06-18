namespace Microsoft.JScript
{
    using System;

    public class NumberConstructor : ScriptFunction
    {
        public const double MAX_VALUE = double.MaxValue;
        public const double MIN_VALUE = double.Epsilon;
        public const double NaN = double.NaN;
        public const double NEGATIVE_INFINITY = double.NegativeInfinity;
        internal static readonly NumberConstructor ob = new NumberConstructor();
        private NumberPrototype originalPrototype;
        public const double POSITIVE_INFINITY = double.PositiveInfinity;

        internal NumberConstructor() : base(FunctionPrototype.ob, "Number", 1)
        {
            this.originalPrototype = NumberPrototype.ob;
            NumberPrototype._constructor = this;
            base.proto = NumberPrototype.ob;
        }

        internal NumberConstructor(LenientFunctionPrototype parent, LenientNumberPrototype prototypeProp) : base(parent, "Number", 1)
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
                return 0;
            }
            return Microsoft.JScript.Convert.ToNumber(args[0]);
        }

        internal NumberObject Construct()
        {
            return new NumberObject(this.originalPrototype, 0.0, false);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        internal NumberObject ConstructImplicitWrapper(object arg)
        {
            return new NumberObject(this.originalPrototype, arg, true);
        }

        internal NumberObject ConstructWrapper(object arg)
        {
            return new NumberObject(this.originalPrototype, arg, false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public NumberObject CreateInstance(params object[] args)
        {
            if (args.Length == 0)
            {
                return new NumberObject(this.originalPrototype, 0.0, false);
            }
            return new NumberObject(this.originalPrototype, Microsoft.JScript.Convert.ToNumber(args[0]), false);
        }

        public double Invoke(object arg)
        {
            return Microsoft.JScript.Convert.ToNumber(arg);
        }
    }
}

