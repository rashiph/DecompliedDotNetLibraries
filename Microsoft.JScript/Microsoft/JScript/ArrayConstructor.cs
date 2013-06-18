namespace Microsoft.JScript
{
    using System;

    public sealed class ArrayConstructor : ScriptFunction
    {
        internal static readonly ArrayConstructor ob = new ArrayConstructor();
        private ArrayPrototype originalPrototype;

        internal ArrayConstructor() : base(FunctionPrototype.ob, "Array", 1)
        {
            this.originalPrototype = ArrayPrototype.ob;
            ArrayPrototype._constructor = this;
            base.proto = ArrayPrototype.ob;
        }

        internal ArrayConstructor(LenientFunctionPrototype parent, LenientArrayPrototype prototypeProp) : base(parent, "Array", 1)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            return this.Construct(args);
        }

        internal ArrayObject Construct()
        {
            return new ArrayObject(this.originalPrototype, typeof(ArrayObject));
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        public ArrayObject ConstructArray(object[] args)
        {
            ArrayObject obj2 = new ArrayObject(this.originalPrototype, typeof(ArrayObject)) {
                length = args.Length
            };
            for (int i = 0; i < args.Length; i++)
            {
                obj2.SetValueAtIndex((uint) i, args[i]);
            }
            return obj2;
        }

        internal ArrayObject ConstructImplicitWrapper(Array arr)
        {
            return new ArrayWrapper(this.originalPrototype, arr, true);
        }

        internal ArrayObject ConstructWrapper()
        {
            return new ArrayWrapper(this.originalPrototype, null, false);
        }

        internal ArrayObject ConstructWrapper(Array arr)
        {
            return new ArrayWrapper(this.originalPrototype, arr, false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public ArrayObject CreateInstance(params object[] args)
        {
            ArrayObject obj2 = new ArrayObject(this.originalPrototype, typeof(ArrayObject));
            if (args.Length != 0)
            {
                if (args.Length == 1)
                {
                    object ob = args[0];
                    IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                    switch (Microsoft.JScript.Convert.GetTypeCode(ob, iConvertible))
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                        {
                            double num = Microsoft.JScript.Convert.ToNumber(ob, iConvertible);
                            uint num2 = Microsoft.JScript.Convert.ToUint32(ob, iConvertible);
                            if (num != num2)
                            {
                                throw new JScriptException(JSError.ArrayLengthConstructIncorrect);
                            }
                            obj2.length = num2;
                            return obj2;
                        }
                    }
                }
                if ((args.Length == 1) && (args[0] is Array))
                {
                    Array array = (Array) args[0];
                    if (array.Rank != 1)
                    {
                        throw new JScriptException(JSError.TypeMismatch);
                    }
                    obj2.length = array.Length;
                    for (int j = 0; j < array.Length; j++)
                    {
                        obj2.SetValueAtIndex((uint) j, array.GetValue(j));
                    }
                    return obj2;
                }
                obj2.length = args.Length;
                for (int i = 0; i < args.Length; i++)
                {
                    obj2.SetValueAtIndex((uint) i, args[i]);
                }
            }
            return obj2;
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public ArrayObject Invoke(params object[] args)
        {
            if ((args.Length == 1) && (args[0] is Array))
            {
                return this.ConstructWrapper((Array) args[0]);
            }
            return this.CreateInstance(args);
        }
    }
}

