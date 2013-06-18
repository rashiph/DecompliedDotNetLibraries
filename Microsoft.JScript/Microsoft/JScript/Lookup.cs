namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Lookup : Binding
    {
        private int evalLexLevel;
        private LocalBuilder fieldLoc;
        private LateBinding lateBinding;
        private int lexLevel;
        private LocalBuilder refLoc;
        private bool thereIsAnObjectOnTheStack;

        internal Lookup(Context context) : base(context, context.GetCode())
        {
            this.lexLevel = 0;
            this.evalLexLevel = 0;
            this.fieldLoc = null;
            this.refLoc = null;
            this.lateBinding = null;
            this.thereIsAnObjectOnTheStack = false;
        }

        internal Lookup(string name, Context context) : this(context)
        {
            base.name = name;
        }

        private void BindName()
        {
            int num = 0;
            int num2 = 0;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            bool flag = false;
            bool flag2 = false;
            while (parent != null)
            {
                MemberInfo[] member = null;
                WithObject obj3 = parent as WithObject;
                if ((obj3 != null) && flag2)
                {
                    member = obj3.GetMember(base.name, bindingAttr, false);
                }
                else
                {
                    member = parent.GetMember(base.name, bindingAttr);
                }
                base.members = member;
                if (member.Length > 0)
                {
                    break;
                }
                if (parent is WithObject)
                {
                    base.isFullyResolved = base.isFullyResolved && ((WithObject) parent).isKnownAtCompileTime;
                    num++;
                }
                else if (parent is ActivationObject)
                {
                    base.isFullyResolved = base.isFullyResolved && ((ActivationObject) parent).isKnownAtCompileTime;
                    if ((parent is BlockScope) || ((parent is FunctionScope) && ((FunctionScope) parent).mustSaveStackLocals))
                    {
                        num++;
                    }
                    if (parent is ClassScope)
                    {
                        if (flag)
                        {
                            flag2 = true;
                        }
                        if (((ClassScope) parent).owner.isStatic)
                        {
                            bindingAttr &= ~BindingFlags.Instance;
                            flag = true;
                        }
                    }
                }
                else if (parent is StackFrame)
                {
                    num++;
                }
                num2++;
                parent = parent.GetParent();
            }
            if (base.members.Length > 0)
            {
                this.lexLevel = num;
                this.evalLexLevel = num2;
            }
        }

        internal bool CanPlaceAppropriateObjectOnStack(object ob)
        {
            if (ob is LenientGlobalObject)
            {
                return true;
            }
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            int lexLevel = this.lexLevel;
            while ((lexLevel > 0) && ((parent is WithObject) || (parent is BlockScope)))
            {
                if (parent is WithObject)
                {
                    lexLevel--;
                }
                parent = parent.GetParent();
            }
            return ((parent is WithObject) || (parent is GlobalScope));
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            FieldInfo member = base.member as FieldInfo;
            if (member != null)
            {
                if (!member.IsStatic)
                {
                    base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
                }
            }
            else
            {
                MethodInfo getMethod = base.member as MethodInfo;
                if (getMethod != null)
                {
                    if (!getMethod.IsStatic)
                    {
                        base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
                    }
                }
                else
                {
                    PropertyInfo prop = base.member as PropertyInfo;
                    if (prop != null)
                    {
                        getMethod = JSProperty.GetGetMethod(prop, true);
                        if ((getMethod != null) && !getMethod.IsStatic)
                        {
                            base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
                        }
                        else
                        {
                            getMethod = JSProperty.GetSetMethod(prop, true);
                            if ((getMethod != null) && !getMethod.IsStatic)
                            {
                                base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
                            }
                        }
                    }
                }
            }
        }

        internal override object Evaluate()
        {
            object memberValue = null;
            ScriptObject obj3 = base.Globals.ScopeStack.Peek();
            if (!base.isFullyResolved)
            {
                memberValue = ((IActivationObject) obj3).GetMemberValue(base.name, this.evalLexLevel);
                if (!(memberValue is Microsoft.JScript.Missing))
                {
                    return memberValue;
                }
            }
            if ((base.members == null) && !VsaEngine.executeForJSEE)
            {
                this.BindName();
                base.ResolveRHValue();
            }
            memberValue = base.Evaluate();
            if (memberValue is Microsoft.JScript.Missing)
            {
                throw new JScriptException(JSError.UndefinedIdentifier, base.context);
            }
            return memberValue;
        }

        internal override LateBinding EvaluateAsLateBinding()
        {
            if (!base.isFullyResolved)
            {
                this.BindName();
                base.isFullyResolved = false;
            }
            if (base.defaultMember == base.member)
            {
                base.defaultMember = null;
            }
            object obj2 = this.GetObject();
            LateBinding lateBinding = this.lateBinding;
            if (lateBinding == null)
            {
                lateBinding = this.lateBinding = new LateBinding(base.name, obj2, VsaEngine.executeForJSEE);
            }
            lateBinding.obj = obj2;
            lateBinding.last_object = obj2;
            lateBinding.last_members = base.members;
            lateBinding.last_member = base.member;
            if (!base.isFullyResolved)
            {
                base.members = null;
            }
            return lateBinding;
        }

        internal override WrappedNamespace EvaluateAsWrappedNamespace(bool giveErrorIfNameInUse)
        {
            Namespace namespace2 = Namespace.GetNamespace(base.name, base.Engine);
            GlobalScope globalScope = ((IActivationObject) base.Globals.ScopeStack.Peek()).GetGlobalScope();
            FieldInfo info = giveErrorIfNameInUse ? globalScope.GetLocalField(base.name) : globalScope.GetField(base.name, BindingFlags.Public | BindingFlags.Static);
            if (info != null)
            {
                if (giveErrorIfNameInUse && (!info.IsLiteral || !(info.GetValue(null) is Namespace)))
                {
                    base.context.HandleError(JSError.DuplicateName, true);
                }
            }
            else
            {
                info = globalScope.AddNewField(base.name, namespace2, FieldAttributes.Literal | FieldAttributes.Public);
                ((JSVariableField) info).type = new TypeExpression(new ConstantWrapper(Typeob.Namespace, base.context));
                ((JSVariableField) info).originalContext = base.context;
            }
            return new WrappedNamespace(base.name, base.Engine);
        }

        protected override object GetObject()
        {
            object closureInstance;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            if (base.member is JSMemberField)
            {
                while (parent != null)
                {
                    StackFrame frame = parent as StackFrame;
                    if (frame != null)
                    {
                        closureInstance = frame.closureInstance;
                        goto Label_0059;
                    }
                    parent = parent.GetParent();
                }
                return null;
            }
            for (int i = this.evalLexLevel; i > 0; i--)
            {
                parent = parent.GetParent();
            }
            closureInstance = parent;
        Label_0059:
            if (base.defaultMember != null)
            {
                MemberTypes memberType = base.defaultMember.MemberType;
                if (memberType <= MemberTypes.Method)
                {
                    switch (memberType)
                    {
                        case MemberTypes.Event:
                            return null;

                        case (MemberTypes.Event | MemberTypes.Constructor):
                            return closureInstance;

                        case MemberTypes.Field:
                            return ((FieldInfo) base.defaultMember).GetValue(closureInstance);

                        case MemberTypes.Method:
                            return ((MethodInfo) base.defaultMember).Invoke(closureInstance, new object[0]);
                    }
                    return closureInstance;
                }
                switch (memberType)
                {
                    case MemberTypes.Property:
                        return ((PropertyInfo) base.defaultMember).GetValue(closureInstance, null);

                    case MemberTypes.NestedType:
                        return base.member;
                }
            }
            return closureInstance;
        }

        protected override void HandleNoSuchMemberError()
        {
            if (base.isFullyResolved)
            {
                base.context.HandleError(JSError.UndeclaredVariable, base.Engine.doFast);
            }
        }

        internal override IReflect InferType(JSField inference_target)
        {
            if (!base.isFullyResolved)
            {
                return Typeob.Object;
            }
            return base.InferType(inference_target);
        }

        internal bool InFunctionNestedInsideInstanceMethod()
        {
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while ((parent is WithObject) || (parent is BlockScope))
            {
                parent = parent.GetParent();
            }
            for (FunctionScope scope = parent as FunctionScope; scope != null; scope = parent as FunctionScope)
            {
                if (scope.owner.isMethod)
                {
                    return !scope.owner.isStatic;
                }
                for (parent = scope.owner.enclosing_scope; (parent is WithObject) || (parent is BlockScope); parent = parent.GetParent())
                {
                }
            }
            return false;
        }

        internal bool InStaticCode()
        {
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while ((parent is WithObject) || (parent is BlockScope))
            {
                parent = parent.GetParent();
            }
            FunctionScope scope = parent as FunctionScope;
            if (scope != null)
            {
                return scope.isStatic;
            }
            StackFrame frame = parent as StackFrame;
            if (frame != null)
            {
                return (frame.thisObject is Type);
            }
            ClassScope scope2 = parent as ClassScope;
            if (scope2 != null)
            {
                return scope2.inStaticInitializerCode;
            }
            return true;
        }

        private bool IsBoundToMethodInfos()
        {
            if ((base.members == null) || (base.members.Length == 0))
            {
                return false;
            }
            for (int i = 0; i < base.members.Length; i++)
            {
                if (!(base.members[i] is MethodInfo))
                {
                    return false;
                }
            }
            return true;
        }

        internal override AST PartiallyEvaluate()
        {
            this.BindName();
            if ((base.members == null) || (base.members.Length == 0))
            {
                ScriptObject parent = base.Globals.ScopeStack.Peek();
                while (parent is FunctionScope)
                {
                    parent = parent.GetParent();
                }
                if (!(parent is WithObject) || base.isFullyResolved)
                {
                    base.context.HandleError(JSError.UndeclaredVariable, base.isFullyResolved && base.Engine.doFast);
                }
            }
            else
            {
                base.ResolveRHValue();
                MemberInfo member = base.member;
                if (member is FieldInfo)
                {
                    FieldInfo field = (FieldInfo) member;
                    if ((field is JSLocalField) && !((JSLocalField) field).isDefined)
                    {
                        ((JSLocalField) field).isUsedBeforeDefinition = true;
                        base.context.HandleError(JSError.VariableMightBeUnitialized);
                    }
                    if (!field.IsLiteral)
                    {
                        if ((field.IsInitOnly && field.IsStatic) && ((field.DeclaringType == Typeob.GlobalObject) && base.isFullyResolved))
                        {
                            return new ConstantWrapper(field.GetValue(null), base.context);
                        }
                    }
                    else
                    {
                        object obj3 = (field is JSVariableField) ? ((JSVariableField) field).value : TypeReferences.GetConstantValue(field);
                        if (obj3 is AST)
                        {
                            AST ast = ((AST) obj3).PartiallyEvaluate();
                            if ((ast is ConstantWrapper) && base.isFullyResolved)
                            {
                                return ast;
                            }
                            obj3 = null;
                        }
                        if (!(obj3 is FunctionObject) && base.isFullyResolved)
                        {
                            return new ConstantWrapper(obj3, base.context);
                        }
                    }
                }
                else if (member is PropertyInfo)
                {
                    PropertyInfo info3 = (PropertyInfo) member;
                    if ((!info3.CanWrite && !(info3 is JSProperty)) && ((info3.DeclaringType == Typeob.GlobalObject) && base.isFullyResolved))
                    {
                        return new ConstantWrapper(info3.GetValue(null, null), base.context);
                    }
                }
                if ((member is Type) && base.isFullyResolved)
                {
                    return new ConstantWrapper(member, base.context);
                }
            }
            return this;
        }

        internal override AST PartiallyEvaluateAsCallable()
        {
            this.BindName();
            return this;
        }

        internal override AST PartiallyEvaluateAsReference()
        {
            this.BindName();
            if ((base.members == null) || (base.members.Length == 0))
            {
                if (!(base.Globals.ScopeStack.Peek() is WithObject) || base.isFullyResolved)
                {
                    base.context.HandleError(JSError.UndeclaredVariable, base.isFullyResolved && base.Engine.doFast);
                }
            }
            else
            {
                base.ResolveLHValue();
            }
            return this;
        }

        internal override object ResolveCustomAttribute(ASTList args, IReflect[] argIRs, AST target)
        {
            if (base.name == "expando")
            {
                base.members = Typeob.Expando.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            }
            else if (base.name == "override")
            {
                base.members = Typeob.Override.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            }
            else if (base.name == "hide")
            {
                base.members = Typeob.Hide.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            }
            else if (base.name == "...")
            {
                base.members = Typeob.ParamArrayAttribute.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                base.name = base.name + "Attribute";
                this.BindName();
                if ((base.members == null) || (base.members.Length == 0))
                {
                    base.name = base.name.Substring(0, base.name.Length - 9);
                    this.BindName();
                }
            }
            return base.ResolveCustomAttribute(args, argIRs, target);
        }

        internal override void SetPartialValue(AST partial_value)
        {
            if ((base.members != null) && (base.members.Length != 0))
            {
                if (base.member is JSLocalField)
                {
                    JSLocalField member = (JSLocalField) base.member;
                    if (member.type == null)
                    {
                        IReflect ir = partial_value.InferType(member);
                        if ((ir == Typeob.String) && (partial_value is Plus))
                        {
                            member.SetInferredType(Typeob.Object, partial_value);
                            return;
                        }
                        member.SetInferredType(ir, partial_value);
                        return;
                    }
                    member.isDefined = true;
                }
                Binding.AssignmentCompatible(this.InferType(null), partial_value, partial_value.InferType(null), base.isFullyResolved);
            }
        }

        internal override void SetValue(object value)
        {
            if (!base.isFullyResolved)
            {
                this.EvaluateAsLateBinding().SetValue(value);
            }
            else
            {
                base.SetValue(value);
            }
        }

        internal void SetWithValue(WithObject scope, object value)
        {
            FieldInfo field = scope.GetField(base.name, this.lexLevel);
            if (field != null)
            {
                field.SetValue(scope, value);
            }
        }

        public override string ToString()
        {
            return base.name;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (base.isFullyResolved)
            {
                base.TranslateToIL(il, rtype);
            }
            else
            {
                Label label = il.DefineLabel();
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
                il.Emit(OpCodes.Ldstr, base.name);
                ConstantWrapper.TranslateToILInt(il, this.lexLevel);
                il.Emit(OpCodes.Callvirt, CompilerGlobals.getMemberValueMethod);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Call, CompilerGlobals.isMissingMethod);
                il.Emit(OpCodes.Brfalse, label);
                il.Emit(OpCodes.Pop);
                base.TranslateToIL(il, Typeob.Object);
                il.MarkLabel(label);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
            }
        }

        internal override void TranslateToILCall(ILGenerator il, Type rtype, ASTList argList, bool construct, bool brackets)
        {
            if (base.isFullyResolved)
            {
                base.TranslateToILCall(il, rtype, argList, construct, brackets);
            }
            else
            {
                Label label = il.DefineLabel();
                Label label2 = il.DefineLabel();
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
                il.Emit(OpCodes.Ldstr, base.name);
                ConstantWrapper.TranslateToILInt(il, this.lexLevel);
                il.Emit(OpCodes.Callvirt, CompilerGlobals.getMemberValueMethod);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Call, CompilerGlobals.isMissingMethod);
                il.Emit(OpCodes.Brfalse, label);
                il.Emit(OpCodes.Pop);
                base.TranslateToILCall(il, Typeob.Object, argList, construct, brackets);
                il.Emit(OpCodes.Br, label2);
                il.MarkLabel(label);
                this.TranslateToILDefaultThisObject(il);
                argList.TranslateToIL(il, Typeob.ArrayOfObject);
                if (construct)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                if (brackets)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.callValue2Method);
                il.MarkLabel(label2);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
            }
        }

        internal void TranslateToILDefaultThisObject(ILGenerator il)
        {
            this.TranslateToILDefaultThisObject(il, 0);
        }

        private void TranslateToILDefaultThisObject(ILGenerator il, int lexLevel)
        {
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
            while (lexLevel-- > 0)
            {
                il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
            }
            il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
            il.Emit(OpCodes.Callvirt, CompilerGlobals.getDefaultThisObjectMethod);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (base.defaultMember == null)
            {
                if (base.member != null)
                {
                    switch (base.member.MemberType)
                    {
                        case MemberTypes.Constructor:
                        case MemberTypes.Method:
                            return;

                        case MemberTypes.Field:
                            if (base.member is JSExpandoField)
                            {
                                base.member = null;
                            }
                            else
                            {
                                return;
                            }
                            break;

                        case MemberTypes.Property:
                        case MemberTypes.TypeInfo:
                        case MemberTypes.NestedType:
                            return;
                    }
                }
                this.refLoc = il.DeclareLocal(Typeob.LateBinding);
                il.Emit(OpCodes.Ldstr, base.name);
                if ((base.isFullyResolved && (base.member == null)) && this.IsBoundToMethodInfos())
                {
                    MethodInfo mem = base.members[0] as MethodInfo;
                    if (mem.IsStatic)
                    {
                        il.Emit(OpCodes.Ldtoken, mem.DeclaringType);
                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                    }
                    else
                    {
                        this.TranslateToILObjectForMember(il, mem.DeclaringType, false, mem);
                    }
                }
                else
                {
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                }
                il.Emit(OpCodes.Newobj, CompilerGlobals.lateBindingConstructor2);
                il.Emit(OpCodes.Stloc, this.refLoc);
            }
        }

        protected override void TranslateToILObject(ILGenerator il, Type obType, bool noValue)
        {
            this.TranslateToILObjectForMember(il, obType, noValue, base.member);
        }

        private void TranslateToILObjectForMember(ILGenerator il, Type obType, bool noValue, MemberInfo mem)
        {
            this.thereIsAnObjectOnTheStack = true;
            if (mem is IWrappedMember)
            {
                object wrappedObject = ((IWrappedMember) mem).GetWrappedObject();
                if (wrappedObject is LenientGlobalObject)
                {
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Call, CompilerGlobals.getLenientGlobalObjectMethod);
                }
                else if ((wrappedObject is Type) || (wrappedObject is ClassScope))
                {
                    if (obType.IsAssignableFrom(Typeob.Type))
                    {
                        new ConstantWrapper(wrappedObject, null).TranslateToIL(il, Typeob.Type);
                    }
                    else
                    {
                        ScriptObject parent = base.Globals.ScopeStack.Peek();
                        while ((parent is WithObject) || (parent is BlockScope))
                        {
                            parent = parent.GetParent();
                        }
                        if (parent is FunctionScope)
                        {
                            if (((FunctionScope) parent).owner.isMethod)
                            {
                                il.Emit(OpCodes.Ldarg_0);
                            }
                            else
                            {
                                base.EmitILToLoadEngine(il);
                                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                                parent = base.Globals.ScopeStack.Peek();
                                while ((parent is WithObject) || (parent is BlockScope))
                                {
                                    il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
                                    parent = parent.GetParent();
                                }
                                il.Emit(OpCodes.Castclass, Typeob.StackFrame);
                                il.Emit(OpCodes.Ldfld, CompilerGlobals.closureInstanceField);
                            }
                        }
                        else if (parent is ClassScope)
                        {
                            il.Emit(OpCodes.Ldarg_0);
                        }
                        for (parent = base.Globals.ScopeStack.Peek(); parent != null; parent = parent.GetParent())
                        {
                            ClassScope scope = parent as ClassScope;
                            if (scope != null)
                            {
                                if (scope.IsSameOrDerivedFrom(obType))
                                {
                                    return;
                                }
                                il.Emit(OpCodes.Ldfld, scope.outerClassField);
                            }
                        }
                    }
                }
                else
                {
                    this.TranslateToILDefaultThisObject(il, this.lexLevel);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, obType);
                }
            }
            else
            {
                ScriptObject obj5 = base.Globals.ScopeStack.Peek();
                while ((obj5 is WithObject) || (obj5 is BlockScope))
                {
                    obj5 = obj5.GetParent();
                }
                if (!(obj5 is FunctionScope) || ((FunctionScope) obj5).owner.isMethod)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    while (obj5 != null)
                    {
                        if (obj5 is ClassScope)
                        {
                            ClassScope scope3 = (ClassScope) obj5;
                            if (scope3.IsSameOrDerivedFrom(obType))
                            {
                                return;
                            }
                            il.Emit(OpCodes.Ldfld, scope3.outerClassField);
                        }
                        obj5 = obj5.GetParent();
                    }
                }
                else
                {
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                    obj5 = base.Globals.ScopeStack.Peek();
                    while ((obj5 is WithObject) || (obj5 is BlockScope))
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
                        obj5 = obj5.GetParent();
                    }
                    il.Emit(OpCodes.Castclass, Typeob.StackFrame);
                    il.Emit(OpCodes.Ldfld, CompilerGlobals.closureInstanceField);
                    while (obj5 != null)
                    {
                        if (obj5 is ClassScope)
                        {
                            ClassScope scope2 = (ClassScope) obj5;
                            if (scope2.IsSameOrDerivedFrom(obType))
                            {
                                break;
                            }
                            il.Emit(OpCodes.Castclass, scope2.GetTypeBuilder());
                            il.Emit(OpCodes.Ldfld, scope2.outerClassField);
                        }
                        obj5 = obj5.GetParent();
                    }
                    il.Emit(OpCodes.Castclass, obType);
                }
            }
        }

        internal override void TranslateToILPreSet(ILGenerator il)
        {
            this.TranslateToILPreSet(il, false);
        }

        internal void TranslateToILPreSet(ILGenerator il, bool doBoth)
        {
            if (base.isFullyResolved)
            {
                base.TranslateToILPreSet(il);
            }
            else
            {
                Label label = il.DefineLabel();
                LocalBuilder local = this.fieldLoc = il.DeclareLocal(Typeob.FieldInfo);
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
                il.Emit(OpCodes.Ldstr, base.name);
                ConstantWrapper.TranslateToILInt(il, this.lexLevel);
                il.Emit(OpCodes.Callvirt, CompilerGlobals.getFieldMethod);
                il.Emit(OpCodes.Stloc, local);
                if (!doBoth)
                {
                    il.Emit(OpCodes.Ldloc, local);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Bne_Un_S, label);
                }
                base.TranslateToILPreSet(il);
                if (this.thereIsAnObjectOnTheStack)
                {
                    Label label2 = il.DefineLabel();
                    il.Emit(OpCodes.Br_S, label2);
                    il.MarkLabel(label);
                    il.Emit(OpCodes.Ldnull);
                    il.MarkLabel(label2);
                }
                else
                {
                    il.MarkLabel(label);
                }
            }
        }

        internal override void TranslateToILPreSetPlusGet(ILGenerator il)
        {
            if (base.isFullyResolved)
            {
                base.TranslateToILPreSetPlusGet(il);
            }
            else
            {
                Label label = il.DefineLabel();
                Label label2 = il.DefineLabel();
                LocalBuilder local = this.fieldLoc = il.DeclareLocal(Typeob.FieldInfo);
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
                il.Emit(OpCodes.Ldstr, base.name);
                ConstantWrapper.TranslateToILInt(il, this.lexLevel);
                il.Emit(OpCodes.Callvirt, CompilerGlobals.getFieldMethod);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Bne_Un_S, label2);
                base.TranslateToILPreSetPlusGet(il);
                il.Emit(OpCodes.Br_S, label);
                il.MarkLabel(label2);
                if (this.thereIsAnObjectOnTheStack)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                il.Emit(OpCodes.Ldloc, this.fieldLoc);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Callvirt, CompilerGlobals.getFieldValueMethod);
                il.MarkLabel(label);
            }
        }

        internal override void TranslateToILSet(ILGenerator il, AST rhvalue)
        {
            this.TranslateToILSet(il, false, rhvalue);
        }

        internal void TranslateToILSet(ILGenerator il, bool doBoth, AST rhvalue)
        {
            if (base.isFullyResolved)
            {
                base.TranslateToILSet(il, rhvalue);
            }
            else
            {
                if (rhvalue != null)
                {
                    rhvalue.TranslateToIL(il, Typeob.Object);
                }
                if (this.fieldLoc == null)
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.setIndexedPropertyValueStaticMethod);
                }
                else
                {
                    LocalBuilder local = il.DeclareLocal(Typeob.Object);
                    if (doBoth)
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Stloc, local);
                        base.isFullyResolved = true;
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, Microsoft.JScript.Convert.ToType(this.InferType(null)));
                        base.TranslateToILSet(il, null);
                    }
                    Label label = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, this.fieldLoc);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Beq_S, label);
                    Label label2 = il.DefineLabel();
                    if (!doBoth)
                    {
                        il.Emit(OpCodes.Stloc, local);
                        if (this.thereIsAnObjectOnTheStack)
                        {
                            il.Emit(OpCodes.Pop);
                        }
                    }
                    il.Emit(OpCodes.Ldloc, this.fieldLoc);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Ldloc, local);
                    il.Emit(OpCodes.Callvirt, CompilerGlobals.setFieldValueMethod);
                    il.Emit(OpCodes.Br_S, label2);
                    il.MarkLabel(label);
                    if (!doBoth)
                    {
                        base.isFullyResolved = true;
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, Microsoft.JScript.Convert.ToType(this.InferType(null)));
                        base.TranslateToILSet(il, null);
                    }
                    il.MarkLabel(label2);
                }
            }
        }

        protected override void TranslateToILWithDupOfThisOb(ILGenerator il)
        {
            this.TranslateToILDefaultThisObject(il);
            this.TranslateToIL(il, Typeob.Object);
        }

        internal void TranslateToLateBinding(ILGenerator il)
        {
            this.thereIsAnObjectOnTheStack = true;
            il.Emit(OpCodes.Ldloc, this.refLoc);
        }

        internal string Name
        {
            get
            {
                return base.name;
            }
        }
    }
}

