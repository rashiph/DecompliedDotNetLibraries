namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection.Emit;

    public sealed class With : AST
    {
        private AST block;
        private Completion completion;
        private FunctionScope enclosing_function;
        private AST obj;

        internal With(Context context, AST obj, AST block) : base(context)
        {
            this.obj = obj;
            this.block = block;
            this.completion = new Completion();
            ScriptObject obj2 = base.Globals.ScopeStack.Peek();
            if (obj2 is FunctionScope)
            {
                this.enclosing_function = (FunctionScope) obj2;
            }
            else
            {
                this.enclosing_function = null;
            }
        }

        internal override object Evaluate()
        {
            try
            {
                JScriptWith(this.obj.Evaluate(), base.Engine);
            }
            catch (JScriptException exception)
            {
                exception.context = this.obj.context;
                throw exception;
            }
            Completion completion = null;
            try
            {
                completion = (Completion) this.block.Evaluate();
            }
            finally
            {
                base.Globals.ScopeStack.Pop();
            }
            if (completion.Continue > 1)
            {
                this.completion.Continue = completion.Continue - 1;
            }
            else
            {
                this.completion.Continue = 0;
            }
            if (completion.Exit > 0)
            {
                this.completion.Exit = completion.Exit - 1;
            }
            else
            {
                this.completion.Exit = 0;
            }
            if (completion.Return)
            {
                return completion;
            }
            return this.completion;
        }

        public static object JScriptWith(object withOb, VsaEngine engine)
        {
            object obj2 = Microsoft.JScript.Convert.ToObject(withOb, engine);
            if (obj2 == null)
            {
                throw new JScriptException(JSError.ObjectExpected);
            }
            Globals globals = engine.Globals;
            globals.ScopeStack.GuardedPush(new WithObject(globals.ScopeStack.Peek(), obj2));
            return obj2;
        }

        internal override AST PartiallyEvaluate()
        {
            WithObject obj2;
            this.obj = this.obj.PartiallyEvaluate();
            if (this.obj is ConstantWrapper)
            {
                object obj3 = Microsoft.JScript.Convert.ToObject(this.obj.Evaluate(), base.Engine);
                obj2 = new WithObject(base.Globals.ScopeStack.Peek(), obj3);
                if ((obj3 is JSObject) && ((JSObject) obj3).noExpando)
                {
                    obj2.isKnownAtCompileTime = true;
                }
            }
            else
            {
                obj2 = new WithObject(base.Globals.ScopeStack.Peek(), new JSObject(null, false));
            }
            base.Globals.ScopeStack.Push(obj2);
            try
            {
                this.block = this.block.PartiallyEvaluate();
            }
            finally
            {
                base.Globals.ScopeStack.Pop();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.context.EmitLineInfo(il);
            base.Globals.ScopeStack.Push(new WithObject(base.Globals.ScopeStack.Peek(), new JSObject(null, false)));
            bool insideProtectedRegion = base.compilerGlobals.InsideProtectedRegion;
            base.compilerGlobals.InsideProtectedRegion = true;
            Label item = il.DefineLabel();
            base.compilerGlobals.BreakLabelStack.Push(item);
            base.compilerGlobals.ContinueLabelStack.Push(item);
            this.obj.TranslateToIL(il, Typeob.Object);
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptWithMethod);
            LocalBuilder local = null;
            if (base.context.document.debugOn)
            {
                il.BeginScope();
                local = il.DeclareLocal(Typeob.Object);
                local.SetLocalSymInfo("with()");
                il.Emit(OpCodes.Stloc, local);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }
            il.BeginExceptionBlock();
            this.block.TranslateToILInitializer(il);
            this.block.TranslateToIL(il, Typeob.Void);
            il.BeginFinallyBlock();
            if (base.context.document.debugOn)
            {
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, local);
            }
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.popScriptObjectMethod);
            il.Emit(OpCodes.Pop);
            il.EndExceptionBlock();
            if (base.context.document.debugOn)
            {
                il.EndScope();
            }
            il.MarkLabel(item);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
            base.compilerGlobals.InsideProtectedRegion = insideProtectedRegion;
            base.Globals.ScopeStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.obj.TranslateToILInitializer(il);
        }
    }
}

