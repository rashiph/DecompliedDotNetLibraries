namespace Microsoft.JScript
{
    using System;
    using System.Text;

    public class StringConstructor : ScriptFunction
    {
        internal static readonly StringConstructor ob = new StringConstructor();
        private StringPrototype originalPrototype;

        internal StringConstructor() : base(FunctionPrototype.ob, "String", 1)
        {
            this.originalPrototype = StringPrototype.ob;
            StringPrototype._constructor = this;
            base.proto = StringPrototype.ob;
        }

        internal StringConstructor(FunctionPrototype parent, LenientStringPrototype prototypeProp) : base(parent, "String", 1)
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
                return "";
            }
            return Microsoft.JScript.Convert.ToString(args[0]);
        }

        internal StringObject Construct()
        {
            return new StringObject(this.originalPrototype, "", false);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        internal StringObject ConstructImplicitWrapper(string arg)
        {
            return new StringObject(this.originalPrototype, arg, true);
        }

        internal StringObject ConstructWrapper(string arg)
        {
            return new StringObject(this.originalPrototype, arg, false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public StringObject CreateInstance(params object[] args)
        {
            return new StringObject(this.originalPrototype, (args.Length == 0) ? "" : Microsoft.JScript.Convert.ToString(args[0]), false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.String_fromCharCode)]
        public static string fromCharCode(params object[] args)
        {
            StringBuilder builder = new StringBuilder(args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                builder.Append(Microsoft.JScript.Convert.ToChar(args[i]));
            }
            return builder.ToString();
        }

        public string Invoke(object arg)
        {
            return Microsoft.JScript.Convert.ToString(arg);
        }
    }
}

