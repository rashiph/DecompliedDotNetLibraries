namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Eval : AST
    {
        private FunctionScope enclosingFunctionScope;
        private AST operand;
        private AST unsafeOption;

        internal Eval(Context context, AST operand, AST unsafeOption) : base(context)
        {
            this.operand = operand;
            this.unsafeOption = unsafeOption;
            ScriptObject obj2 = base.Globals.ScopeStack.Peek();
            ((IActivationObject) obj2).GetGlobalScope().evilScript = true;
            if (obj2 is ActivationObject)
            {
                ((ActivationObject) obj2).isKnownAtCompileTime = base.Engine.doFast;
            }
            if (obj2 is FunctionScope)
            {
                this.enclosingFunctionScope = (FunctionScope) obj2;
                this.enclosingFunctionScope.mustSaveStackLocals = true;
                for (ScriptObject obj3 = this.enclosingFunctionScope.GetParent(); obj3 != null; obj3 = obj3.GetParent())
                {
                    FunctionScope scope = obj3 as FunctionScope;
                    if (scope != null)
                    {
                        scope.mustSaveStackLocals = true;
                        scope.closuresMightEscape = true;
                    }
                }
            }
            else
            {
                this.enclosingFunctionScope = null;
            }
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
        }

        private static object DoEvaluate(object source, VsaEngine engine, bool isUnsafe)
        {
            object obj2;
            if (engine.doFast)
            {
                engine.PushScriptObject(new BlockScope(engine.ScriptObjectStackTop()));
            }
            try
            {
                Context context = new Context(new DocumentContext("eval code", engine), ((IConvertible) source).ToString());
                JSParser parser = new JSParser(context);
                obj2 = ((Completion) parser.ParseEvalBody().PartiallyEvaluate().Evaluate()).value;
            }
            finally
            {
                if (engine.doFast)
                {
                    engine.PopScriptObject();
                }
            }
            return obj2;
        }

        internal override object Evaluate()
        {
            object obj4;
            if (VsaEngine.executeForJSEE)
            {
                throw new JScriptException(JSError.NonSupportedInDebugger);
            }
            object source = this.operand.Evaluate();
            object unsafeOption = null;
            if (this.unsafeOption != null)
            {
                unsafeOption = this.unsafeOption.Evaluate();
            }
            base.Globals.CallContextStack.Push(new CallContext(base.context, null, new object[] { source, unsafeOption }));
            try
            {
                obj4 = JScriptEvaluate(source, unsafeOption, base.Engine);
            }
            catch (JScriptException exception)
            {
                if (exception.context == null)
                {
                    exception.context = base.context;
                }
                throw exception;
            }
            catch (Exception exception2)
            {
                throw new JScriptException(exception2, base.context);
            }
            finally
            {
                base.Globals.CallContextStack.Pop();
            }
            return obj4;
        }

        public static object JScriptEvaluate(object source, VsaEngine engine)
        {
            if (Microsoft.JScript.Convert.GetTypeCode(source) != TypeCode.String)
            {
                return source;
            }
            return DoEvaluate(source, engine, true);
        }

        public static object JScriptEvaluate(object source, object unsafeOption, VsaEngine engine)
        {
            if (Microsoft.JScript.Convert.GetTypeCode(source) != TypeCode.String)
            {
                return source;
            }
            bool isUnsafe = false;
            if ((Microsoft.JScript.Convert.GetTypeCode(unsafeOption) == TypeCode.String) && (((IConvertible) unsafeOption).ToString() == "unsafe"))
            {
                isUnsafe = true;
            }
            return DoEvaluate(source, engine, isUnsafe);
        }

        internal override AST PartiallyEvaluate()
        {
            AST ast;
            VsaEngine engine = base.Engine;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            ClassScope scope = ClassScope.ScopeOfClassMemberInitializer(parent);
            if (scope != null)
            {
                if (scope.inStaticInitializerCode)
                {
                    scope.staticInitializerUsesEval = true;
                }
                else
                {
                    scope.instanceInitializerUsesEval = true;
                }
            }
            if (!engine.doFast)
            {
                while ((parent is WithObject) || (parent is BlockScope))
                {
                    if (parent is BlockScope)
                    {
                        ((BlockScope) parent).isKnownAtCompileTime = false;
                    }
                    parent = parent.GetParent();
                }
            }
            else
            {
                engine.PushScriptObject(new BlockScope(parent));
            }
            try
            {
                this.operand = this.operand.PartiallyEvaluate();
                if (this.unsafeOption != null)
                {
                    this.unsafeOption = this.unsafeOption.PartiallyEvaluate();
                }
                if ((this.enclosingFunctionScope != null) && (this.enclosingFunctionScope.owner == null))
                {
                    base.context.HandleError(JSError.NotYetImplemented);
                }
                ast = this;
            }
            finally
            {
                if (engine.doFast)
                {
                    base.Engine.PopScriptObject();
                }
            }
            return ast;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if ((this.enclosingFunctionScope != null) && (this.enclosingFunctionScope.owner != null))
            {
                this.enclosingFunctionScope.owner.TranslateToILToSaveLocals(il);
            }
            this.operand.TranslateToIL(il, Typeob.Object);
            MethodInfo meth = null;
            ConstantWrapper unsafeOption = this.unsafeOption as ConstantWrapper;
            if (unsafeOption != null)
            {
                string str = unsafeOption.value as string;
                if ((str != null) && (str == "unsafe"))
                {
                    meth = CompilerGlobals.jScriptEvaluateMethod1;
                }
            }
            if (meth == null)
            {
                meth = CompilerGlobals.jScriptEvaluateMethod2;
                if (this.unsafeOption == null)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    this.unsafeOption.TranslateToIL(il, Typeob.Object);
                }
            }
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, meth);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
            if ((this.enclosingFunctionScope != null) && (this.enclosingFunctionScope.owner != null))
            {
                this.enclosingFunctionScope.owner.TranslateToILToRestoreLocals(il);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.operand.TranslateToILInitializer(il);
            if (this.unsafeOption != null)
            {
                this.unsafeOption.TranslateToILInitializer(il);
            }
        }
    }
}

