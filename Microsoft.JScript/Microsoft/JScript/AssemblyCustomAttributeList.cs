namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    public sealed class AssemblyCustomAttributeList : AST
    {
        private CustomAttributeList list;
        internal bool okToUse;

        internal AssemblyCustomAttributeList(CustomAttributeList list) : base(list.context)
        {
            this.list = list;
            this.okToUse = false;
        }

        internal override object Evaluate()
        {
            return null;
        }

        internal override AST PartiallyEvaluate()
        {
            if (!this.okToUse)
            {
                base.context.HandleError(JSError.AssemblyAttributesMustBeGlobal);
            }
            return this;
        }

        internal void Process()
        {
            this.okToUse = true;
            this.list.SetTarget(this);
            this.list.PartiallyEvaluate();
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            foreach (CustomAttributeBuilder builder in this.list.GetCustomAttributeBuilders(false))
            {
                base.compilerGlobals.assemblyBuilder.SetCustomAttribute(builder);
            }
            if (rtype != Typeob.Void)
            {
                il.Emit(OpCodes.Ldnull);
                if (rtype.IsValueType)
                {
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }
    }
}

