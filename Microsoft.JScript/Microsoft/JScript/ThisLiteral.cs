namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class ThisLiteral : AST
    {
        internal bool isSuper;
        private MethodInfo method;

        internal ThisLiteral(Context context, bool isSuper) : base(context)
        {
            this.isSuper = isSuper;
            this.method = null;
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
        }

        internal override object Evaluate()
        {
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while ((parent is WithObject) || (parent is BlockScope))
            {
                parent = parent.GetParent();
            }
            if (parent is StackFrame)
            {
                return ((StackFrame) parent).thisObject;
            }
            return ((GlobalScope) parent).thisObject;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            if (this.method != null)
            {
                ParameterInfo[] parameters = this.method.GetParameters();
                if ((parameters != null) && (parameters.Length != 0))
                {
                    return parameters[0].ParameterType;
                }
                return this.method.ReturnType;
            }
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is GlobalScope)
            {
                return parent;
            }
            if (!(parent is FunctionScope) || !((FunctionScope) parent).isMethod)
            {
                return Typeob.Object;
            }
            ClassScope scope = (ClassScope) ((FunctionScope) parent).owner.enclosing_scope;
            if (this.isSuper)
            {
                return scope.GetSuperType();
            }
            return scope;
        }

        internal override AST PartiallyEvaluate()
        {
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            bool flag = false;
            if (parent is FunctionScope)
            {
                flag = ((FunctionScope) parent).isStatic && ((FunctionScope) parent).isMethod;
            }
            else if (parent is StackFrame)
            {
                flag = ((StackFrame) parent).thisObject is Type;
            }
            if (flag)
            {
                base.context.HandleError(JSError.NotAccessible);
                return new Lookup("this", base.context).PartiallyEvaluate();
            }
            return this;
        }

        internal override AST PartiallyEvaluateAsReference()
        {
            base.context.HandleError(JSError.CantAssignThis);
            return new Lookup("this", base.context).PartiallyEvaluateAsReference();
        }

        internal void ResolveAssignmentToDefaultIndexedProperty(ASTList args, IReflect[] argIRs, AST rhvalue)
        {
            string str;
            IReflect reflect = this.InferType(null);
            Type t = (reflect is Type) ? ((Type) reflect) : null;
            if (reflect is ClassScope)
            {
                t = ((ClassScope) reflect).GetBakedSuperType();
            }
            MemberInfo[] defaultMembers = JSBinder.GetDefaultMembers(t);
            if ((defaultMembers != null) && (defaultMembers.Length > 0))
            {
                try
                {
                    PropertyInfo prop = JSBinder.SelectProperty(defaultMembers, argIRs);
                    if (prop == null)
                    {
                        goto Label_00B1;
                    }
                    this.method = JSProperty.GetSetMethod(prop, true);
                    if (this.method == null)
                    {
                        base.context.HandleError(JSError.AssignmentToReadOnly, true);
                    }
                    if (!Binding.CheckParameters(prop.GetIndexParameters(), argIRs, args, base.context, 0, false, true))
                    {
                        this.method = null;
                    }
                }
                catch (AmbiguousMatchException)
                {
                    base.context.HandleError(JSError.AmbiguousMatch);
                }
                return;
            }
        Label_00B1:
            str = (reflect is ClassScope) ? ((ClassScope) reflect).GetName() : ((Type) reflect).Name;
            base.context.HandleError(JSError.NotIndexable, str);
        }

        internal override void ResolveCall(ASTList args, IReflect[] argIRs, bool constructor, bool brackets)
        {
            string str;
            if (constructor || !brackets)
            {
                if (this.isSuper)
                {
                    base.context.HandleError(JSError.IllegalUseOfSuper);
                    return;
                }
                base.context.HandleError(JSError.IllegalUseOfThis);
                return;
            }
            IReflect reflect = this.InferType(null);
            Type t = (reflect is Type) ? ((Type) reflect) : null;
            if (reflect is ClassScope)
            {
                t = ((ClassScope) reflect).GetBakedSuperType();
            }
            MemberInfo[] defaultMembers = JSBinder.GetDefaultMembers(t);
            if ((defaultMembers != null) && (defaultMembers.Length > 0))
            {
                try
                {
                    this.method = JSBinder.SelectMethod(defaultMembers, argIRs);
                    if (this.method == null)
                    {
                        goto Label_00C5;
                    }
                    if (!Binding.CheckParameters(this.method.GetParameters(), argIRs, args, base.context, 0, false, true))
                    {
                        this.method = null;
                    }
                }
                catch (AmbiguousMatchException)
                {
                    base.context.HandleError(JSError.AmbiguousMatch);
                }
                return;
            }
        Label_00C5:
            str = (reflect is ClassScope) ? ((ClassScope) reflect).GetName() : ((Type) reflect).Name;
            base.context.HandleError(JSError.NotIndexable, str);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (rtype != Typeob.Void)
            {
                if (this.InferType(null) is GlobalScope)
                {
                    base.EmitILToLoadEngine(il);
                    if (rtype == Typeob.LenientGlobalObject)
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.getLenientGlobalObjectMethod);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                        il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
                        il.Emit(OpCodes.Callvirt, CompilerGlobals.getDefaultThisObjectMethod);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    Microsoft.JScript.Convert.Emit(this, il, Microsoft.JScript.Convert.ToType(this.InferType(null)), rtype);
                }
            }
        }

        internal override void TranslateToILCall(ILGenerator il, Type rtype, ASTList argList, bool construct, bool brackets)
        {
            MethodInfo method = this.method;
            if (method != null)
            {
                Type reflectedType = method.ReflectedType;
                if (!method.IsStatic)
                {
                    this.method = null;
                    this.TranslateToIL(il, reflectedType);
                    this.method = method;
                }
                ParameterInfo[] parameters = method.GetParameters();
                Binding.PlaceArgumentsOnStack(il, parameters, argList, 0, 0, Binding.ReflectionMissingCW);
                if ((method.IsVirtual && !method.IsFinal) && (!reflectedType.IsSealed || !reflectedType.IsValueType))
                {
                    il.Emit(OpCodes.Callvirt, method);
                }
                else
                {
                    il.Emit(OpCodes.Call, method);
                }
                Microsoft.JScript.Convert.Emit(this, il, method.ReturnType, rtype);
            }
            else
            {
                base.TranslateToILCall(il, rtype, argList, construct, brackets);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }

        internal override void TranslateToILPreSet(ILGenerator il, ASTList argList)
        {
            MethodInfo method = this.method;
            if (method != null)
            {
                Type reflectedType = method.ReflectedType;
                if (!method.IsStatic)
                {
                    this.TranslateToIL(il, reflectedType);
                }
                Binding.PlaceArgumentsOnStack(il, method.GetParameters(), argList, 0, 1, Binding.ReflectionMissingCW);
            }
            else
            {
                base.TranslateToILPreSet(il, argList);
            }
        }

        internal override void TranslateToILSet(ILGenerator il, AST rhvalue)
        {
            MethodInfo method = this.method;
            if (method != null)
            {
                if (rhvalue != null)
                {
                    rhvalue.TranslateToIL(il, method.GetParameters()[0].ParameterType);
                }
                Type reflectedType = method.ReflectedType;
                if ((method.IsVirtual && !method.IsFinal) && (!reflectedType.IsSealed || !reflectedType.IsValueType))
                {
                    il.Emit(OpCodes.Callvirt, method);
                }
                else
                {
                    il.Emit(OpCodes.Call, method);
                }
                if (method.ReturnType != Typeob.Void)
                {
                    il.Emit(OpCodes.Pop);
                }
            }
            else
            {
                base.TranslateToILSet(il, rhvalue);
            }
        }
    }
}

