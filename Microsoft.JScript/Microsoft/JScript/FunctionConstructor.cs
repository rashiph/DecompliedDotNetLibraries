namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Text;

    public sealed class FunctionConstructor : ScriptFunction
    {
        internal static readonly FunctionConstructor ob = new FunctionConstructor();
        internal FunctionPrototype originalPrototype;

        internal FunctionConstructor() : base(FunctionPrototype.ob, "Function", 1)
        {
            this.originalPrototype = FunctionPrototype.ob;
            FunctionPrototype._constructor = this;
            base.proto = FunctionPrototype.ob;
        }

        internal FunctionConstructor(LenientFunctionPrototype prototypeProp) : base(prototypeProp, "Function", 1)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            return this.Construct(args, base.engine);
        }

        internal override object Construct(object[] args)
        {
            return this.Construct(args, base.engine);
        }

        internal ScriptFunction Construct(object[] args, VsaEngine engine)
        {
            ScriptFunction function;
            StringBuilder builder = new StringBuilder("function anonymous(");
            int index = 0;
            int num2 = args.Length - 2;
            while (index < num2)
            {
                builder.Append(Microsoft.JScript.Convert.ToString(args[index]));
                builder.Append(", ");
                index++;
            }
            if (args.Length > 1)
            {
                builder.Append(Microsoft.JScript.Convert.ToString(args[args.Length - 2]));
            }
            builder.Append(") {\n");
            if (args.Length > 0)
            {
                builder.Append(Microsoft.JScript.Convert.ToString(args[args.Length - 1]));
            }
            builder.Append("\n}");
            Context context = new Context(new DocumentContext("anonymous", engine), builder.ToString());
            JSParser parser = new JSParser(context);
            engine.PushScriptObject(((IActivationObject) engine.ScriptObjectStackTop()).GetGlobalScope());
            try
            {
                function = (ScriptFunction) parser.ParseFunctionExpression().PartiallyEvaluate().Evaluate();
            }
            finally
            {
                engine.PopScriptObject();
            }
            return function;
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public ScriptFunction CreateInstance(params object[] args)
        {
            return this.Construct(args, base.engine);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public ScriptFunction Invoke(params object[] args)
        {
            return this.Construct(args, base.engine);
        }
    }
}

