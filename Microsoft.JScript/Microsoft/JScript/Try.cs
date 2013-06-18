namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Try : AST
    {
        private AST body;
        private FieldInfo field;
        private string fieldName;
        private AST finally_block;
        private bool finallyHasControlFlowOutOfIt;
        private AST handler;
        private BlockScope handler_scope;
        private Context tryEndContext;
        private TypeExpression type;

        internal Try(Context context, AST body, AST identifier, TypeExpression type, AST handler, AST finally_block, bool finallyHasControlFlowOutOfIt, Context tryEndContext) : base(context)
        {
            this.body = body;
            this.type = type;
            this.handler = handler;
            this.finally_block = finally_block;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            this.handler_scope = null;
            this.field = null;
            if (identifier != null)
            {
                this.fieldName = identifier.ToString();
                this.field = parent.GetField(this.fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (this.field != null)
                {
                    if ((((type == null) && (this.field is JSVariableField)) && (this.field.IsStatic && (((JSVariableField) this.field).type == null))) && (!this.field.IsLiteral && !this.field.IsInitOnly))
                    {
                        return;
                    }
                    if (((IActivationObject) parent).GetLocalField(this.fieldName) != null)
                    {
                        identifier.context.HandleError(JSError.DuplicateName, false);
                    }
                }
                this.handler_scope = new BlockScope(parent);
                this.handler_scope.catchHanderScope = true;
                JSVariableField field = this.handler_scope.AddNewField(identifier.ToString(), Microsoft.JScript.Missing.Value, FieldAttributes.Public);
                this.field = field;
                field.originalContext = identifier.context;
                if (identifier.context.document.debugOn && (this.field is JSLocalField))
                {
                    this.handler_scope.AddFieldForLocalScopeDebugInfo((JSLocalField) this.field);
                }
            }
            this.finallyHasControlFlowOutOfIt = finallyHasControlFlowOutOfIt;
            this.tryEndContext = tryEndContext;
        }

        internal override object Evaluate()
        {
            int i = base.Globals.ScopeStack.Size();
            int num2 = base.Globals.CallContextStack.Size();
            Completion completion = null;
            Completion completion2 = null;
            try
            {
                object obj2 = null;
                try
                {
                    completion = (Completion) this.body.Evaluate();
                }
                catch (Exception exception)
                {
                    if (this.handler == null)
                    {
                        throw;
                    }
                    obj2 = exception;
                    if (this.type != null)
                    {
                        Type c = this.type.ToType();
                        if (!Typeob.Exception.IsAssignableFrom(c))
                        {
                            if (!c.IsInstanceOfType(obj2 = JScriptExceptionValue(exception, base.Engine)))
                            {
                                throw;
                            }
                        }
                        else if (!c.IsInstanceOfType(exception))
                        {
                            throw;
                        }
                    }
                    else
                    {
                        obj2 = JScriptExceptionValue(exception, base.Engine);
                    }
                }
                if (obj2 != null)
                {
                    base.Globals.ScopeStack.TrimToSize(i);
                    base.Globals.CallContextStack.TrimToSize(num2);
                    if (this.handler_scope != null)
                    {
                        this.handler_scope.SetParent(base.Globals.ScopeStack.Peek());
                        base.Globals.ScopeStack.Push(this.handler_scope);
                    }
                    if (this.field != null)
                    {
                        this.field.SetValue(base.Globals.ScopeStack.Peek(), obj2);
                    }
                    completion = (Completion) this.handler.Evaluate();
                }
            }
            finally
            {
                base.Globals.ScopeStack.TrimToSize(i);
                base.Globals.CallContextStack.TrimToSize(num2);
                if (this.finally_block != null)
                {
                    completion2 = (Completion) this.finally_block.Evaluate();
                }
            }
            if ((completion == null) || ((completion2 != null) && (((completion2.Exit > 0) || (completion2.Continue > 0)) || completion2.Return)))
            {
                completion = completion2;
            }
            else if ((completion2 != null) && (completion2.value is Microsoft.JScript.Missing))
            {
                completion.value = completion2.value;
            }
            return new Completion { Continue = completion.Continue - 1, Exit = completion.Exit - 1, Return = completion.Return, value = completion.value };
        }

        internal override Context GetFirstExecutableContext()
        {
            return this.body.GetFirstExecutableContext();
        }

        public static object JScriptExceptionValue(object e, VsaEngine engine)
        {
            if (engine == null)
            {
                engine = new VsaEngine(true);
                engine.InitVsaEngine("JS7://Microsoft.JScript.Vsa.VsaEngine", new DefaultVsaSite());
            }
            ErrorConstructor originalError = engine.Globals.globalObject.originalError;
            if (e is JScriptException)
            {
                object obj2 = ((JScriptException) e).value;
                if ((!(obj2 is Exception) && !(obj2 is Microsoft.JScript.Missing)) && ((((JScriptException) e).Number & 0xffff) == 0x139e))
                {
                    return obj2;
                }
                return originalError.Construct((Exception) e);
            }
            if (e is StackOverflowException)
            {
                return originalError.Construct(new JScriptException(JSError.OutOfStack));
            }
            if (e is OutOfMemoryException)
            {
                return originalError.Construct(new JScriptException(JSError.OutOfMemory));
            }
            return originalError.Construct(e);
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.type != null)
            {
                this.type.PartiallyEvaluate();
                ((JSVariableField) this.field).type = this.type;
            }
            else if (this.field is JSLocalField)
            {
                ((JSLocalField) this.field).SetInferredType(Typeob.Object, null);
            }
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            FunctionScope scope = null;
            BitArray definedFlags = null;
            if (parent is FunctionScope)
            {
                scope = (FunctionScope) parent;
                definedFlags = scope.DefinedFlags;
            }
            this.body = this.body.PartiallyEvaluate();
            if (this.handler != null)
            {
                if (this.handler_scope != null)
                {
                    base.Globals.ScopeStack.Push(this.handler_scope);
                }
                if (this.field is JSLocalField)
                {
                    ((JSLocalField) this.field).isDefined = true;
                }
                this.handler = this.handler.PartiallyEvaluate();
                if (this.handler_scope != null)
                {
                    base.Globals.ScopeStack.Pop();
                }
            }
            if (this.finally_block != null)
            {
                this.finally_block = this.finally_block.PartiallyEvaluate();
            }
            if (scope != null)
            {
                scope.DefinedFlags = definedFlags;
            }
            return this;
        }

        public static void PushHandlerScope(VsaEngine engine, string id, int scopeId)
        {
            engine.PushScriptObject(new BlockScope(engine.ScriptObjectStackTop(), id, scopeId));
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            bool insideProtectedRegion = base.compilerGlobals.InsideProtectedRegion;
            base.compilerGlobals.InsideProtectedRegion = true;
            base.compilerGlobals.BreakLabelStack.Push(base.compilerGlobals.BreakLabelStack.Peek(0));
            base.compilerGlobals.ContinueLabelStack.Push(base.compilerGlobals.ContinueLabelStack.Peek(0));
            il.BeginExceptionBlock();
            if (this.finally_block != null)
            {
                if (this.finallyHasControlFlowOutOfIt)
                {
                    il.BeginExceptionBlock();
                }
                if (this.handler != null)
                {
                    il.BeginExceptionBlock();
                }
            }
            this.body.TranslateToIL(il, Typeob.Void);
            if (this.tryEndContext != null)
            {
                this.tryEndContext.EmitLineInfo(il);
            }
            if (this.handler != null)
            {
                if (this.type == null)
                {
                    il.BeginCatchBlock(Typeob.Exception);
                    this.handler.context.EmitLineInfo(il);
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Call, CompilerGlobals.jScriptExceptionValueMethod);
                }
                else
                {
                    Type c = this.type.ToType();
                    if (Typeob.Exception.IsAssignableFrom(c))
                    {
                        il.BeginCatchBlock(c);
                        this.handler.context.EmitLineInfo(il);
                    }
                    else
                    {
                        il.BeginExceptFilterBlock();
                        this.handler.context.EmitLineInfo(il);
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.jScriptExceptionValueMethod);
                        il.Emit(OpCodes.Isinst, c);
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Cgt_Un);
                        il.BeginCatchBlock(null);
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.jScriptExceptionValueMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, c);
                    }
                }
                object obj2 = (this.field is JSVariableField) ? ((JSVariableField) this.field).GetMetaData() : this.field;
                if (obj2 is LocalBuilder)
                {
                    il.Emit(OpCodes.Stloc, (LocalBuilder) obj2);
                }
                else if (obj2 is FieldInfo)
                {
                    il.Emit(OpCodes.Stsfld, (FieldInfo) obj2);
                }
                else
                {
                    Microsoft.JScript.Convert.EmitLdarg(il, (short) obj2);
                }
                if (this.handler_scope != null)
                {
                    if (!this.handler_scope.isKnownAtCompileTime)
                    {
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Ldstr, this.fieldName);
                        ConstantWrapper.TranslateToILInt(il, this.handler_scope.scopeId);
                        il.Emit(OpCodes.Call, Typeob.Try.GetMethod("PushHandlerScope"));
                        base.Globals.ScopeStack.Push(this.handler_scope);
                        il.BeginExceptionBlock();
                    }
                    il.BeginScope();
                    if (base.context.document.debugOn)
                    {
                        this.handler_scope.EmitLocalInfoForFields(il);
                    }
                }
                this.handler.TranslateToIL(il, Typeob.Void);
                if (this.handler_scope != null)
                {
                    il.EndScope();
                    if (!this.handler_scope.isKnownAtCompileTime)
                    {
                        il.BeginFinallyBlock();
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.popScriptObjectMethod);
                        il.Emit(OpCodes.Pop);
                        base.Globals.ScopeStack.Pop();
                        il.EndExceptionBlock();
                    }
                }
                il.EndExceptionBlock();
            }
            if (this.finally_block != null)
            {
                bool insideFinally = base.compilerGlobals.InsideFinally;
                int finallyStackTop = base.compilerGlobals.FinallyStackTop;
                base.compilerGlobals.InsideFinally = true;
                base.compilerGlobals.FinallyStackTop = base.compilerGlobals.BreakLabelStack.Size();
                il.BeginFinallyBlock();
                this.finally_block.TranslateToIL(il, Typeob.Void);
                il.EndExceptionBlock();
                base.compilerGlobals.InsideFinally = insideFinally;
                base.compilerGlobals.FinallyStackTop = finallyStackTop;
                if (this.finallyHasControlFlowOutOfIt)
                {
                    il.BeginCatchBlock(Typeob.BreakOutOfFinally);
                    il.Emit(OpCodes.Ldfld, Typeob.BreakOutOfFinally.GetField("target"));
                    int i = base.compilerGlobals.BreakLabelStack.Size() - 1;
                    int num3 = i;
                    while (i > 0)
                    {
                        il.Emit(OpCodes.Dup);
                        ConstantWrapper.TranslateToILInt(il, i);
                        Label label = il.DefineLabel();
                        il.Emit(OpCodes.Blt_S, label);
                        il.Emit(OpCodes.Pop);
                        if (insideFinally && (i < finallyStackTop))
                        {
                            il.Emit(OpCodes.Rethrow);
                        }
                        else
                        {
                            il.Emit(OpCodes.Leave, (Label) base.compilerGlobals.BreakLabelStack.Peek(num3 - i));
                        }
                        il.MarkLabel(label);
                        i--;
                    }
                    il.Emit(OpCodes.Pop);
                    il.BeginCatchBlock(Typeob.ContinueOutOfFinally);
                    il.Emit(OpCodes.Ldfld, Typeob.ContinueOutOfFinally.GetField("target"));
                    int num4 = base.compilerGlobals.ContinueLabelStack.Size() - 1;
                    int num5 = num4;
                    while (num4 > 0)
                    {
                        il.Emit(OpCodes.Dup);
                        ConstantWrapper.TranslateToILInt(il, num4);
                        Label label2 = il.DefineLabel();
                        il.Emit(OpCodes.Blt_S, label2);
                        il.Emit(OpCodes.Pop);
                        if (insideFinally && (num4 < finallyStackTop))
                        {
                            il.Emit(OpCodes.Rethrow);
                        }
                        else
                        {
                            il.Emit(OpCodes.Leave, (Label) base.compilerGlobals.ContinueLabelStack.Peek(num5 - num4));
                        }
                        il.MarkLabel(label2);
                        num4--;
                    }
                    il.Emit(OpCodes.Pop);
                    ScriptObject parent = base.Globals.ScopeStack.Peek();
                    while ((parent != null) && !(parent is FunctionScope))
                    {
                        parent = parent.GetParent();
                    }
                    if ((parent != null) && !insideFinally)
                    {
                        il.BeginCatchBlock(Typeob.ReturnOutOfFinally);
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Leave, ((FunctionScope) parent).owner.returnLabel);
                    }
                    il.EndExceptionBlock();
                }
            }
            base.compilerGlobals.InsideProtectedRegion = insideProtectedRegion;
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.body.TranslateToILInitializer(il);
            if (this.handler != null)
            {
                this.handler.TranslateToILInitializer(il);
            }
            if (this.finally_block != null)
            {
                this.finally_block.TranslateToILInitializer(il);
            }
        }
    }
}

