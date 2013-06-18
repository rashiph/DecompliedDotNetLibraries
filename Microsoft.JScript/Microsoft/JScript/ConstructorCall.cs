namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal class ConstructorCall : AST
    {
        internal ASTList arguments;
        internal bool isOK;
        internal bool isSuperConstructorCall;

        internal ConstructorCall(Context context, ASTList arguments, bool isSuperConstructorCall) : base(context)
        {
            this.isOK = false;
            this.isSuperConstructorCall = isSuperConstructorCall;
            if (arguments == null)
            {
                this.arguments = new ASTList(context);
            }
            else
            {
                this.arguments = arguments;
            }
        }

        internal override object Evaluate()
        {
            return new Completion();
        }

        internal override AST PartiallyEvaluate()
        {
            if (!this.isOK)
            {
                base.context.HandleError(JSError.NotOKToCallSuper);
                return this;
            }
            int num = 0;
            int count = this.arguments.count;
            while (num < count)
            {
                this.arguments[num] = this.arguments[num].PartiallyEvaluate();
                this.arguments[num].CheckIfOKToUseInSuperConstructorCall();
                num++;
            }
            ScriptObject obj2 = base.Globals.ScopeStack.Peek();
            if (!(obj2 is FunctionScope))
            {
                base.context.HandleError(JSError.NotOKToCallSuper);
                return this;
            }
            if (!((FunctionScope) obj2).owner.isConstructor)
            {
                base.context.HandleError(JSError.NotOKToCallSuper);
            }
            ((FunctionScope) obj2).owner.superConstructorCall = this;
            return this;
        }

        internal override AST PartiallyEvaluateAsReference()
        {
            throw new JScriptException(JSError.InternalError);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }
    }
}

