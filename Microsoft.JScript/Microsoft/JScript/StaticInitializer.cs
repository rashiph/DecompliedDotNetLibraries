namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class StaticInitializer : AST
    {
        private Completion completion;
        private FunctionObject func;

        internal StaticInitializer(Context context, Block body, FunctionScope own_scope) : base(context)
        {
            this.func = new FunctionObject(null, new ParameterDeclaration[0], null, body, own_scope, base.Globals.ScopeStack.Peek(), context, MethodAttributes.Static | MethodAttributes.Private);
            this.func.isMethod = true;
            this.func.hasArgumentsObject = false;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            this.func.Call(new object[0], ((IActivationObject) base.Globals.ScopeStack.Peek()).GetGlobalScope());
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            this.func.PartiallyEvaluate();
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            this.func.TranslateBodyToIL(il, base.compilerGlobals);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            throw new JScriptException(JSError.InternalError, base.context);
        }
    }
}

