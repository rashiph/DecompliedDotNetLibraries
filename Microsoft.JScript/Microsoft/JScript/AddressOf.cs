namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class AddressOf : UnaryOp
    {
        internal AddressOf(Context context, AST operand) : base(context, operand)
        {
        }

        internal override object Evaluate()
        {
            return base.operand.Evaluate();
        }

        internal FieldInfo GetField()
        {
            if (base.operand is Binding)
            {
                MemberInfo member = ((Binding) base.operand).member;
                if (member is FieldInfo)
                {
                    return (FieldInfo) member;
                }
            }
            return null;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return base.operand.InferType(inference_target);
        }

        internal override AST PartiallyEvaluate()
        {
            base.operand = base.operand.PartiallyEvaluate();
            if (!(base.operand is Binding) || !((Binding) base.operand).RefersToMemoryLocation())
            {
                base.context.HandleError(JSError.DoesNotHaveAnAddress);
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.operand.TranslateToIL(il, rtype);
        }

        internal override void TranslateToILPreSet(ILGenerator il)
        {
            base.operand.TranslateToILPreSet(il);
        }

        internal override object TranslateToILReference(ILGenerator il, Type rtype)
        {
            return base.operand.TranslateToILReference(il, rtype);
        }

        internal override void TranslateToILSet(ILGenerator il, AST rhvalue)
        {
            base.operand.TranslateToILSet(il, rhvalue);
        }
    }
}

