namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection.Emit;

    internal sealed class Conditional : AST
    {
        private AST condition;
        private AST operand1;
        private AST operand2;

        internal Conditional(Context context, AST condition, AST operand1, AST operand2) : base(context)
        {
            this.condition = condition;
            this.operand1 = operand1;
            this.operand2 = operand2;
        }

        internal override object Evaluate()
        {
            if (Microsoft.JScript.Convert.ToBoolean(this.condition.Evaluate()))
            {
                return this.operand1.Evaluate();
            }
            return this.operand2.Evaluate();
        }

        internal override AST PartiallyEvaluate()
        {
            this.condition = this.condition.PartiallyEvaluate();
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                FunctionScope scope = (FunctionScope) parent;
                BitArray definedFlags = scope.DefinedFlags;
                this.operand1 = this.operand1.PartiallyEvaluate();
                BitArray array2 = scope.DefinedFlags;
                scope.DefinedFlags = definedFlags;
                this.operand2 = this.operand2.PartiallyEvaluate();
                BitArray array3 = scope.DefinedFlags;
                int length = array2.Length;
                int num2 = array3.Length;
                if (length < num2)
                {
                    array2.Length = num2;
                }
                if (num2 < length)
                {
                    array3.Length = length;
                }
                scope.DefinedFlags = array2.And(array3);
            }
            else
            {
                this.operand1 = this.operand1.PartiallyEvaluate();
                this.operand2 = this.operand2.PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label label = il.DefineLabel();
            Label label2 = il.DefineLabel();
            this.condition.TranslateToConditionalBranch(il, false, label, false);
            this.operand1.TranslateToIL(il, rtype);
            il.Emit(OpCodes.Br, label2);
            il.MarkLabel(label);
            this.operand2.TranslateToIL(il, rtype);
            il.MarkLabel(label2);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.condition.TranslateToILInitializer(il);
            this.operand1.TranslateToILInitializer(il);
            this.operand2.TranslateToILInitializer(il);
        }
    }
}

