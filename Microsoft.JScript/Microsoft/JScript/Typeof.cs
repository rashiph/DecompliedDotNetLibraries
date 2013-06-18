namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Typeof : UnaryOp
    {
        internal Typeof(Context context, AST operand) : base(context, operand)
        {
        }

        internal override object Evaluate()
        {
            try
            {
                return JScriptTypeof(base.operand.Evaluate(), VsaEngine.executeForJSEE);
            }
            catch (JScriptException exception)
            {
                if ((exception.Number & 0xffff) != 0x1391)
                {
                    throw exception;
                }
                return "undefined";
            }
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.String;
        }

        public static string JScriptTypeof(object value)
        {
            return JScriptTypeof(value, false);
        }

        internal static string JScriptTypeof(object value, bool checkForDebuggerObject)
        {
            switch (Microsoft.JScript.Convert.GetTypeCode(value))
            {
                case TypeCode.Empty:
                    return "undefined";

                case TypeCode.Object:
                    if ((value is Microsoft.JScript.Missing) || (value is System.Reflection.Missing))
                    {
                        return "undefined";
                    }
                    if (checkForDebuggerObject)
                    {
                        IDebuggerObject obj2 = value as IDebuggerObject;
                        if (obj2 != null)
                        {
                            if (!obj2.IsScriptFunction())
                            {
                                return "object";
                            }
                            return "function";
                        }
                    }
                    if (value is ScriptFunction)
                    {
                        return "function";
                    }
                    return "object";

                case TypeCode.DBNull:
                    return "object";

                case TypeCode.Boolean:
                    return "boolean";

                case TypeCode.Char:
                case TypeCode.String:
                    return "string";

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
                    return "number";

                case TypeCode.DateTime:
                    return "date";
            }
            return "unknown";
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (base.operand is Binding)
            {
                ((Binding) base.operand).TranslateToIL(il, Typeob.Object, true);
            }
            else
            {
                base.operand.TranslateToIL(il, Typeob.Object);
            }
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptTypeofMethod);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.String, rtype);
        }
    }
}

