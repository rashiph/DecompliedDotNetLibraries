namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Instanceof : BinaryOp
    {
        internal Instanceof(Context context, AST operand1, AST operand2) : base(context, operand1, operand2)
        {
        }

        internal override object Evaluate()
        {
            object obj4;
            object obj2 = base.operand1.Evaluate();
            object obj3 = base.operand2.Evaluate();
            try
            {
                obj4 = JScriptInstanceof(obj2, obj3);
            }
            catch (JScriptException exception)
            {
                if (exception.context == null)
                {
                    exception.context = base.operand2.context;
                }
                throw exception;
            }
            return obj4;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.Boolean;
        }

        public static bool JScriptInstanceof(object v1, object v2)
        {
            if (v2 is ClassScope)
            {
                return ((ClassScope) v2).HasInstance(v1);
            }
            if (v2 is ScriptFunction)
            {
                return ((ScriptFunction) v2).HasInstance(v1);
            }
            if (v1 == null)
            {
                return false;
            }
            if (v2 is Type)
            {
                Type c = v1.GetType();
                if (v1 is IConvertible)
                {
                    try
                    {
                        Microsoft.JScript.Convert.CoerceT(v1, (Type) v2);
                        return true;
                    }
                    catch (JScriptException)
                    {
                        return false;
                    }
                }
                return ((Type) v2).IsAssignableFrom(c);
            }
            if (!(v2 is IDebugType))
            {
                throw new JScriptException(JSError.NeedType);
            }
            return ((IDebugType) v2).HasInstance(v1);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.operand1.TranslateToIL(il, Typeob.Object);
            object obj2 = null;
            if (((base.operand2 is ConstantWrapper) && ((obj2 = base.operand2.Evaluate()) is Type)) && !((Type) obj2).IsValueType)
            {
                il.Emit(OpCodes.Isinst, (Type) obj2);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Cgt_Un);
            }
            else if (obj2 is ClassScope)
            {
                il.Emit(OpCodes.Isinst, ((ClassScope) obj2).GetTypeBuilderOrEnumBuilder());
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Cgt_Un);
            }
            else
            {
                base.operand2.TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.jScriptInstanceofMethod);
            }
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
        }
    }
}

