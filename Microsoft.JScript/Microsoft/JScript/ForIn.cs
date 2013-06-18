namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class ForIn : AST
    {
        private AST body;
        private AST collection;
        private Completion completion;
        private Context inExpressionContext;
        private AST initializer;
        private AST var;

        internal ForIn(Context context, AST var, AST initializer, AST collection, AST body) : base(context)
        {
            if (var != null)
            {
                this.var = var;
                this.inExpressionContext = this.var.context.Clone();
            }
            else
            {
                VariableDeclaration declaration = (VariableDeclaration) initializer;
                this.var = declaration.identifier;
                if (declaration.initializer == null)
                {
                    declaration.initializer = new ConstantWrapper(null, null);
                }
                this.inExpressionContext = initializer.context.Clone();
            }
            this.initializer = initializer;
            this.collection = collection;
            this.inExpressionContext.UpdateWith(this.collection.context);
            this.body = body;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            AST var = this.var;
            if (this.initializer != null)
            {
                this.initializer.Evaluate();
            }
            this.completion.Continue = 0;
            this.completion.Exit = 0;
            this.completion.value = null;
            object coll = Microsoft.JScript.Convert.ToForInObject(this.collection.Evaluate(), base.Engine);
            IEnumerator enumerator = null;
            try
            {
                enumerator = JScriptGetEnumerator(coll);
                goto Label_00F4;
            }
            catch (JScriptException exception)
            {
                exception.context = this.collection.context;
                throw exception;
            }
        Label_0078:
            var.SetValue(enumerator.Current);
            Completion completion = (Completion) this.body.Evaluate();
            this.completion.value = completion.value;
            if (completion.Continue > 1)
            {
                this.completion.Continue = completion.Continue - 1;
                goto Label_00FF;
            }
            if (completion.Exit > 0)
            {
                this.completion.Exit = completion.Exit - 1;
                goto Label_00FF;
            }
            if (completion.Return)
            {
                return completion;
            }
        Label_00F4:
            if (enumerator.MoveNext())
            {
                goto Label_0078;
            }
        Label_00FF:
            return this.completion;
        }

        public static IEnumerator JScriptGetEnumerator(object coll)
        {
            if (coll is IEnumerator)
            {
                return (IEnumerator) coll;
            }
            if (coll is ScriptObject)
            {
                return new ScriptObjectPropertyEnumerator((ScriptObject) coll);
            }
            if (coll is Array)
            {
                Array array = (Array) coll;
                return new RangeEnumerator(array.GetLowerBound(0), array.GetUpperBound(0));
            }
            if (!(coll is IEnumerable))
            {
                throw new JScriptException(JSError.NotCollection);
            }
            IEnumerator enumerator = ((IEnumerable) coll).GetEnumerator();
            if (enumerator != null)
            {
                return enumerator;
            }
            return new ScriptObjectPropertyEnumerator(new JSObject());
        }

        internal override AST PartiallyEvaluate()
        {
            this.var = this.var.PartiallyEvaluateAsReference();
            this.var.SetPartialValue(new ConstantWrapper(null, null));
            if (this.initializer != null)
            {
                this.initializer = this.initializer.PartiallyEvaluate();
            }
            this.collection = this.collection.PartiallyEvaluate();
            IReflect reflect = this.collection.InferType(null);
            if ((((reflect is ClassScope) && ((ClassScope) reflect).noExpando) && !((ClassScope) reflect).ImplementsInterface(Typeob.IEnumerable)) || ((((reflect != Typeob.Object) && (reflect is Type)) && (!Typeob.ScriptObject.IsAssignableFrom((Type) reflect) && !Typeob.IEnumerable.IsAssignableFrom((Type) reflect))) && (!Typeob.IConvertible.IsAssignableFrom((Type) reflect) && !Typeob.IEnumerator.IsAssignableFrom((Type) reflect))))
            {
                this.collection.context.HandleError(JSError.NotCollection);
            }
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                FunctionScope scope = (FunctionScope) parent;
                BitArray definedFlags = scope.DefinedFlags;
                this.body = this.body.PartiallyEvaluate();
                scope.DefinedFlags = definedFlags;
            }
            else
            {
                this.body = this.body.PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label item = il.DefineLabel();
            Label label2 = il.DefineLabel();
            Label loc = il.DefineLabel();
            base.compilerGlobals.BreakLabelStack.Push(label2);
            base.compilerGlobals.ContinueLabelStack.Push(item);
            if (this.initializer != null)
            {
                this.initializer.TranslateToIL(il, Typeob.Void);
            }
            this.inExpressionContext.EmitLineInfo(il);
            this.collection.TranslateToIL(il, Typeob.Object);
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.toForInObjectMethod);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptGetEnumeratorMethod);
            LocalBuilder local = il.DeclareLocal(Typeob.IEnumerator);
            il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Br, item);
            il.MarkLabel(loc);
            this.body.TranslateToIL(il, Typeob.Void);
            il.MarkLabel(item);
            base.context.EmitLineInfo(il);
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Callvirt, CompilerGlobals.moveNextMethod);
            il.Emit(OpCodes.Brfalse, label2);
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Callvirt, CompilerGlobals.getCurrentMethod);
            Type localType = Microsoft.JScript.Convert.ToType(this.var.InferType(null));
            LocalBuilder builder2 = il.DeclareLocal(localType);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, localType);
            il.Emit(OpCodes.Stloc, builder2);
            this.var.TranslateToILPreSet(il);
            il.Emit(OpCodes.Ldloc, builder2);
            this.var.TranslateToILSet(il);
            il.Emit(OpCodes.Br, loc);
            il.MarkLabel(label2);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.var.TranslateToILInitializer(il);
            if (this.initializer != null)
            {
                this.initializer.TranslateToILInitializer(il);
            }
            this.collection.TranslateToILInitializer(il);
            this.body.TranslateToILInitializer(il);
        }
    }
}

