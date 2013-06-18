namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public abstract class AST
    {
        internal Context context;

        internal AST(Context context)
        {
            this.context = context;
        }

        internal virtual void CheckIfOKToUseInSuperConstructorCall()
        {
        }

        internal virtual bool Delete()
        {
            return true;
        }

        internal void EmitILToLoadEngine(ILGenerator il)
        {
            ScriptObject parent = this.Engine.ScriptObjectStackTop();
            while ((parent != null) && ((parent is WithObject) || (parent is BlockScope)))
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                ((FunctionScope) parent).owner.TranslateToILToLoadEngine(il);
            }
            else if (parent is ClassScope)
            {
                if (this.Engine.doCRS)
                {
                    il.Emit(OpCodes.Ldsfld, CompilerGlobals.contextEngineField);
                }
                else if (this.context.document.engine.PEFileKind == PEFileKinds.Dll)
                {
                    il.Emit(OpCodes.Ldtoken, ((ClassScope) parent).GetTypeBuilder());
                    il.Emit(OpCodes.Call, CompilerGlobals.createVsaEngineWithType);
                }
                else
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.createVsaEngine);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, CompilerGlobals.engineField);
            }
        }

        internal abstract object Evaluate();
        internal virtual LateBinding EvaluateAsLateBinding()
        {
            return new LateBinding(null, this.Evaluate(), VsaEngine.executeForJSEE);
        }

        internal virtual WrappedNamespace EvaluateAsWrappedNamespace(bool giveErrorIfNameInUse)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual Context GetFirstExecutableContext()
        {
            return this.context;
        }

        internal virtual bool HasReturn()
        {
            return false;
        }

        internal virtual IReflect InferType(JSField inference_target)
        {
            return Typeob.Object;
        }

        internal virtual void InvalidateInferredTypes()
        {
        }

        internal virtual bool OkToUseAsType()
        {
            return false;
        }

        internal abstract AST PartiallyEvaluate();
        internal virtual AST PartiallyEvaluateAsCallable()
        {
            return new CallableExpression(this.PartiallyEvaluate());
        }

        internal virtual AST PartiallyEvaluateAsReference()
        {
            return this.PartiallyEvaluate();
        }

        internal virtual void ResolveCall(ASTList args, IReflect[] argIRs, bool constructor, bool brackets)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual object ResolveCustomAttribute(ASTList args, IReflect[] argIRs, AST target)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual void SetPartialValue(AST partial_value)
        {
            this.context.HandleError(JSError.IllegalAssignment);
        }

        internal virtual void SetValue(object value)
        {
            this.context.HandleError(JSError.IllegalAssignment);
        }

        internal virtual void TranslateToConditionalBranch(ILGenerator il, bool branchIfTrue, Label label, bool shortForm)
        {
            IReflect ir = this.InferType(null);
            if ((ir != Typeob.Object) && (ir is Type))
            {
                string name = branchIfTrue ? "op_True" : "op_False";
                MethodInfo meth = ir.GetMethod(name, BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { (Type) ir }, null);
                if (meth != null)
                {
                    this.TranslateToIL(il, (Type) ir);
                    il.Emit(OpCodes.Call, meth);
                    il.Emit(OpCodes.Brtrue, label);
                    return;
                }
            }
            Type rtype = Microsoft.JScript.Convert.ToType(ir);
            this.TranslateToIL(il, rtype);
            Microsoft.JScript.Convert.Emit(this, il, rtype, Typeob.Boolean, true);
            if (branchIfTrue)
            {
                il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
            }
            else
            {
                il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
            }
        }

        internal abstract void TranslateToIL(ILGenerator il, Type rtype);
        internal virtual void TranslateToILCall(ILGenerator il, Type rtype, ASTList args, bool construct, bool brackets)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual void TranslateToILDelete(ILGenerator il, Type rtype)
        {
            if (rtype != Typeob.Void)
            {
                il.Emit(OpCodes.Ldc_I4_1);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
            }
        }

        internal virtual void TranslateToILInitializer(ILGenerator il)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual void TranslateToILPreSet(ILGenerator il)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual void TranslateToILPreSet(ILGenerator il, ASTList args)
        {
            this.TranslateToIL(il, Typeob.Object);
            args.TranslateToIL(il, Typeob.ArrayOfObject);
        }

        internal virtual void TranslateToILPreSetPlusGet(ILGenerator il)
        {
            throw new JScriptException(JSError.InternalError, this.context);
        }

        internal virtual void TranslateToILPreSetPlusGet(ILGenerator il, ASTList args, bool inBrackets)
        {
            il.Emit(OpCodes.Ldnull);
            this.TranslateToIL(il, Typeob.Object);
            il.Emit(OpCodes.Dup);
            LocalBuilder local = il.DeclareLocal(Typeob.Object);
            il.Emit(OpCodes.Stloc, local);
            args.TranslateToIL(il, Typeob.ArrayOfObject);
            il.Emit(OpCodes.Dup);
            LocalBuilder builder2 = il.DeclareLocal(Typeob.ArrayOfObject);
            il.Emit(OpCodes.Stloc, builder2);
            il.Emit(OpCodes.Ldc_I4_0);
            if (inBrackets)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            this.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.callValueMethod);
            LocalBuilder builder3 = il.DeclareLocal(Typeob.Object);
            il.Emit(OpCodes.Stloc, builder3);
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Ldloc, builder2);
            il.Emit(OpCodes.Ldloc, builder3);
        }

        internal virtual object TranslateToILReference(ILGenerator il, Type rtype)
        {
            this.TranslateToIL(il, rtype);
            LocalBuilder local = il.DeclareLocal(rtype);
            il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Ldloca, local);
            return local;
        }

        internal void TranslateToILSet(ILGenerator il)
        {
            this.TranslateToILSet(il, null);
        }

        internal virtual void TranslateToILSet(ILGenerator il, AST rhvalue)
        {
            if (rhvalue != null)
            {
                rhvalue.TranslateToIL(il, Typeob.Object);
            }
            il.Emit(OpCodes.Call, CompilerGlobals.setIndexedPropertyValueStaticMethod);
        }

        internal CompilerGlobals compilerGlobals
        {
            get
            {
                return this.context.document.compilerGlobals;
            }
        }

        internal VsaEngine Engine
        {
            get
            {
                return this.context.document.engine;
            }
        }

        internal Microsoft.JScript.Globals Globals
        {
            get
            {
                return this.context.document.engine.Globals;
            }
        }
    }
}

