namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Logical_and : BinaryOp
    {
        internal Logical_and(Context context, AST operand1, AST operand2) : base(context, operand1, operand2)
        {
        }

        internal override object Evaluate()
        {
            object obj2 = base.operand1.Evaluate();
            MethodInfo method = null;
            Type type = null;
            if ((obj2 != null) && !(obj2 is IConvertible))
            {
                type = obj2.GetType();
                method = type.GetMethod("op_False", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type }, null);
                if (((method == null) || ((method.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || (method.ReturnType != Typeob.Boolean))
                {
                    method = null;
                }
            }
            if (method == null)
            {
                if (!Microsoft.JScript.Convert.ToBoolean(obj2))
                {
                    return obj2;
                }
                return base.operand2.Evaluate();
            }
            method = new JSMethodInfo(method);
            if ((bool) method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { obj2 }, null))
            {
                return obj2;
            }
            object obj3 = base.operand2.Evaluate();
            Type type2 = null;
            if ((obj3 != null) && !(obj3 is IConvertible))
            {
                type2 = obj3.GetType();
                if (type == type2)
                {
                    MethodInfo info2 = type.GetMethod("op_BitwiseAnd", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, type }, null);
                    if ((info2 != null) && ((info2.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope))
                    {
                        info2 = new JSMethodInfo(info2);
                        return info2.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { obj2, obj3 }, null);
                    }
                }
            }
            return obj3;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            IReflect reflect = base.operand1.InferType(inference_target);
            IReflect reflect2 = base.operand2.InferType(inference_target);
            if (reflect == reflect2)
            {
                return reflect;
            }
            return Typeob.Object;
        }

        internal override void TranslateToConditionalBranch(ILGenerator il, bool branchIfTrue, Label label, bool shortForm)
        {
            Label label2 = il.DefineLabel();
            if (branchIfTrue)
            {
                base.operand1.TranslateToConditionalBranch(il, false, label2, shortForm);
                base.operand2.TranslateToConditionalBranch(il, true, label, shortForm);
                il.MarkLabel(label2);
            }
            else
            {
                base.operand1.TranslateToConditionalBranch(il, false, label, shortForm);
                base.operand2.TranslateToConditionalBranch(il, false, label, shortForm);
            }
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Type type = Microsoft.JScript.Convert.ToType(base.operand1.InferType(null));
            Type type2 = Microsoft.JScript.Convert.ToType(base.operand2.InferType(null));
            if (type != type2)
            {
                type = Typeob.Object;
            }
            MethodInfo meth = type.GetMethod("op_False", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type }, null);
            if (((meth == null) || ((meth.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || (meth.ReturnType != Typeob.Boolean))
            {
                meth = null;
            }
            MethodInfo info2 = null;
            if (meth != null)
            {
                info2 = type.GetMethod("op_BitwiseAnd", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, type }, null);
            }
            if ((info2 == null) || ((info2.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope))
            {
                meth = null;
            }
            Label label = il.DefineLabel();
            base.operand1.TranslateToIL(il, type);
            il.Emit(OpCodes.Dup);
            if (meth != null)
            {
                if (type.IsValueType)
                {
                    Microsoft.JScript.Convert.EmitLdloca(il, type);
                }
                il.Emit(OpCodes.Call, meth);
                il.Emit(OpCodes.Brtrue, label);
                base.operand2.TranslateToIL(il, type);
                il.Emit(OpCodes.Call, info2);
                il.MarkLabel(label);
                Microsoft.JScript.Convert.Emit(this, il, info2.ReturnType, rtype);
            }
            else
            {
                Microsoft.JScript.Convert.Emit(this, il, type, Typeob.Boolean, true);
                il.Emit(OpCodes.Brfalse, label);
                il.Emit(OpCodes.Pop);
                base.operand2.TranslateToIL(il, type);
                il.MarkLabel(label);
                Microsoft.JScript.Convert.Emit(this, il, type, rtype);
            }
        }
    }
}

