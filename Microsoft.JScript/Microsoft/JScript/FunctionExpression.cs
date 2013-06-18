namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class FunctionExpression : AST
    {
        private JSVariableField field;
        private FunctionObject func;
        private LocalBuilder func_local;
        private string name;
        private static int uniqueNumber;

        internal FunctionExpression(Context context, AST id, ParameterDeclaration[] formal_parameters, TypeExpression return_type, Block body, FunctionScope own_scope, FieldAttributes attributes) : base(context)
        {
            if (attributes != FieldAttributes.PrivateScope)
            {
                base.context.HandleError(JSError.SyntaxError);
                attributes = FieldAttributes.PrivateScope;
            }
            ScriptObject enclosingScope = base.Globals.ScopeStack.Peek();
            this.name = id.ToString();
            if (this.name.Length == 0)
            {
                this.name = "anonymous " + uniqueNumber++.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                this.AddNameTo(enclosingScope);
            }
            this.func = new FunctionObject(this.name, formal_parameters, return_type, body, own_scope, enclosingScope, base.context, MethodAttributes.Static | MethodAttributes.Public);
        }

        private void AddNameTo(ScriptObject enclosingScope)
        {
            while (enclosingScope is WithObject)
            {
                enclosingScope = enclosingScope.GetParent();
            }
            if (((IActivationObject) enclosingScope).GetLocalField(this.name) == null)
            {
                FieldInfo info;
                if (enclosingScope is ActivationObject)
                {
                    if (enclosingScope is FunctionScope)
                    {
                        info = ((ActivationObject) enclosingScope).AddNewField(this.name, null, FieldAttributes.Public);
                    }
                    else
                    {
                        info = ((ActivationObject) enclosingScope).AddNewField(this.name, null, FieldAttributes.Static | FieldAttributes.Public);
                    }
                }
                else
                {
                    info = ((StackFrame) enclosingScope).AddNewField(this.name, null, FieldAttributes.Public);
                }
                JSLocalField field = info as JSLocalField;
                if (field != null)
                {
                    field.debugOn = base.context.document.debugOn;
                    field.isDefined = true;
                }
                this.field = (JSVariableField) info;
            }
        }

        internal override object Evaluate()
        {
            if (VsaEngine.executeForJSEE)
            {
                throw new JScriptException(JSError.NonSupportedInDebugger);
            }
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            this.func.own_scope.SetParent(parent);
            Closure closure = new Closure(this.func);
            if (this.field != null)
            {
                this.field.value = closure;
            }
            return closure;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.ScriptFunction;
        }

        public static FunctionObject JScriptFunctionExpression(RuntimeTypeHandle handle, string name, string method_name, string[] formal_params, JSLocalField[] fields, bool must_save_stack_locals, bool hasArgumentsObject, string text, VsaEngine engine)
        {
            return new FunctionObject(Type.GetTypeFromHandle(handle), name, method_name, formal_params, fields, must_save_stack_locals, hasArgumentsObject, text, engine);
        }

        internal override AST PartiallyEvaluate()
        {
            ScriptObject obj2 = base.Globals.ScopeStack.Peek();
            if (ClassScope.ScopeOfClassMemberInitializer(obj2) != null)
            {
                base.context.HandleError(JSError.MemberInitializerCannotContainFuncExpr);
                return this;
            }
            ScriptObject parent = obj2;
            while ((parent is WithObject) || (parent is BlockScope))
            {
                parent = parent.GetParent();
            }
            FunctionScope scope = parent as FunctionScope;
            if (scope != null)
            {
                scope.closuresMightEscape = true;
            }
            if (parent != obj2)
            {
                this.func.own_scope.SetParent(new WithObject(new JSObject(), this.func.own_scope.GetGlobalScope()));
            }
            this.func.PartiallyEvaluate();
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (rtype != Typeob.Void)
            {
                il.Emit(OpCodes.Ldloc, this.func_local);
                il.Emit(OpCodes.Newobj, CompilerGlobals.closureConstructor);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Closure, rtype);
                if (this.field != null)
                {
                    il.Emit(OpCodes.Dup);
                    object metaData = this.field.GetMetaData();
                    if (metaData is LocalBuilder)
                    {
                        il.Emit(OpCodes.Stloc, (LocalBuilder) metaData);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stsfld, (FieldInfo) metaData);
                    }
                }
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.func.TranslateToIL(base.compilerGlobals);
            this.func_local = il.DeclareLocal(Typeob.FunctionObject);
            il.Emit(OpCodes.Ldtoken, this.func.classwriter);
            il.Emit(OpCodes.Ldstr, this.name);
            il.Emit(OpCodes.Ldstr, this.func.GetName());
            int length = this.func.formal_parameters.Length;
            ConstantWrapper.TranslateToILInt(il, length);
            il.Emit(OpCodes.Newarr, Typeob.String);
            for (int i = 0; i < length; i++)
            {
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, i);
                il.Emit(OpCodes.Ldstr, this.func.formal_parameters[i]);
                il.Emit(OpCodes.Stelem_Ref);
            }
            length = this.func.fields.Length;
            ConstantWrapper.TranslateToILInt(il, length);
            il.Emit(OpCodes.Newarr, Typeob.JSLocalField);
            for (int j = 0; j < length; j++)
            {
                JSLocalField field = this.func.fields[j];
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, j);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                ConstantWrapper.TranslateToILInt(il, field.slotNumber);
                il.Emit(OpCodes.Newobj, CompilerGlobals.jsLocalFieldConstructor);
                il.Emit(OpCodes.Stelem_Ref);
            }
            if (this.func.must_save_stack_locals)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            if (this.func.hasArgumentsObject)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            il.Emit(OpCodes.Ldstr, this.func.ToString());
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptFunctionExpressionMethod);
            il.Emit(OpCodes.Stloc, this.func_local);
        }
    }
}

