namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices.Expando;

    public sealed class In : BinaryOp
    {
        internal In(Context context, AST operand1, AST operand2) : base(context, operand1, operand2)
        {
        }

        internal override object Evaluate()
        {
            object obj4;
            object obj2 = base.operand1.Evaluate();
            object obj3 = base.operand2.Evaluate();
            try
            {
                obj4 = JScriptIn(obj2, obj3);
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

        public static bool JScriptIn(object v1, object v2)
        {
            bool flag = false;
            if (v2 is ScriptObject)
            {
                return !(((ScriptObject) v2).GetMemberValue(Microsoft.JScript.Convert.ToString(v1)) is Microsoft.JScript.Missing);
            }
            if (v2 is Array)
            {
                Array array = (Array) v2;
                double num = Microsoft.JScript.Convert.ToNumber(v1);
                int num2 = (int) num;
                return (((num == num2) && (array.GetLowerBound(0) <= num2)) && (num2 <= array.GetUpperBound(0)));
            }
            if (v2 is IEnumerable)
            {
                if (v1 == null)
                {
                    return false;
                }
                if (v2 is IDictionary)
                {
                    return ((IDictionary) v2).Contains(v1);
                }
                if (v2 is IExpando)
                {
                    return (((IReflect) v2).GetMember(Microsoft.JScript.Convert.ToString(v1), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0);
                }
                IEnumerator enumerator = ((IEnumerable) v2).GetEnumerator();
                while (!flag && enumerator.MoveNext())
                {
                    if (v1.Equals(enumerator.Current))
                    {
                        return true;
                    }
                }
            }
            else if (v2 is IEnumerator)
            {
                if (v1 == null)
                {
                    return false;
                }
                IEnumerator enumerator2 = (IEnumerator) v2;
                while (!flag && enumerator2.MoveNext())
                {
                    if (v1.Equals(enumerator2.Current))
                    {
                        return true;
                    }
                }
            }
            else if (v2 is IDebuggerObject)
            {
                return ((IDebuggerObject) v2).HasEnumerableMember(Microsoft.JScript.Convert.ToString(v1));
            }
            throw new JScriptException(JSError.ObjectExpected);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.operand1.TranslateToIL(il, Typeob.Object);
            base.operand2.TranslateToIL(il, Typeob.Object);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptInMethod);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
        }
    }
}

