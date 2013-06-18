namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Member : Binding
    {
        private bool fast;
        private bool isImplicitWrapper;
        private LateBinding lateBinding;
        private Context memberNameContext;
        private LocalBuilder refLoc;
        internal AST rootObject;
        private IReflect rootObjectInferredType;
        private LocalBuilder temp;

        internal Member(Context context, AST rootObject, AST memberName) : base(context, memberName.context.GetCode())
        {
            this.fast = base.Engine.doFast;
            this.isImplicitWrapper = false;
            base.isNonVirtual = (rootObject is ThisLiteral) && ((ThisLiteral) rootObject).isSuper;
            this.lateBinding = null;
            this.memberNameContext = memberName.context;
            this.rootObject = rootObject;
            this.rootObjectInferredType = null;
            this.refLoc = null;
            this.temp = null;
        }

        private void BindName(JSField inferenceTarget)
        {
            MemberInfo[] mems = null;
            this.rootObject = this.rootObject.PartiallyEvaluate();
            IReflect obType = this.rootObjectInferredType = this.rootObject.InferType(inferenceTarget);
            if (this.rootObject is ConstantWrapper)
            {
                object obj2 = Microsoft.JScript.Convert.ToObject2(this.rootObject.Evaluate(), base.Engine);
                if (obj2 == null)
                {
                    this.rootObject.context.HandleError(JSError.ObjectExpected);
                    return;
                }
                ClassScope scope = obj2 as ClassScope;
                Type type = obj2 as Type;
                if ((scope != null) || (type != null))
                {
                    if (scope != null)
                    {
                        base.members = mems = scope.GetMember(base.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    }
                    else
                    {
                        base.members = mems = type.GetMember(base.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    }
                    if (mems.Length <= 0)
                    {
                        base.members = mems = Typeob.Type.GetMember(base.name, BindingFlags.Public | BindingFlags.Instance);
                    }
                    return;
                }
                Namespace namespace2 = obj2 as Namespace;
                if (namespace2 != null)
                {
                    string className = namespace2.Name + "." + base.name;
                    scope = base.Engine.GetClass(className);
                    if (scope != null)
                    {
                        FieldAttributes literal = FieldAttributes.Literal;
                        if ((scope.owner.attributes & TypeAttributes.Public) == TypeAttributes.AnsiClass)
                        {
                            literal |= FieldAttributes.Private;
                        }
                        base.members = new MemberInfo[] { new JSGlobalField(null, base.name, scope, literal) };
                        return;
                    }
                    type = base.Engine.GetType(className);
                    if (type != null)
                    {
                        base.members = new MemberInfo[] { type };
                        return;
                    }
                }
                else if ((obj2 is MathObject) || ((obj2 is ScriptFunction) && !(obj2 is FunctionObject)))
                {
                    obType = (IReflect) obj2;
                }
            }
            obType = this.ProvideWrapperForPrototypeProperties(obType);
            if ((obType == Typeob.Object) && !base.isNonVirtual)
            {
                base.members = new MemberInfo[0];
            }
            else
            {
                Type t = obType as Type;
                if ((t != null) && t.IsInterface)
                {
                    base.members = JSBinder.GetInterfaceMembers(base.name, t);
                }
                else
                {
                    ClassScope scope2 = obType as ClassScope;
                    if ((scope2 != null) && scope2.owner.isInterface)
                    {
                        base.members = scope2.owner.GetInterfaceMember(base.name);
                    }
                    else
                    {
                        while (obType != null)
                        {
                            scope2 = obType as ClassScope;
                            if (scope2 != null)
                            {
                                mems = base.members = obType.GetMember(base.name, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                                if (mems.Length > 0)
                                {
                                    return;
                                }
                                obType = scope2.GetSuperType();
                            }
                            else
                            {
                                t = obType as Type;
                                if (t == null)
                                {
                                    base.members = obType.GetMember(base.name, BindingFlags.Public | BindingFlags.Instance);
                                    return;
                                }
                                mems = base.members = t.GetMember(base.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                                if (mems.Length > 0)
                                {
                                    if (LateBinding.SelectMember(mems) == null)
                                    {
                                        mems = base.members = t.GetMember(base.name, MemberTypes.Method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                        if (mems.Length == 0)
                                        {
                                            base.members = t.GetMember(base.name, MemberTypes.Property, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                        }
                                    }
                                    return;
                                }
                                obType = t.BaseType;
                            }
                        }
                    }
                }
            }
        }

        internal override object Evaluate()
        {
            object obj2 = base.Evaluate();
            if (obj2 is Microsoft.JScript.Missing)
            {
                obj2 = null;
            }
            return obj2;
        }

        internal override LateBinding EvaluateAsLateBinding()
        {
            LateBinding lateBinding = this.lateBinding;
            if (lateBinding == null)
            {
                if ((base.member != null) && !this.rootObjectInferredType.Equals(this.rootObject.InferType(null)))
                {
                    base.InvalidateBinding();
                }
                this.lateBinding = lateBinding = new LateBinding(base.name, null, VsaEngine.executeForJSEE);
                lateBinding.last_member = base.member;
            }
            object obj2 = this.rootObject.Evaluate();
            try
            {
                lateBinding.obj = obj2 = Microsoft.JScript.Convert.ToObject(obj2, base.Engine);
                if ((base.defaultMember == null) && (base.member != null))
                {
                    lateBinding.last_object = obj2;
                }
            }
            catch (JScriptException exception)
            {
                if (exception.context == null)
                {
                    exception.context = this.rootObject.context;
                }
                throw exception;
            }
            return lateBinding;
        }

        internal object EvaluateAsType()
        {
            object memberValue = this.rootObject.EvaluateAsWrappedNamespace(false).GetMemberValue(base.name);
            if ((memberValue != null) && !(memberValue is Microsoft.JScript.Missing))
            {
                return memberValue;
            }
            object obj3 = null;
            Member rootObject = this.rootObject as Member;
            if (rootObject == null)
            {
                Lookup lookup = this.rootObject as Lookup;
                if (lookup == null)
                {
                    return null;
                }
                ConstantWrapper wrapper = lookup.PartiallyEvaluate() as ConstantWrapper;
                if (wrapper == null)
                {
                    JSGlobalField member = lookup.member as JSGlobalField;
                    if ((member == null) || !member.IsLiteral)
                    {
                        return null;
                    }
                    obj3 = member.value;
                }
                else
                {
                    obj3 = wrapper.value;
                }
            }
            else
            {
                obj3 = rootObject.EvaluateAsType();
            }
            ClassScope scope = obj3 as ClassScope;
            if (scope != null)
            {
                MemberInfo[] infoArray = scope.GetMember(base.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (infoArray.Length != 0)
                {
                    JSMemberField field2 = infoArray[0] as JSMemberField;
                    if (((field2 != null) && field2.IsLiteral) && ((field2.value is ClassScope) && (field2.IsPublic || field2.IsAccessibleFrom(base.Engine.ScriptObjectStackTop()))))
                    {
                        return field2.value;
                    }
                }
                return null;
            }
            Type type = obj3 as Type;
            if (type != null)
            {
                return type.GetNestedType(base.name);
            }
            return null;
        }

        internal override WrappedNamespace EvaluateAsWrappedNamespace(bool giveErrorIfNameInUse)
        {
            WrappedNamespace namespace2 = this.rootObject.EvaluateAsWrappedNamespace(giveErrorIfNameInUse);
            string name = base.name;
            namespace2.AddFieldOrUseExistingField(name, Namespace.GetNamespace(namespace2.ToString() + "." + name, base.Engine), FieldAttributes.Literal);
            return new WrappedNamespace(namespace2.ToString() + "." + name, base.Engine);
        }

        protected override object GetObject()
        {
            return Microsoft.JScript.Convert.ToObject(this.rootObject.Evaluate(), base.Engine);
        }

        protected override void HandleNoSuchMemberError()
        {
            IReflect ir = this.rootObject.InferType(null);
            object obj2 = null;
            if (this.rootObject is ConstantWrapper)
            {
                obj2 = this.rootObject.Evaluate();
            }
            if ((((ir != Typeob.Object) || base.isNonVirtual) && (!(ir is JSObject) || ((JSObject) ir).noExpando)) && (!(ir is GlobalScope) || ((GlobalScope) ir).isKnownAtCompileTime))
            {
                if (ir is Type)
                {
                    Type c = (Type) ir;
                    if (Typeob.ScriptFunction.IsAssignableFrom(c) || (c == Typeob.MathObject))
                    {
                        this.memberNameContext.HandleError(JSError.OLENoPropOrMethod);
                        return;
                    }
                    if (Typeob.IExpando.IsAssignableFrom(c))
                    {
                        return;
                    }
                    if (!this.fast && (((c == Typeob.Boolean) || (c == Typeob.String)) || Microsoft.JScript.Convert.IsPrimitiveNumericType(c)))
                    {
                        return;
                    }
                    if ((obj2 is ClassScope) && (((ClassScope) obj2).GetMember(base.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Length > 0))
                    {
                        this.memberNameContext.HandleError(JSError.NonStaticWithTypeName);
                        return;
                    }
                }
                if (obj2 is FunctionObject)
                {
                    this.rootObject = new ConstantWrapper(((FunctionObject) obj2).name, this.rootObject.context);
                    this.memberNameContext.HandleError(JSError.OLENoPropOrMethod);
                }
                else if ((ir is ClassScope) && (((ClassScope) ir).GetMember(base.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Length > 0))
                {
                    this.memberNameContext.HandleError(JSError.StaticRequiresTypeName);
                }
                else if (obj2 is Type)
                {
                    this.memberNameContext.HandleError(JSError.NoSuchStaticMember, Microsoft.JScript.Convert.ToTypeName((Type) obj2));
                }
                else if (obj2 is ClassScope)
                {
                    this.memberNameContext.HandleError(JSError.NoSuchStaticMember, Microsoft.JScript.Convert.ToTypeName((ClassScope) obj2));
                }
                else if (obj2 is Namespace)
                {
                    this.memberNameContext.HandleError(JSError.NoSuchType, ((Namespace) obj2).Name + "." + base.name);
                }
                else if (((ir != FunctionPrototype.ob) || !(this.rootObject is Binding)) || (!(((Binding) this.rootObject).member is JSVariableField) || !(((JSVariableField) ((Binding) this.rootObject).member).value is FunctionObject)))
                {
                    this.memberNameContext.HandleError(JSError.NoSuchMember, Microsoft.JScript.Convert.ToTypeName(ir));
                }
            }
        }

        internal override IReflect InferType(JSField inference_target)
        {
            if (base.members == null)
            {
                this.BindName(inference_target);
            }
            else if (!this.rootObjectInferredType.Equals(this.rootObject.InferType(inference_target)))
            {
                base.InvalidateBinding();
            }
            return base.InferType(null);
        }

        internal override IReflect InferTypeOfCall(JSField inference_target, bool isConstructor)
        {
            if (!this.rootObjectInferredType.Equals(this.rootObject.InferType(inference_target)))
            {
                base.InvalidateBinding();
            }
            return base.InferTypeOfCall(null, isConstructor);
        }

        internal override AST PartiallyEvaluate()
        {
            this.BindName(null);
            if ((base.members == null) || (base.members.Length == 0))
            {
                if (this.rootObject is ConstantWrapper)
                {
                    object obj2 = this.rootObject.Evaluate();
                    if (obj2 is Namespace)
                    {
                        return new ConstantWrapper(Namespace.GetNamespace(((Namespace) obj2).Name + "." + base.name, base.Engine), base.context);
                    }
                }
                this.HandleNoSuchMemberError();
                return this;
            }
            base.ResolveRHValue();
            if ((base.member is FieldInfo) && ((FieldInfo) base.member).IsLiteral)
            {
                object obj3 = (base.member is JSVariableField) ? ((JSVariableField) base.member).value : TypeReferences.GetConstantValue((FieldInfo) base.member);
                if (obj3 is AST)
                {
                    AST ast = ((AST) obj3).PartiallyEvaluate();
                    if (ast is ConstantWrapper)
                    {
                        return ast;
                    }
                    obj3 = null;
                }
                if (!(obj3 is FunctionObject) && (!(obj3 is ClassScope) || ((ClassScope) obj3).owner.IsStatic))
                {
                    return new ConstantWrapper(obj3, base.context);
                }
            }
            else if (base.member is Type)
            {
                return new ConstantWrapper(base.member, base.context);
            }
            return this;
        }

        internal override AST PartiallyEvaluateAsCallable()
        {
            this.BindName(null);
            return this;
        }

        internal override AST PartiallyEvaluateAsReference()
        {
            this.BindName(null);
            if ((base.members == null) || (base.members.Length == 0))
            {
                if (this.isImplicitWrapper && !Microsoft.JScript.Convert.IsArray(this.rootObjectInferredType))
                {
                    base.context.HandleError(JSError.UselessAssignment);
                }
                else
                {
                    this.HandleNoSuchMemberError();
                }
                return this;
            }
            base.ResolveLHValue();
            if (this.isImplicitWrapper && ((base.member == null) || (!(base.member is JSField) && Typeob.JSObject.IsAssignableFrom(base.member.DeclaringType))))
            {
                base.context.HandleError(JSError.UselessAssignment);
            }
            return this;
        }

        private IReflect ProvideWrapperForPrototypeProperties(IReflect obType)
        {
            if (obType == Typeob.String)
            {
                obType = base.Globals.globalObject.originalString.Construct();
                ((JSObject) obType).noExpando = this.fast;
                this.isImplicitWrapper = true;
                return obType;
            }
            if (((obType is Type) && Typeob.Array.IsAssignableFrom((Type) obType)) || (obType is TypedArray))
            {
                obType = base.Globals.globalObject.originalArray.ConstructWrapper();
                ((JSObject) obType).noExpando = this.fast;
                this.isImplicitWrapper = true;
                return obType;
            }
            if (obType == Typeob.Boolean)
            {
                obType = base.Globals.globalObject.originalBoolean.Construct();
                ((JSObject) obType).noExpando = this.fast;
                this.isImplicitWrapper = true;
                return obType;
            }
            if (Microsoft.JScript.Convert.IsPrimitiveNumericType(obType))
            {
                Type type = (Type) obType;
                obType = base.Globals.globalObject.originalNumber.Construct();
                ((JSObject) obType).noExpando = this.fast;
                ((NumberObject) obType).baseType = type;
                this.isImplicitWrapper = true;
                return obType;
            }
            if (obType is Type)
            {
                obType = Microsoft.JScript.Convert.ToIReflect((Type) obType, base.Engine);
            }
            return obType;
        }

        internal override object ResolveCustomAttribute(ASTList args, IReflect[] argIRs, AST target)
        {
            base.name = base.name + "Attribute";
            this.BindName(null);
            if ((base.members == null) || (base.members.Length == 0))
            {
                base.name = base.name.Substring(0, base.name.Length - 9);
                this.BindName(null);
            }
            return base.ResolveCustomAttribute(args, argIRs, target);
        }

        public override string ToString()
        {
            return (this.rootObject.ToString() + "." + base.name);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.rootObject.TranslateToILInitializer(il);
            if (!this.rootObjectInferredType.Equals(this.rootObject.InferType(null)))
            {
                base.InvalidateBinding();
            }
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
                il.Emit(OpCodes.Newobj, CompilerGlobals.lateBindingConstructor);
                il.Emit(OpCodes.Stloc, this.refLoc);
            }
        }

        protected override void TranslateToILObject(ILGenerator il, Type obType, bool noValue)
        {
            if ((noValue && obType.IsValueType) && (obType != Typeob.Enum))
            {
                if (this.temp == null)
                {
                    this.rootObject.TranslateToILReference(il, obType);
                }
                else
                {
                    Type type = Microsoft.JScript.Convert.ToType(this.rootObject.InferType(null));
                    if (type == obType)
                    {
                        il.Emit(OpCodes.Ldloca, this.temp);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, this.temp);
                        Microsoft.JScript.Convert.Emit(this, il, type, obType);
                        Microsoft.JScript.Convert.EmitLdloca(il, obType);
                    }
                }
            }
            else if ((this.temp == null) || (this.rootObject is ThisLiteral))
            {
                this.rootObject.TranslateToIL(il, obType);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, this.temp);
                Type type2 = Microsoft.JScript.Convert.ToType(this.rootObject.InferType(null));
                Microsoft.JScript.Convert.Emit(this, il, type2, obType);
            }
        }

        protected override void TranslateToILWithDupOfThisOb(ILGenerator il)
        {
            IReflect ir = this.rootObject.InferType(null);
            Type rtype = Microsoft.JScript.Convert.ToType(ir);
            this.rootObject.TranslateToIL(il, rtype);
            if ((((ir == Typeob.Object) || (ir == Typeob.String)) || (ir is TypedArray)) || (((ir is Type) && (((Type) ir) == rtype)) && Typeob.Array.IsAssignableFrom(rtype)))
            {
                rtype = Typeob.Object;
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.toObjectMethod);
            }
            il.Emit(OpCodes.Dup);
            this.temp = il.DeclareLocal(rtype);
            il.Emit(OpCodes.Stloc, this.temp);
            Microsoft.JScript.Convert.Emit(this, il, rtype, Typeob.Object);
            this.TranslateToIL(il, Typeob.Object);
        }

        internal void TranslateToLateBinding(ILGenerator il, bool speculativeEarlyBindingsExist)
        {
            if (speculativeEarlyBindingsExist)
            {
                LocalBuilder local = il.DeclareLocal(Typeob.Object);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, this.refLoc);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldloc, local);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, this.refLoc);
                il.Emit(OpCodes.Dup);
                this.TranslateToILObject(il, Typeob.Object, false);
            }
            IReflect reflect = this.rootObject.InferType(null);
            if (((((reflect == Typeob.Object) || (reflect == Typeob.String)) || (reflect is TypedArray)) || ((reflect is Type) && ((Type) reflect).IsPrimitive)) || ((reflect is Type) && Typeob.Array.IsAssignableFrom((Type) reflect)))
            {
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.toObjectMethod);
            }
            il.Emit(OpCodes.Stfld, CompilerGlobals.objectField);
        }
    }
}

