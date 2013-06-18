namespace Microsoft.JScript
{
    using System;
    using System.Reflection;

    public sealed class ObjectConstructor : ScriptFunction
    {
        internal static readonly ObjectConstructor ob = new ObjectConstructor();
        internal ObjectPrototype originalPrototype;

        internal ObjectConstructor() : base(FunctionPrototype.ob, "Object", 1)
        {
            this.originalPrototype = ObjectPrototype.ob;
            ObjectPrototype._constructor = this;
            base.proto = ObjectPrototype.ob;
        }

        internal ObjectConstructor(LenientFunctionPrototype parent, LenientObjectPrototype prototypeProp) : base(parent, "Object", 1)
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
                return this.ConstructObject();
            }
            object obj2 = args[0];
            if ((obj2 != null) && (obj2 != DBNull.Value))
            {
                return Microsoft.JScript.Convert.ToObject3(obj2, base.engine);
            }
            return this.Construct(args);
        }

        internal override object Construct(object[] args)
        {
            if (args.Length == 0)
            {
                return this.ConstructObject();
            }
            object ob = args[0];
            switch (Microsoft.JScript.Convert.GetTypeCode(ob))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return this.ConstructObject();

                case TypeCode.Object:
                {
                    if (ob is ScriptObject)
                    {
                        return ob;
                    }
                    IReflect type = null;
                    if (ob is IReflect)
                    {
                        type = (IReflect) ob;
                    }
                    else
                    {
                        type = ob.GetType();
                    }
                    return type.InvokeMember(string.Empty, BindingFlags.OptionalParamBinding | BindingFlags.CreateInstance | BindingFlags.Public, JSBinder.ob, ob, new object[0], null, null, null);
                }
            }
            return Microsoft.JScript.Convert.ToObject3(ob, base.engine);
        }

        public JSObject ConstructObject()
        {
            return new JSObject(this.originalPrototype, false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public object CreateInstance(params object[] args)
        {
            return this.Construct(args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public object Invoke(params object[] args)
        {
            return this.Call(args, null);
        }
    }
}

