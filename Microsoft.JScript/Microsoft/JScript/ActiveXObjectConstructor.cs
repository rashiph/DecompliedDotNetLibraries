namespace Microsoft.JScript
{
    using System;

    public sealed class ActiveXObjectConstructor : ScriptFunction
    {
        internal static readonly ActiveXObjectConstructor ob = new ActiveXObjectConstructor();

        internal ActiveXObjectConstructor() : base(FunctionPrototype.ob, "ActiveXObject", 1)
        {
        }

        internal ActiveXObjectConstructor(LenientFunctionPrototype parent) : base(parent, "ActiveXObject", 1)
        {
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
        public object CreateInstance(params object[] args)
        {
            object obj2;
            if ((args.Length == 0) || (args[0].GetType() != typeof(string)))
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            string progID = args[0].ToString();
            string server = null;
            if (args.Length == 2)
            {
                if (args[1].GetType() != typeof(string))
                {
                    throw new JScriptException(JSError.TypeMismatch);
                }
                server = args[1].ToString();
            }
            try
            {
                Type typeFromProgID = null;
                if (server == null)
                {
                    typeFromProgID = Type.GetTypeFromProgID(progID);
                }
                else
                {
                    typeFromProgID = Type.GetTypeFromProgID(progID, server);
                }
                if (!typeFromProgID.IsPublic && (typeFromProgID.Assembly == typeof(ActiveXObjectConstructor).Assembly))
                {
                    throw new JScriptException(JSError.CantCreateObject);
                }
                obj2 = Activator.CreateInstance(typeFromProgID);
            }
            catch
            {
                throw new JScriptException(JSError.CantCreateObject);
            }
            return obj2;
        }

        internal override bool HasInstance(object ob)
        {
            if (ob is JSObject)
            {
                return false;
            }
            return true;
        }

        public object Invoke()
        {
            return null;
        }
    }
}

