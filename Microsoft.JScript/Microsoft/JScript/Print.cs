namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Print : AST
    {
        private Completion completion;
        private ASTList operand;

        internal Print(Context context, AST operand) : base(context)
        {
            this.operand = (ASTList) operand;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            object[] objArray = this.operand.EvaluateAsArray();
            for (int i = 0; i < (objArray.Length - 1); i++)
            {
                ScriptStream.Out.Write(Microsoft.JScript.Convert.ToString(objArray[i]));
            }
            if (objArray.Length > 0)
            {
                this.completion.value = Microsoft.JScript.Convert.ToString(objArray[objArray.Length - 1]);
                ScriptStream.Out.WriteLine(this.completion.value);
            }
            else
            {
                ScriptStream.Out.WriteLine("");
                this.completion.value = null;
            }
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            this.operand = (ASTList) this.operand.PartiallyEvaluate();
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (base.context.document.debugOn)
            {
                il.Emit(OpCodes.Nop);
            }
            ASTList operand = this.operand;
            int count = operand.count;
            for (int i = 0; i < count; i++)
            {
                AST ast = operand[i];
                if (ast.InferType(null) == Typeob.String)
                {
                    ast.TranslateToIL(il, Typeob.String);
                }
                else
                {
                    ast.TranslateToIL(il, Typeob.Object);
                    ConstantWrapper.TranslateToILInt(il, 1);
                    il.Emit(OpCodes.Call, CompilerGlobals.toStringMethod);
                }
                if (i == (count - 1))
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.writeLineMethod);
                }
                else
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.writeMethod);
                }
            }
            if (count == 0)
            {
                il.Emit(OpCodes.Ldstr, "");
                il.Emit(OpCodes.Call, CompilerGlobals.writeLineMethod);
            }
            if (rtype != Typeob.Void)
            {
                il.Emit(OpCodes.Ldsfld, CompilerGlobals.undefinedField);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            ASTList operand = this.operand;
            for (int i = 0; i < operand.count; i++)
            {
                operand[i].TranslateToILInitializer(il);
            }
        }
    }
}

