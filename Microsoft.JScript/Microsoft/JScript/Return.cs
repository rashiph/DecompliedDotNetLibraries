namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Return : AST
    {
        private Completion completion;
        private FunctionScope enclosingFunctionScope;
        private bool leavesFinally;
        private AST operand;

        internal Return(Context context, AST operand, bool leavesFinally) : base(context)
        {
            this.completion = new Completion();
            this.completion.Return = true;
            this.operand = operand;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (!(parent is FunctionScope))
            {
                parent = parent.GetParent();
                if (parent == null)
                {
                    base.context.HandleError(JSError.BadReturn);
                    parent = new FunctionScope(null);
                }
            }
            this.enclosingFunctionScope = (FunctionScope) parent;
            if ((this.operand != null) && (this.enclosingFunctionScope.returnVar == null))
            {
                this.enclosingFunctionScope.AddReturnValueField();
            }
            this.leavesFinally = leavesFinally;
        }

        internal override object Evaluate()
        {
            if (this.operand != null)
            {
                this.completion.value = this.operand.Evaluate();
            }
            return this.completion;
        }

        internal override bool HasReturn()
        {
            return true;
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.leavesFinally)
            {
                base.context.HandleError(JSError.BadWayToLeaveFinally);
            }
            if (this.operand != null)
            {
                this.operand = this.operand.PartiallyEvaluate();
                if (this.enclosingFunctionScope.returnVar != null)
                {
                    if (this.enclosingFunctionScope.returnVar.type == null)
                    {
                        this.enclosingFunctionScope.returnVar.SetInferredType(this.operand.InferType(this.enclosingFunctionScope.returnVar), this.operand);
                    }
                    else
                    {
                        Binding.AssignmentCompatible(this.enclosingFunctionScope.returnVar.type.ToIReflect(), this.operand, this.operand.InferType(null), true);
                    }
                }
                else
                {
                    base.context.HandleError(JSError.CannotReturnValueFromVoidFunction);
                    this.operand = null;
                }
            }
            else if (this.enclosingFunctionScope.returnVar != null)
            {
                this.enclosingFunctionScope.returnVar.SetInferredType(Typeob.Object, null);
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.context.EmitLineInfo(il);
            if (this.operand != null)
            {
                this.operand.TranslateToIL(il, this.enclosingFunctionScope.returnVar.FieldType);
            }
            else if (this.enclosingFunctionScope.returnVar != null)
            {
                il.Emit(OpCodes.Ldsfld, CompilerGlobals.undefinedField);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, this.enclosingFunctionScope.returnVar.FieldType);
            }
            if (this.enclosingFunctionScope.returnVar != null)
            {
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.enclosingFunctionScope.returnVar.GetMetaData());
            }
            if (this.leavesFinally)
            {
                il.Emit(OpCodes.Newobj, CompilerGlobals.returnOutOfFinallyConstructor);
                il.Emit(OpCodes.Throw);
            }
            else if (base.compilerGlobals.InsideProtectedRegion)
            {
                il.Emit(OpCodes.Leave, this.enclosingFunctionScope.owner.returnLabel);
            }
            else
            {
                il.Emit(OpCodes.Br, this.enclosingFunctionScope.owner.returnLabel);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (this.operand != null)
            {
                this.operand.TranslateToILInitializer(il);
            }
        }
    }
}

