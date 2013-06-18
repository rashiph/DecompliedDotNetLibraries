namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    public abstract class Binding : AST
    {
        private IReflect[] argIRs;
        protected MemberInfo defaultMember;
        private IReflect defaultMemberReturnIR;
        private bool giveErrors;
        private bool isArrayConstructor;
        private bool isArrayElementAccess;
        protected bool isAssignmentToDefaultIndexedProperty;
        protected bool isFullyResolved;
        protected bool isNonVirtual;
        private static ConstantWrapper JScriptMissingCW = new ConstantWrapper(Microsoft.JScript.Missing.Value, null);
        internal MemberInfo member;
        internal MemberInfo[] members;
        protected string name;
        internal static ConstantWrapper ReflectionMissingCW = new ConstantWrapper(System.Reflection.Missing.Value, null);

        internal Binding(Context context, string name) : base(context)
        {
            this.argIRs = null;
            this.defaultMember = null;
            this.defaultMemberReturnIR = null;
            this.isArrayElementAccess = false;
            this.isArrayConstructor = false;
            this.isAssignmentToDefaultIndexedProperty = false;
            this.isFullyResolved = true;
            this.isNonVirtual = false;
            this.members = null;
            this.member = null;
            this.name = name;
            this.giveErrors = true;
        }

        private bool Accessible(bool checkSetter)
        {
            if (this.member != null)
            {
                switch (this.member.MemberType)
                {
                    case MemberTypes.Property:
                        return this.AccessibleProperty(checkSetter);

                    case MemberTypes.TypeInfo:
                        if (((Type) this.member).IsPublic)
                        {
                            if (checkSetter)
                            {
                                return false;
                            }
                            return true;
                        }
                        if (this.giveErrors)
                        {
                            base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                        }
                        return false;

                    case MemberTypes.NestedType:
                        if (((Type) this.member).IsNestedPublic)
                        {
                            if (checkSetter)
                            {
                                return false;
                            }
                            return true;
                        }
                        if (this.giveErrors)
                        {
                            base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                        }
                        return false;

                    case MemberTypes.Constructor:
                        return this.AccessibleConstructor();

                    case MemberTypes.Event:
                        return false;

                    case MemberTypes.Field:
                        return this.AccessibleField(checkSetter);

                    case MemberTypes.Method:
                        return this.AccessibleMethod();
                }
            }
            return false;
        }

        private bool AccessibleConstructor()
        {
            ConstructorInfo member = (ConstructorInfo) this.member;
            if (((member is JSConstructor) && ((JSConstructor) this.member).GetClassScope().owner.isAbstract) || (!(member is JSConstructor) && member.DeclaringType.IsAbstract))
            {
                base.context.HandleError(JSError.CannotInstantiateAbstractClass);
                return false;
            }
            if (member.IsPublic)
            {
                return true;
            }
            if ((member is JSConstructor) && ((JSConstructor) member).IsAccessibleFrom(base.Globals.ScopeStack.Peek()))
            {
                return true;
            }
            if (this.giveErrors)
            {
                base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
            }
            return false;
        }

        private bool AccessibleField(bool checkWritable)
        {
            FieldInfo member = (FieldInfo) this.member;
            if (!checkWritable || (!member.IsInitOnly && !member.IsLiteral))
            {
                if (!member.IsPublic)
                {
                    JSWrappedField field = member as JSWrappedField;
                    if (field != null)
                    {
                        this.member = member = field.wrappedField;
                    }
                    JSClosureField field2 = member as JSClosureField;
                    JSMemberField field3 = ((field2 != null) ? ((JSMemberField) field2.field) : ((JSMemberField) member)) as JSMemberField;
                    if (field3 == null)
                    {
                        if ((!member.IsFamily && !member.IsFamilyOrAssembly) || !InsideClassThatExtends(base.Globals.ScopeStack.Peek(), member.ReflectedType))
                        {
                            if (this.giveErrors)
                            {
                                base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                            }
                            return false;
                        }
                    }
                    else if (!field3.IsAccessibleFrom(base.Globals.ScopeStack.Peek()))
                    {
                        if (this.giveErrors)
                        {
                            base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                        }
                        return false;
                    }
                }
                if (member.IsLiteral && (member is JSVariableField))
                {
                    ClassScope scope = ((JSVariableField) member).value as ClassScope;
                    if ((scope != null) && !scope.owner.IsStatic)
                    {
                        Lookup lookup = this as Lookup;
                        if ((((lookup != null) && lookup.InStaticCode()) && !lookup.InFunctionNestedInsideInstanceMethod()) && this.giveErrors)
                        {
                            base.context.HandleError(JSError.InstanceNotAccessibleFromStatic, this.isFullyResolved);
                        }
                        return true;
                    }
                }
                if ((member.IsStatic || member.IsLiteral) || (((this.defaultMember != null) || !(this is Lookup)) || !((Lookup) this).InStaticCode()))
                {
                    return true;
                }
                if ((member is JSWrappedField) && (member.DeclaringType == Typeob.LenientGlobalObject))
                {
                    return true;
                }
                if (this.giveErrors)
                {
                    if ((!member.IsStatic && (this is Lookup)) && ((Lookup) this).InStaticCode())
                    {
                        base.context.HandleError(JSError.InstanceNotAccessibleFromStatic, this.isFullyResolved);
                    }
                    else
                    {
                        base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                    }
                }
            }
            return false;
        }

        private bool AccessibleMethod()
        {
            MethodInfo member = (MethodInfo) this.member;
            return this.AccessibleMethod(member);
        }

        private bool AccessibleMethod(MethodInfo meth)
        {
            if (meth != null)
            {
                if (this.isNonVirtual && meth.IsAbstract)
                {
                    base.context.HandleError(JSError.InvalidCall);
                    return false;
                }
                if (!meth.IsPublic)
                {
                    JSWrappedMethod method = meth as JSWrappedMethod;
                    if (method != null)
                    {
                        meth = method.method;
                    }
                    JSClosureMethod method2 = meth as JSClosureMethod;
                    JSFieldMethod method3 = ((method2 != null) ? ((JSFieldMethod) method2.method) : ((JSFieldMethod) meth)) as JSFieldMethod;
                    if (method3 == null)
                    {
                        if ((meth.IsFamily || meth.IsFamilyOrAssembly) && InsideClassThatExtends(base.Globals.ScopeStack.Peek(), meth.ReflectedType))
                        {
                            return true;
                        }
                        if (this.giveErrors)
                        {
                            base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                        }
                        return false;
                    }
                    if (!method3.IsAccessibleFrom(base.Globals.ScopeStack.Peek()))
                    {
                        if (this.giveErrors)
                        {
                            base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                        }
                        return false;
                    }
                }
                if ((meth.IsStatic || (this.defaultMember != null)) || (!(this is Lookup) || !((Lookup) this).InStaticCode()))
                {
                    return true;
                }
                if ((meth is JSWrappedMethod) && ((Lookup) this).CanPlaceAppropriateObjectOnStack(((JSWrappedMethod) meth).obj))
                {
                    return true;
                }
                if (this.giveErrors)
                {
                    if ((!meth.IsStatic && (this is Lookup)) && ((Lookup) this).InStaticCode())
                    {
                        base.context.HandleError(JSError.InstanceNotAccessibleFromStatic, this.isFullyResolved);
                    }
                    else
                    {
                        base.context.HandleError(JSError.NotAccessible, this.isFullyResolved);
                    }
                }
            }
            return false;
        }

        private bool AccessibleProperty(bool checkSetter)
        {
            PropertyInfo member = (PropertyInfo) this.member;
            if (this.AccessibleMethod(checkSetter ? JSProperty.GetSetMethod(member, true) : JSProperty.GetGetMethod(member, true)))
            {
                return true;
            }
            if (this.giveErrors && !checkSetter)
            {
                base.context.HandleError(JSError.WriteOnlyProperty);
            }
            return false;
        }

        private static bool ArrayAssignmentCompatible(AST ast, IReflect lhir)
        {
            if (Microsoft.JScript.Convert.IsArray(lhir))
            {
                if (lhir == Typeob.Array)
                {
                    ast.context.HandleError(JSError.ArrayMayBeCopied);
                    return true;
                }
                if (Microsoft.JScript.Convert.GetArrayRank(lhir) == 1)
                {
                    ast.context.HandleError(JSError.ArrayMayBeCopied);
                    return true;
                }
            }
            return false;
        }

        internal static bool AssignmentCompatible(IReflect lhir, AST rhexpr, IReflect rhir, bool reportError)
        {
            if (rhexpr is ConstantWrapper)
            {
                object obj2 = rhexpr.Evaluate();
                if (obj2 is ClassScope)
                {
                    if (((lhir == Typeob.Type) || (lhir == Typeob.Object)) || (lhir == Typeob.String))
                    {
                        return true;
                    }
                    if (reportError)
                    {
                        rhexpr.context.HandleError(JSError.TypeMismatch);
                    }
                    return false;
                }
                ClassScope classScope = lhir as ClassScope;
                if (classScope != null)
                {
                    EnumDeclaration owner = classScope.owner as EnumDeclaration;
                    if (owner != null)
                    {
                        ConstantWrapper wrapper = rhexpr as ConstantWrapper;
                        if ((wrapper != null) && (wrapper.value is string))
                        {
                            FieldInfo field = classScope.GetField((string) wrapper.value, BindingFlags.Public | BindingFlags.Static);
                            if (field == null)
                            {
                                return false;
                            }
                            owner.PartiallyEvaluate();
                            wrapper.value = new DeclaredEnumValue(((JSMemberField) field).value, field.Name, classScope);
                        }
                        if (rhir == Typeob.String)
                        {
                            return true;
                        }
                        lhir = owner.baseType.ToType();
                    }
                }
                else if (lhir is Type)
                {
                    Type enumType = lhir as Type;
                    if (enumType.IsEnum)
                    {
                        ConstantWrapper wrapper2 = rhexpr as ConstantWrapper;
                        if ((wrapper2 != null) && (wrapper2.value is string))
                        {
                            FieldInfo info2 = enumType.GetField((string) wrapper2.value, BindingFlags.Public | BindingFlags.Static);
                            if (info2 == null)
                            {
                                return false;
                            }
                            wrapper2.value = MetadataEnumValue.GetEnumValue(info2.FieldType, info2.GetRawConstantValue());
                        }
                        if (rhir == Typeob.String)
                        {
                            return true;
                        }
                        lhir = Enum.GetUnderlyingType(enumType);
                    }
                }
                if (lhir is Type)
                {
                    try
                    {
                        Microsoft.JScript.Convert.CoerceT(obj2, (Type) lhir);
                        return true;
                    }
                    catch
                    {
                        if ((lhir == Typeob.Single) && (obj2 is double))
                        {
                            if (((ConstantWrapper) rhexpr).isNumericLiteral)
                            {
                                return true;
                            }
                            double num = (double) obj2;
                            float num2 = (float) num;
                            if (num.ToString(CultureInfo.InvariantCulture).Equals(num2.ToString(CultureInfo.InvariantCulture)))
                            {
                                ((ConstantWrapper) rhexpr).value = num2;
                                return true;
                            }
                        }
                        if (lhir == Typeob.Decimal)
                        {
                            ConstantWrapper wrapper3 = rhexpr as ConstantWrapper;
                            if ((wrapper3 != null) && wrapper3.isNumericLiteral)
                            {
                                try
                                {
                                    Microsoft.JScript.Convert.CoerceT(wrapper3.context.GetCode(), Typeob.Decimal);
                                    return true;
                                }
                                catch
                                {
                                }
                            }
                        }
                        if (reportError)
                        {
                            rhexpr.context.HandleError(JSError.TypeMismatch);
                        }
                    }
                    return false;
                }
            }
            else if (rhexpr is ArrayLiteral)
            {
                return ((ArrayLiteral) rhexpr).AssignmentCompatible(lhir, reportError);
            }
            if (rhir == Typeob.Object)
            {
                return true;
            }
            if ((rhir == Typeob.Double) && Microsoft.JScript.Convert.IsPrimitiveNumericType(lhir))
            {
                return true;
            }
            if ((((lhir is Type) && Typeob.Delegate.IsAssignableFrom((Type) lhir)) && ((rhir == Typeob.ScriptFunction) && (rhexpr is Binding))) && ((Binding) rhexpr).IsCompatibleWithDelegate((Type) lhir))
            {
                return true;
            }
            if (Microsoft.JScript.Convert.IsPromotableTo(rhir, lhir))
            {
                return true;
            }
            if (Microsoft.JScript.Convert.IsJScriptArray(rhir) && ArrayAssignmentCompatible(rhexpr, lhir))
            {
                return true;
            }
            if (lhir == Typeob.String)
            {
                return true;
            }
            if ((rhir == Typeob.String) && ((lhir == Typeob.Boolean) || Microsoft.JScript.Convert.IsPrimitiveNumericType(lhir)))
            {
                if (reportError)
                {
                    rhexpr.context.HandleError(JSError.PossibleBadConversionFromString);
                }
                return true;
            }
            if (((lhir == Typeob.Char) && (rhir == Typeob.String)) || (Microsoft.JScript.Convert.IsPromotableTo(lhir, rhir) || (Microsoft.JScript.Convert.IsPrimitiveNumericType(lhir) && Microsoft.JScript.Convert.IsPrimitiveNumericType(rhir))))
            {
                if (reportError)
                {
                    rhexpr.context.HandleError(JSError.PossibleBadConversion);
                }
                return true;
            }
            if (reportError)
            {
                rhexpr.context.HandleError(JSError.TypeMismatch);
            }
            return false;
        }

        internal void CheckIfDeletable()
        {
            if ((this.member != null) || (this.defaultMember != null))
            {
                base.context.HandleError(JSError.NotDeletable);
            }
            this.member = null;
            this.defaultMember = null;
        }

        internal void CheckIfUseless()
        {
            if ((this.members != null) && (this.members.Length != 0))
            {
                base.context.HandleError(JSError.UselessExpression);
            }
        }

        internal static bool CheckParameters(ParameterInfo[] pars, IReflect[] argIRs, ASTList argAST, Context ctx)
        {
            return CheckParameters(pars, argIRs, argAST, ctx, 0, false, true);
        }

        internal static bool CheckParameters(ParameterInfo[] pars, IReflect[] argIRs, ASTList argAST, Context ctx, int offset, bool defaultIsUndefined, bool reportError)
        {
            int length = argIRs.Length;
            int num2 = pars.Length;
            bool flag = false;
            if (length > (num2 - offset))
            {
                length = num2 - offset;
                flag = true;
            }
            for (int i = 0; i < length; i++)
            {
                IReflect lhir = (pars[i + offset] is ParameterDeclaration) ? ((ParameterDeclaration) pars[i + offset]).ParameterIReflect : pars[i + offset].ParameterType;
                IReflect rhir = argIRs[i];
                if (((i == (length - 1)) && (((lhir is Type) && Typeob.Array.IsAssignableFrom((Type) lhir)) || (lhir is TypedArray))) && Microsoft.JScript.CustomAttribute.IsDefined(pars[i + offset], typeof(ParamArrayAttribute), false))
                {
                    flag = false;
                    int num4 = argIRs.Length;
                    if ((i != (num4 - 1)) || !AssignmentCompatible(lhir, argAST[i], argIRs[i], false))
                    {
                        IReflect reflect3 = (lhir is Type) ? ((Type) lhir).GetElementType() : ((TypedArray) lhir).elementType;
                        for (int j = i; j < num4; j++)
                        {
                            if (!AssignmentCompatible(reflect3, argAST[j], argIRs[j], reportError))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                if (!AssignmentCompatible(lhir, argAST[i], rhir, reportError))
                {
                    return false;
                }
            }
            if (flag && reportError)
            {
                ctx.HandleError(JSError.TooManyParameters);
            }
            if (((offset == 0) && (length < num2)) && !defaultIsUndefined)
            {
                for (int k = length; k < num2; k++)
                {
                    if (TypeReferences.GetDefaultParameterValue(pars[k]) == System.Convert.DBNull)
                    {
                        ParameterDeclaration declaration = pars[k] as ParameterDeclaration;
                        if (declaration != null)
                        {
                            declaration.PartiallyEvaluate();
                        }
                        if ((k < (num2 - 1)) || !Microsoft.JScript.CustomAttribute.IsDefined(pars[k], typeof(ParamArrayAttribute), false))
                        {
                            if (reportError)
                            {
                                ctx.HandleError(JSError.TooFewParameters);
                            }
                            IReflect reflect4 = (pars[k + offset] is ParameterDeclaration) ? ((ParameterDeclaration) pars[k + offset]).ParameterIReflect : pars[k + offset].ParameterType;
                            Type type = reflect4 as Type;
                            if (((type != null) && type.IsValueType) && (!type.IsPrimitive && !type.IsEnum))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        internal override bool Delete()
        {
            return this.EvaluateAsLateBinding().Delete();
        }

        internal override object Evaluate()
        {
            object obj2 = this.GetObject();
            MemberInfo member = this.member;
            if (member != null)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Event:
                        return null;

                    case MemberTypes.Field:
                        return ((FieldInfo) member).GetValue(obj2);

                    case MemberTypes.Property:
                    {
                        MemberInfo[] members = new MemberInfo[] { JSProperty.GetGetMethod((PropertyInfo) member, false) };
                        return LateBinding.CallOneOfTheMembers(members, new object[0], false, obj2, null, null, null, base.Engine);
                    }
                    case MemberTypes.NestedType:
                        return member;
                }
            }
            if ((this.members == null) || (this.members.Length <= 0))
            {
                return this.EvaluateAsLateBinding().GetValue();
            }
            if ((this.members.Length == 1) && (this.members[0].MemberType == MemberTypes.Method))
            {
                MethodInfo method = (MethodInfo) this.members[0];
                Type type = (method is JSMethod) ? null : method.DeclaringType;
                if ((type == Typeob.GlobalObject) || ((((type != null) && (type != Typeob.StringObject)) && ((type != Typeob.NumberObject) && (type != Typeob.BooleanObject))) && type.IsSubclassOf(Typeob.JSObject)))
                {
                    return Globals.BuiltinFunctionFor(obj2, TypeReferences.ToExecutionContext(method));
                }
            }
            return new FunctionWrapper(this.name, obj2, this.members);
        }

        private IReflect[] GetAllEligibleClasses()
        {
            ArrayList result = new ArrayList(0x10);
            ClassScope excludedClass = null;
            PackageScope package = null;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while ((parent is WithObject) || (parent is BlockScope))
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                parent = ((FunctionScope) parent).owner.enclosing_scope;
            }
            if (parent is ClassScope)
            {
                excludedClass = (ClassScope) parent;
                package = excludedClass.package;
            }
            if (excludedClass != null)
            {
                excludedClass.AddClassesFromInheritanceChain(this.name, result);
            }
            if (package != null)
            {
                package.AddClassesExcluding(excludedClass, this.name, result);
            }
            else
            {
                ((IActivationObject) parent).GetGlobalScope().AddClassesExcluding(excludedClass, this.name, result);
            }
            IReflect[] array = new IReflect[result.Count];
            result.CopyTo(array);
            return array;
        }

        private MemberInfoList GetAllKnownInstanceBindingsForThisName()
        {
            IReflect[] allEligibleClasses = this.GetAllEligibleClasses();
            MemberInfoList list = new MemberInfoList();
            foreach (IReflect reflect in allEligibleClasses)
            {
                if (reflect is ClassScope)
                {
                    if (((ClassScope) reflect).ParentIsInSamePackage())
                    {
                        list.AddRange(reflect.GetMember(this.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                    }
                    else
                    {
                        list.AddRange(reflect.GetMember(this.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
                    }
                }
                else
                {
                    list.AddRange(reflect.GetMember(this.name, BindingFlags.Public | BindingFlags.Instance));
                }
            }
            return list;
        }

        private MethodInfo GetMethodInfoMetadata(MethodInfo method)
        {
            if (method is JSMethod)
            {
                return ((JSMethod) method).GetMethodInfo(base.compilerGlobals);
            }
            if (method is JSMethodInfo)
            {
                return ((JSMethodInfo) method).method;
            }
            return method;
        }

        protected abstract object GetObject();
        protected abstract void HandleNoSuchMemberError();
        internal override IReflect InferType(JSField inference_target)
        {
            if (this.isArrayElementAccess)
            {
                IReflect defaultMemberReturnIR = this.defaultMemberReturnIR;
                if (defaultMemberReturnIR is TypedArray)
                {
                    return ((TypedArray) defaultMemberReturnIR).elementType;
                }
                return ((Type) defaultMemberReturnIR).GetElementType();
            }
            if (this.isAssignmentToDefaultIndexedProperty)
            {
                if (this.member is PropertyInfo)
                {
                    return ((PropertyInfo) this.member).PropertyType;
                }
                return Typeob.Object;
            }
            MemberInfo member = this.member;
            if (member is FieldInfo)
            {
                JSWrappedField field = member as JSWrappedField;
                if (field != null)
                {
                    member = field.wrappedField;
                }
                if (member is JSVariableField)
                {
                    return ((JSVariableField) member).GetInferredType(inference_target);
                }
                return ((FieldInfo) member).FieldType;
            }
            if (member is PropertyInfo)
            {
                JSWrappedProperty property = member as JSWrappedProperty;
                if (property != null)
                {
                    member = property.property;
                }
                if (member is JSProperty)
                {
                    return ((JSProperty) member).PropertyIR();
                }
                PropertyInfo info2 = (PropertyInfo) member;
                if (info2.DeclaringType == Typeob.GlobalObject)
                {
                    return (IReflect) info2.GetValue(base.Globals.globalObject, null);
                }
                return info2.PropertyType;
            }
            if (member is Type)
            {
                return Typeob.Type;
            }
            if (member is EventInfo)
            {
                return Typeob.EventInfo;
            }
            if ((this.members.Length > 0) && base.Engine.doFast)
            {
                return Typeob.ScriptFunction;
            }
            return Typeob.Object;
        }

        internal virtual IReflect InferTypeOfCall(JSField inference_target, bool isConstructor)
        {
            if (this.isFullyResolved)
            {
                if (this.isArrayConstructor)
                {
                    return this.defaultMemberReturnIR;
                }
                if (this.isArrayElementAccess)
                {
                    IReflect defaultMemberReturnIR = this.defaultMemberReturnIR;
                    if (defaultMemberReturnIR is TypedArray)
                    {
                        return ((TypedArray) defaultMemberReturnIR).elementType;
                    }
                    return ((Type) defaultMemberReturnIR).GetElementType();
                }
                MemberInfo member = this.member;
                if (member is JSFieldMethod)
                {
                    if (!isConstructor)
                    {
                        return ((JSFieldMethod) member).ReturnIR();
                    }
                    return Typeob.Object;
                }
                if (member is MethodInfo)
                {
                    return ((MethodInfo) member).ReturnType;
                }
                if (member is JSConstructor)
                {
                    return ((JSConstructor) member).GetClassScope();
                }
                if (member is ConstructorInfo)
                {
                    return ((ConstructorInfo) member).DeclaringType;
                }
                if (member is Type)
                {
                    return (Type) member;
                }
                if ((member is FieldInfo) && ((FieldInfo) member).IsLiteral)
                {
                    object obj2 = (member is JSVariableField) ? ((JSVariableField) member).value : TypeReferences.GetConstantValue((FieldInfo) member);
                    if ((obj2 is ClassScope) || (obj2 is TypedArray))
                    {
                        return (IReflect) obj2;
                    }
                }
            }
            return Typeob.Object;
        }

        private static bool InsideClassThatExtends(ScriptObject scope, Type type)
        {
            while ((scope is WithObject) || (scope is BlockScope))
            {
                scope = scope.GetParent();
            }
            if (scope is ClassScope)
            {
                return type.IsAssignableFrom(((ClassScope) scope).GetBakedSuperType());
            }
            return ((scope is FunctionScope) && InsideClassThatExtends(((FunctionScope) scope).owner.enclosing_scope, type));
        }

        internal void InvalidateBinding()
        {
            this.isAssignmentToDefaultIndexedProperty = false;
            this.isArrayConstructor = false;
            this.isArrayElementAccess = false;
            this.defaultMember = null;
            this.member = null;
            this.members = new MemberInfo[0];
        }

        internal bool IsCompatibleWithDelegate(Type delegateType)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method.GetParameters();
            Type returnType = method.ReturnType;
            foreach (MemberInfo info2 in this.members)
            {
                if (info2 is MethodInfo)
                {
                    MethodInfo info3 = (MethodInfo) info2;
                    Type bakedSuperType = null;
                    if (info3 is JSFieldMethod)
                    {
                        IReflect ir = ((JSFieldMethod) info3).ReturnIR();
                        if (ir is ClassScope)
                        {
                            bakedSuperType = ((ClassScope) ir).GetBakedSuperType();
                        }
                        else if (ir is Type)
                        {
                            bakedSuperType = (Type) ir;
                        }
                        else
                        {
                            bakedSuperType = Microsoft.JScript.Convert.ToType(ir);
                        }
                        if (((JSFieldMethod) info3).func.isExpandoMethod)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        bakedSuperType = info3.ReturnType;
                    }
                    if ((bakedSuperType == returnType) && Class.ParametersMatch(parameters, info3.GetParameters()))
                    {
                        this.member = info3;
                        this.isFullyResolved = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsMissing(object value)
        {
            return (value is Microsoft.JScript.Missing);
        }

        private MethodInfo LookForParameterlessPropertyGetter()
        {
            int index = 0;
            int length = this.members.Length;
            while (index < length)
            {
                PropertyInfo info = this.members[index] as PropertyInfo;
                if (info != null)
                {
                    MethodInfo getMethod = info.GetGetMethod(true);
                    if (getMethod == null)
                    {
                        goto Label_0049;
                    }
                    ParameterInfo[] parameters = getMethod.GetParameters();
                    if ((parameters == null) || (parameters.Length == 0))
                    {
                        goto Label_0049;
                    }
                }
                return null;
            Label_0049:
                index++;
            }
            try
            {
                MethodInfo info3 = JSBinder.SelectMethod(this.members, new IReflect[0]);
                if ((info3 != null) && info3.IsSpecialName)
                {
                    return info3;
                }
            }
            catch (AmbiguousMatchException)
            {
            }
            return null;
        }

        internal override bool OkToUseAsType()
        {
            MemberInfo member = this.member;
            if (member is Type)
            {
                return (this.isFullyResolved = true);
            }
            if (member is FieldInfo)
            {
                FieldInfo info2 = (FieldInfo) member;
                if (info2.IsLiteral)
                {
                    if (((info2 is JSMemberField) && (((JSMemberField) info2).value is ClassScope)) && !info2.IsStatic)
                    {
                        return false;
                    }
                    return (this.isFullyResolved = true);
                }
                if ((!(member is JSField) && info2.IsStatic) && (info2.GetValue(null) is Type))
                {
                    return (this.isFullyResolved = true);
                }
            }
            return false;
        }

        private bool ParameterlessPropertyValueIsCallable(MethodInfo meth, ASTList args, IReflect[] argIRs, bool constructor, bool brackets)
        {
            ParameterInfo[] parameters = meth.GetParameters();
            if ((parameters == null) || (parameters.Length == 0))
            {
                if (((meth is JSWrappedMethod) && (((JSWrappedMethod) meth).GetWrappedObject() is GlobalObject)) || ((argIRs.Length > 0) || (!(meth is JSMethod) && Typeob.ScriptFunction.IsAssignableFrom(meth.ReturnType))))
                {
                    this.member = this.ResolveOtherKindOfCall(args, argIRs, constructor, brackets);
                    return true;
                }
                IReflect reflect = (meth is JSFieldMethod) ? ((JSFieldMethod) meth).ReturnIR() : meth.ReturnType;
                if ((reflect == Typeob.Object) || (reflect == Typeob.ScriptFunction))
                {
                    this.member = this.ResolveOtherKindOfCall(args, argIRs, constructor, brackets);
                    return true;
                }
                base.context.HandleError(JSError.InvalidCall);
            }
            return false;
        }

        internal static void PlaceArgumentsOnStack(ILGenerator il, ParameterInfo[] pars, ASTList args, int offset, int rhoffset, AST missing)
        {
            int count = args.count;
            int num2 = count + offset;
            int i = pars.Length - rhoffset;
            bool flag = ((i > 0) && Microsoft.JScript.CustomAttribute.IsDefined(pars[i - 1], typeof(ParamArrayAttribute), false)) && ((count != i) || !Microsoft.JScript.Convert.IsArrayType(args[count - 1].InferType(null)));
            Type cls = flag ? pars[--i].ParameterType.GetElementType() : null;
            if (num2 > i)
            {
                num2 = i;
            }
            for (int j = offset; j < num2; j++)
            {
                Type parameterType = pars[j].ParameterType;
                AST ast = args[j - offset];
                if ((ast is ConstantWrapper) && (((ConstantWrapper) ast).value == System.Reflection.Missing.Value))
                {
                    object defaultParameterValue = TypeReferences.GetDefaultParameterValue(pars[j]);
                    ((ConstantWrapper) ast).value = (defaultParameterValue != System.Convert.DBNull) ? defaultParameterValue : null;
                }
                if (parameterType.IsByRef)
                {
                    ast.TranslateToILReference(il, parameterType.GetElementType());
                }
                else
                {
                    ast.TranslateToIL(il, parameterType);
                }
            }
            if (num2 < i)
            {
                for (int k = num2; k < i; k++)
                {
                    Type rtype = pars[k].ParameterType;
                    if (TypeReferences.GetDefaultParameterValue(pars[k]) == System.Convert.DBNull)
                    {
                        if (rtype.IsByRef)
                        {
                            missing.TranslateToILReference(il, rtype.GetElementType());
                        }
                        else
                        {
                            missing.TranslateToIL(il, rtype);
                        }
                    }
                    else if (rtype.IsByRef)
                    {
                        new ConstantWrapper(TypeReferences.GetDefaultParameterValue(pars[k]), null).TranslateToILReference(il, rtype.GetElementType());
                    }
                    else
                    {
                        new ConstantWrapper(TypeReferences.GetDefaultParameterValue(pars[k]), null).TranslateToIL(il, rtype);
                    }
                }
            }
            if (flag)
            {
                num2 -= offset;
                i = (count > num2) ? (count - num2) : 0;
                ConstantWrapper.TranslateToILInt(il, i);
                il.Emit(OpCodes.Newarr, cls);
                bool flag2 = cls.IsValueType && !cls.IsPrimitive;
                for (int m = 0; m < i; m++)
                {
                    il.Emit(OpCodes.Dup);
                    ConstantWrapper.TranslateToILInt(il, m);
                    if (flag2)
                    {
                        il.Emit(OpCodes.Ldelema, cls);
                    }
                    args[m + num2].TranslateToIL(il, cls);
                    TranslateToStelem(il, cls);
                }
            }
        }

        private int PlaceValuesForHiddenParametersOnStack(ILGenerator il, MethodInfo meth, ParameterInfo[] pars)
        {
            int num = 0;
            if (meth is JSFieldMethod)
            {
                FunctionObject func = ((JSFieldMethod) meth).func;
                if ((func == null) || !func.isMethod)
                {
                    if (this is Lookup)
                    {
                        ((Lookup) this).TranslateToILDefaultThisObject(il);
                    }
                    else
                    {
                        this.TranslateToILObject(il, Typeob.Object, false);
                    }
                    base.EmitILToLoadEngine(il);
                }
                return 0;
            }
            object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(meth, typeof(JSFunctionAttribute), false);
            if ((objArray == null) || (objArray.Length == 0))
            {
                return 0;
            }
            JSFunctionAttributeEnum attributeValue = ((JSFunctionAttribute) objArray[0]).attributeValue;
            if ((attributeValue & JSFunctionAttributeEnum.HasThisObject) != JSFunctionAttributeEnum.None)
            {
                num = 1;
                Type parameterType = pars[0].ParameterType;
                if ((this is Lookup) && (parameterType == Typeob.Object))
                {
                    ((Lookup) this).TranslateToILDefaultThisObject(il);
                }
                else if (Typeob.ArrayObject.IsAssignableFrom(this.member.DeclaringType))
                {
                    this.TranslateToILObject(il, Typeob.ArrayObject, false);
                }
                else
                {
                    this.TranslateToILObject(il, parameterType, false);
                }
            }
            if ((attributeValue & JSFunctionAttributeEnum.HasEngine) != JSFunctionAttributeEnum.None)
            {
                num++;
                base.EmitILToLoadEngine(il);
            }
            return num;
        }

        internal bool RefersToMemoryLocation()
        {
            if (!this.isFullyResolved)
            {
                return false;
            }
            return (this.isArrayElementAccess || (this.member is FieldInfo));
        }

        internal override void ResolveCall(ASTList args, IReflect[] argIRs, bool constructor, bool brackets)
        {
            this.argIRs = argIRs;
            if ((this.members == null) || (this.members.Length == 0))
            {
                if ((constructor && this.isFullyResolved) && base.Engine.doFast)
                {
                    if ((this.member != null) && ((this.member is Type) || ((this.member is FieldInfo) && ((FieldInfo) this.member).IsLiteral)))
                    {
                        base.context.HandleError(JSError.NoConstructor);
                    }
                    else
                    {
                        this.HandleNoSuchMemberError();
                    }
                }
                else
                {
                    this.HandleNoSuchMemberError();
                }
            }
            else
            {
                MemberInfo target = null;
                if (!(this is CallableExpression) && (!constructor || !brackets))
                {
                    try
                    {
                        if (constructor)
                        {
                            this.member = target = JSBinder.SelectConstructor(this.members, argIRs);
                        }
                        else
                        {
                            MethodInfo info2;
                            this.member = target = info2 = JSBinder.SelectMethod(this.members, argIRs);
                            if ((info2 != null) && info2.IsSpecialName)
                            {
                                if (this.name == info2.Name)
                                {
                                    if (this.name.StartsWith("get_", StringComparison.Ordinal) || this.name.StartsWith("set_", StringComparison.Ordinal))
                                    {
                                        base.context.HandleError(JSError.NotMeantToBeCalledDirectly);
                                        this.member = null;
                                        return;
                                    }
                                }
                                else if (this.ParameterlessPropertyValueIsCallable(info2, args, argIRs, constructor, brackets))
                                {
                                    return;
                                }
                            }
                        }
                    }
                    catch (AmbiguousMatchException)
                    {
                        if (constructor)
                        {
                            base.context.HandleError(JSError.AmbiguousConstructorCall, this.isFullyResolved);
                        }
                        else
                        {
                            MethodInfo meth = this.LookForParameterlessPropertyGetter();
                            if ((meth == null) || !this.ParameterlessPropertyValueIsCallable(meth, args, argIRs, constructor, brackets))
                            {
                                base.context.HandleError(JSError.AmbiguousMatch, this.isFullyResolved);
                                this.member = null;
                            }
                        }
                        return;
                    }
                    catch (JScriptException exception)
                    {
                        base.context.HandleError(((JSError) exception.ErrorNumber) & ((JSError) 0xffff), exception.Message, true);
                        return;
                    }
                }
                if (target == null)
                {
                    target = this.ResolveOtherKindOfCall(args, argIRs, constructor, brackets);
                }
                if (target != null)
                {
                    if (!this.Accessible(false))
                    {
                        this.member = null;
                    }
                    else
                    {
                        this.WarnIfObsolete();
                        if (target is MethodBase)
                        {
                            if (Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(JSFunctionAttribute), false) && !(this.defaultMember is PropertyInfo))
                            {
                                int offset = 0;
                                JSFunctionAttributeEnum attributeValue = ((JSFunctionAttribute) Microsoft.JScript.CustomAttribute.GetCustomAttributes(target, typeof(JSFunctionAttribute), false)[0]).attributeValue;
                                if ((constructor && !(target is ConstructorInfo)) || ((attributeValue & JSFunctionAttributeEnum.HasArguments) != JSFunctionAttributeEnum.None))
                                {
                                    this.member = LateBinding.SelectMember(this.members);
                                    this.defaultMember = null;
                                }
                                else
                                {
                                    if ((attributeValue & JSFunctionAttributeEnum.HasThisObject) != JSFunctionAttributeEnum.None)
                                    {
                                        offset = 1;
                                    }
                                    if ((attributeValue & JSFunctionAttributeEnum.HasEngine) != JSFunctionAttributeEnum.None)
                                    {
                                        offset++;
                                    }
                                    if (!CheckParameters(((MethodBase) target).GetParameters(), argIRs, args, base.context, offset, true, this.isFullyResolved))
                                    {
                                        this.member = null;
                                    }
                                }
                            }
                            else if (constructor && (target is JSFieldMethod))
                            {
                                this.member = LateBinding.SelectMember(this.members);
                            }
                            else if ((constructor && (target is ConstructorInfo)) && (!(target is JSConstructor) && Typeob.Delegate.IsAssignableFrom(target.DeclaringType)))
                            {
                                base.context.HandleError(JSError.DelegatesShouldNotBeExplicitlyConstructed);
                                this.member = null;
                            }
                            else if (!CheckParameters(((MethodBase) target).GetParameters(), argIRs, args, base.context, 0, false, this.isFullyResolved))
                            {
                                this.member = null;
                            }
                        }
                    }
                }
            }
        }

        internal override object ResolveCustomAttribute(ASTList args, IReflect[] argIRs, AST target)
        {
            try
            {
                this.ResolveCall(args, argIRs, true, false);
            }
            catch (AmbiguousMatchException)
            {
                base.context.HandleError(JSError.AmbiguousConstructorCall);
                return null;
            }
            JSConstructor member = this.member as JSConstructor;
            if (member != null)
            {
                ClassScope classScope = member.GetClassScope();
                if (classScope.owner.IsCustomAttribute())
                {
                    return classScope;
                }
            }
            else
            {
                ConstructorInfo info = this.member as ConstructorInfo;
                if (info != null)
                {
                    Type declaringType = info.DeclaringType;
                    if (Typeob.Attribute.IsAssignableFrom(declaringType) && (Microsoft.JScript.CustomAttribute.GetCustomAttributes(declaringType, typeof(AttributeUsageAttribute), false).Length > 0))
                    {
                        return declaringType;
                    }
                }
            }
            base.context.HandleError(JSError.InvalidCustomAttributeClassOrCtor);
            return null;
        }

        internal void ResolveLHValue()
        {
            MemberInfo info = this.member = LateBinding.SelectMember(this.members);
            if (((info != null) && !this.Accessible(true)) || ((this.member == null) && (this.members.Length > 0)))
            {
                base.context.HandleError(JSError.AssignmentToReadOnly, this.isFullyResolved);
                this.member = null;
                this.members = new MemberInfo[0];
            }
            else if (info is JSPrototypeField)
            {
                this.member = null;
                this.members = new MemberInfo[0];
            }
            else
            {
                this.WarnIfNotFullyResolved();
                this.WarnIfObsolete();
            }
        }

        private MemberInfo ResolveOtherKindOfCall(ASTList argList, IReflect[] argIRs, bool constructor, bool brackets)
        {
            MemberInfo boolean = this.member = LateBinding.SelectMember(this.members);
            if (((boolean is PropertyInfo) && !(boolean is JSProperty)) && (boolean.DeclaringType == Typeob.GlobalObject))
            {
                PropertyInfo info2 = (PropertyInfo) boolean;
                Type propertyType = info2.PropertyType;
                if (propertyType == Typeob.Type)
                {
                    boolean = (Type) info2.GetValue(null, null);
                }
                else if (constructor && brackets)
                {
                    MethodInfo info3 = propertyType.GetMethod("CreateInstance", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (info3 != null)
                    {
                        Type returnType = info3.ReturnType;
                        if (returnType == Typeob.BooleanObject)
                        {
                            boolean = Typeob.Boolean;
                        }
                        else if (returnType == Typeob.StringObject)
                        {
                            boolean = Typeob.String;
                        }
                        else
                        {
                            boolean = returnType;
                        }
                    }
                }
            }
            CallableExpression expression = this as CallableExpression;
            if (expression != null)
            {
                ConstantWrapper wrapper = expression.expression as ConstantWrapper;
                if ((wrapper != null) && (wrapper.InferType(null) is Type))
                {
                    boolean = new JSGlobalField(null, null, wrapper.value, FieldAttributes.Literal | FieldAttributes.Public);
                }
            }
            if (boolean is Type)
            {
                if (!constructor)
                {
                    if (!brackets && (argIRs.Length == 1))
                    {
                        return boolean;
                    }
                    base.context.HandleError(JSError.InvalidCall);
                    return (MemberInfo) (this.member = null);
                }
                if (!brackets)
                {
                    ConstructorInfo[] constructors = ((Type) boolean).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    if ((constructors == null) || (constructors.Length == 0))
                    {
                        base.context.HandleError(JSError.NoConstructor);
                        this.member = null;
                        return null;
                    }
                    this.members = constructors;
                    this.ResolveCall(argList, argIRs, true, brackets);
                    return this.member;
                }
                this.isArrayConstructor = true;
                this.defaultMember = boolean;
                this.defaultMemberReturnIR = new TypedArray((Type) boolean, argIRs.Length);
                int index = 0;
                int length = argIRs.Length;
                while (index < length)
                {
                    if ((argIRs[index] != Typeob.Object) && !Microsoft.JScript.Convert.IsPrimitiveNumericType(argIRs[index]))
                    {
                        argList[index].context.HandleError(JSError.TypeMismatch, this.isFullyResolved);
                        break;
                    }
                    index++;
                }
                return (this.member = boolean);
            }
            else
            {
                if (boolean is JSPrototypeField)
                {
                    return (MemberInfo) (this.member = null);
                }
                if ((boolean is FieldInfo) && ((FieldInfo) boolean).IsLiteral)
                {
                    if (!this.AccessibleField(false))
                    {
                        return (MemberInfo) (this.member = null);
                    }
                    object obj2 = (boolean is JSVariableField) ? ((JSVariableField) boolean).value : TypeReferences.GetConstantValue((FieldInfo) boolean);
                    if (!(obj2 is ClassScope) && !(obj2 is Type))
                    {
                        if (obj2 is TypedArray)
                        {
                            if (constructor)
                            {
                                if (brackets && (argIRs.Length != 0))
                                {
                                    this.isArrayConstructor = true;
                                    this.defaultMember = boolean;
                                    this.defaultMemberReturnIR = new TypedArray((IReflect) obj2, argIRs.Length);
                                    int num5 = 0;
                                    int num6 = argIRs.Length;
                                    while (num5 < num6)
                                    {
                                        if ((argIRs[num5] != Typeob.Object) && !Microsoft.JScript.Convert.IsPrimitiveNumericType(argIRs[num5]))
                                        {
                                            argList[num5].context.HandleError(JSError.TypeMismatch, this.isFullyResolved);
                                            break;
                                        }
                                        num5++;
                                    }
                                    return (this.member = boolean);
                                }
                            }
                            else if ((argIRs.Length == 1) && !brackets)
                            {
                                return (this.member = boolean);
                            }
                            goto Label_0919;
                        }
                        if (obj2 is FunctionObject)
                        {
                            FunctionObject obj3 = (FunctionObject) obj2;
                            if ((!obj3.isExpandoMethod && !obj3.Must_save_stack_locals) && ((obj3.own_scope.ProvidesOuterScopeLocals == null) || (obj3.own_scope.ProvidesOuterScopeLocals.count == 0)))
                            {
                                return (this.member = ((JSVariableField) this.member).GetAsMethod(obj3.own_scope));
                            }
                            return this.member;
                        }
                    }
                    else
                    {
                        if (!constructor)
                        {
                            if (!brackets && (argIRs.Length == 1))
                            {
                                Type type3 = obj2 as Type;
                                return (this.member = (type3 != null) ? type3 : boolean);
                            }
                            base.context.HandleError(JSError.InvalidCall);
                            return (MemberInfo) (this.member = null);
                        }
                        if (!brackets)
                        {
                            if ((obj2 is ClassScope) && !((ClassScope) obj2).owner.isStatic)
                            {
                                ConstantWrapper wrapper2 = null;
                                if (((this is Member) && ((wrapper2 = ((Member) this).rootObject as ConstantWrapper) != null)) && !(wrapper2.Evaluate() is Namespace))
                                {
                                    ((Member) this).rootObject.context.HandleError(JSError.NeedInstance);
                                    return null;
                                }
                            }
                            this.members = (obj2 is ClassScope) ? ((MemberInfo[]) ((ClassScope) obj2).constructors) : ((MemberInfo[]) ((Type) obj2).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
                            if ((this.members == null) || (this.members.Length == 0))
                            {
                                base.context.HandleError(JSError.NoConstructor);
                                this.member = null;
                                return null;
                            }
                            this.ResolveCall(argList, argIRs, true, brackets);
                            return this.member;
                        }
                        this.isArrayConstructor = true;
                        this.defaultMember = boolean;
                        this.defaultMemberReturnIR = new TypedArray((obj2 is ClassScope) ? ((IReflect) obj2) : ((IReflect) obj2), argIRs.Length);
                        int num3 = 0;
                        int num4 = argIRs.Length;
                        while (num3 < num4)
                        {
                            if ((argIRs[num3] != Typeob.Object) && !Microsoft.JScript.Convert.IsPrimitiveNumericType(argIRs[num3]))
                            {
                                argList[num3].context.HandleError(JSError.TypeMismatch, this.isFullyResolved);
                                break;
                            }
                            num3++;
                        }
                        return (this.member = boolean);
                    }
                }
            }
            IReflect ir = this.InferType(null);
            Type c = ir as Type;
            if (!brackets && (((c != null) && Typeob.ScriptFunction.IsAssignableFrom(c)) || (ir is ScriptFunction)))
            {
                this.defaultMember = boolean;
                if (c == null)
                {
                    this.defaultMemberReturnIR = Globals.TypeRefs.ToReferenceContext(ir.GetType());
                    this.member = this.defaultMemberReturnIR.GetMethod(constructor ? "CreateInstance" : "Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (this.member == null)
                    {
                        this.defaultMemberReturnIR = Typeob.ScriptFunction;
                        this.member = this.defaultMemberReturnIR.GetMethod(constructor ? "CreateInstance" : "Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    }
                    return this.member;
                }
                if ((constructor && (this.members.Length != 0)) && (this.members[0] is JSFieldMethod))
                {
                    JSFieldMethod method = (JSFieldMethod) this.members[0];
                    method.func.PartiallyEvaluate();
                    if (!method.func.isExpandoMethod)
                    {
                        base.context.HandleError(JSError.NotAnExpandoFunction, this.isFullyResolved);
                    }
                }
                this.defaultMemberReturnIR = c;
                return (this.member = c.GetMethod(constructor ? "CreateInstance" : "Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            }
            if (ir == Typeob.Type)
            {
                this.member = null;
                return null;
            }
            if ((ir == Typeob.Object) || (((ir is ScriptObject) && brackets) && !(ir is ClassScope)))
            {
                return boolean;
            }
            if (!(ir is TypedArray) && (!(ir is Type) || !((Type) ir).IsArray))
            {
                if (!constructor)
                {
                    if ((brackets && (ir == Typeob.String)) && ((this.argIRs.Length != 1) || !Microsoft.JScript.Convert.IsPrimitiveNumericType(argIRs[0])))
                    {
                        ir = Typeob.StringObject;
                    }
                    MemberInfo[] infoArray2 = (brackets || !(ir is ScriptObject)) ? JSBinder.GetDefaultMembers(ir) : null;
                    if ((infoArray2 != null) && (infoArray2.Length > 0))
                    {
                        try
                        {
                            this.defaultMember = boolean;
                            this.defaultMemberReturnIR = ir;
                            return (this.member = JSBinder.SelectMethod(this.members = infoArray2, argIRs));
                        }
                        catch (AmbiguousMatchException)
                        {
                            base.context.HandleError(JSError.AmbiguousMatch, this.isFullyResolved);
                            return (MemberInfo) (this.member = null);
                        }
                    }
                    if ((!brackets && (ir is Type)) && Typeob.Delegate.IsAssignableFrom((Type) ir))
                    {
                        this.defaultMember = boolean;
                        this.defaultMemberReturnIR = ir;
                        return (this.member = ((Type) ir).GetMethod("Invoke"));
                    }
                }
            }
            else
            {
                int num7 = argIRs.Length;
                int num8 = (ir is TypedArray) ? ((TypedArray) ir).rank : ((Type) ir).GetArrayRank();
                if (num7 != num8)
                {
                    base.context.HandleError(JSError.IncorrectNumberOfIndices, this.isFullyResolved);
                }
                else
                {
                    for (int i = 0; i < num8; i++)
                    {
                        if ((argIRs[i] != Typeob.Object) && (!Microsoft.JScript.Convert.IsPrimitiveNumericType(argIRs[i]) || Microsoft.JScript.Convert.IsBadIndex(argList[i])))
                        {
                            argList[i].context.HandleError(JSError.TypeMismatch, this.isFullyResolved);
                            break;
                        }
                    }
                }
                if (constructor)
                {
                    if (!brackets)
                    {
                        goto Label_0919;
                    }
                    if (ir is TypedArray)
                    {
                        IReflect elementType = ((TypedArray) ir).elementType;
                    }
                    else
                    {
                        ((Type) ir).GetElementType();
                    }
                    if (((ir != Typeob.Object) && !(ir is ClassScope)) && ((!(ir is Type) || Typeob.Type.IsAssignableFrom((Type) ir)) || Typeob.ScriptFunction.IsAssignableFrom((Type) ir)))
                    {
                        goto Label_0919;
                    }
                }
                this.isArrayElementAccess = true;
                this.defaultMember = boolean;
                this.defaultMemberReturnIR = ir;
                return null;
            }
        Label_0919:
            if (constructor)
            {
                base.context.HandleError(JSError.NeedType, this.isFullyResolved);
            }
            else if (brackets)
            {
                base.context.HandleError(JSError.NotIndexable, this.isFullyResolved);
            }
            else
            {
                base.context.HandleError(JSError.FunctionExpected, this.isFullyResolved);
            }
            return (MemberInfo) (this.member = null);
        }

        protected void ResolveRHValue()
        {
            MemberInfo info = this.member = LateBinding.SelectMember(this.members);
            JSLocalField member = this.member as JSLocalField;
            if (member != null)
            {
                FunctionObject obj2 = member.value as FunctionObject;
                if (obj2 != null)
                {
                    FunctionScope scope = obj2.enclosing_scope as FunctionScope;
                    if (scope != null)
                    {
                        scope.closuresMightEscape = true;
                    }
                }
            }
            if (info is JSPrototypeField)
            {
                this.member = null;
            }
            else if (!this.Accessible(false))
            {
                this.member = null;
            }
            else
            {
                this.WarnIfObsolete();
                this.WarnIfNotFullyResolved();
            }
        }

        internal override void SetPartialValue(AST partial_value)
        {
            AssignmentCompatible(this.InferType(null), partial_value, partial_value.InferType(null), this.isFullyResolved);
        }

        internal void SetPartialValue(ASTList argList, IReflect[] argIRs, AST partial_value, bool inBrackets)
        {
            if ((this.members == null) || (this.members.Length == 0))
            {
                this.HandleNoSuchMemberError();
                this.isAssignmentToDefaultIndexedProperty = true;
            }
            else
            {
                this.PartiallyEvaluate();
                IReflect ir = this.InferType(null);
                this.isAssignmentToDefaultIndexedProperty = true;
                if (ir == Typeob.Object)
                {
                    JSVariableField member = this.member as JSVariableField;
                    if (((member == null) || !member.IsLiteral) || !(member.value is ClassScope))
                    {
                        return;
                    }
                    ir = Typeob.Type;
                }
                else
                {
                    if ((ir is TypedArray) || ((ir is Type) && ((Type) ir).IsArray))
                    {
                        bool flag = false;
                        int length = argIRs.Length;
                        int num2 = (ir is TypedArray) ? ((TypedArray) ir).rank : ((Type) ir).GetArrayRank();
                        if (length != num2)
                        {
                            base.context.HandleError(JSError.IncorrectNumberOfIndices, this.isFullyResolved);
                            flag = true;
                        }
                        for (int i = 0; i < num2; i++)
                        {
                            if (((!flag && (i < length)) && (argIRs[i] != Typeob.Object)) && (!Microsoft.JScript.Convert.IsPrimitiveNumericType(argIRs[i]) || Microsoft.JScript.Convert.IsBadIndex(argList[i])))
                            {
                                argList[i].context.HandleError(JSError.TypeMismatch, this.isFullyResolved);
                                flag = true;
                            }
                        }
                        this.isArrayElementAccess = true;
                        this.isAssignmentToDefaultIndexedProperty = false;
                        this.defaultMember = this.member;
                        this.defaultMemberReturnIR = ir;
                        IReflect lhir = (ir is TypedArray) ? ((TypedArray) ir).elementType : ((Type) ir).GetElementType();
                        AssignmentCompatible(lhir, partial_value, partial_value.InferType(null), this.isFullyResolved);
                        return;
                    }
                    MemberInfo[] defaultMembers = JSBinder.GetDefaultMembers(ir);
                    if (((defaultMembers != null) && (defaultMembers.Length > 0)) && (this.member != null))
                    {
                        try
                        {
                            PropertyInfo prop = JSBinder.SelectProperty(defaultMembers, argIRs);
                            if (prop == null)
                            {
                                base.context.HandleError(JSError.NotIndexable, Microsoft.JScript.Convert.ToTypeName(ir));
                            }
                            else if (JSProperty.GetSetMethod(prop, true) == null)
                            {
                                if (ir == Typeob.String)
                                {
                                    base.context.HandleError(JSError.UselessAssignment);
                                }
                                else
                                {
                                    base.context.HandleError(JSError.AssignmentToReadOnly, this.isFullyResolved && base.Engine.doFast);
                                }
                            }
                            else if (CheckParameters(prop.GetIndexParameters(), argIRs, argList, base.context, 0, false, true))
                            {
                                this.defaultMember = this.member;
                                this.defaultMemberReturnIR = ir;
                                this.members = defaultMembers;
                                this.member = prop;
                            }
                        }
                        catch (AmbiguousMatchException)
                        {
                            base.context.HandleError(JSError.AmbiguousMatch, this.isFullyResolved);
                            this.member = null;
                        }
                        return;
                    }
                }
                this.member = null;
                if (!inBrackets)
                {
                    base.context.HandleError(JSError.IllegalAssignment);
                }
                else
                {
                    base.context.HandleError(JSError.NotIndexable, Microsoft.JScript.Convert.ToTypeName(ir));
                }
            }
        }

        internal override void SetValue(object value)
        {
            MemberInfo member = this.member;
            object obj2 = this.GetObject();
            if (member is FieldInfo)
            {
                FieldInfo info2 = (FieldInfo) member;
                if (!info2.IsLiteral && !info2.IsInitOnly)
                {
                    if (!(info2 is JSField) || (info2 is JSWrappedField))
                    {
                        value = Microsoft.JScript.Convert.CoerceT(value, info2.FieldType, false);
                    }
                    info2.SetValue(obj2, value, BindingFlags.SuppressChangeType, null, null);
                }
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo prop = (PropertyInfo) member;
                if ((obj2 is ClassScope) && !(prop is JSProperty))
                {
                    JSProperty.SetValue(prop, ((WithObject) ((ClassScope) obj2).GetParent()).contained_object, value, null);
                }
                else
                {
                    if (!(prop is JSProperty))
                    {
                        value = Microsoft.JScript.Convert.CoerceT(value, prop.PropertyType, false);
                    }
                    JSProperty.SetValue(prop, obj2, value, null);
                }
            }
            else
            {
                if ((this.members != null) && (this.members.Length != 0))
                {
                    throw new JScriptException(JSError.IllegalAssignment);
                }
                this.EvaluateAsLateBinding().SetValue(value);
            }
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            this.TranslateToIL(il, rtype, false, false);
        }

        internal void TranslateToIL(ILGenerator il, Type rtype, bool calledFromDelete)
        {
            this.TranslateToIL(il, rtype, false, false, calledFromDelete);
        }

        private void TranslateToIL(ILGenerator il, Type rtype, bool preSet, bool preSetPlusGet)
        {
            this.TranslateToIL(il, rtype, preSet, preSetPlusGet, false);
        }

        private void TranslateToIL(ILGenerator il, Type rtype, bool preSet, bool preSetPlusGet, bool calledFromDelete)
        {
            if (this.member is FieldInfo)
            {
                FieldInfo member = (FieldInfo) this.member;
                bool flag = member.IsStatic || member.IsLiteral;
                if (member.IsLiteral && (member is JSMemberField))
                {
                    FunctionObject obj3 = ((JSMemberField) member).value as FunctionObject;
                    flag = (obj3 == null) || !obj3.isExpandoMethod;
                }
                if (!flag || (member is JSClosureField))
                {
                    this.TranslateToILObject(il, member.DeclaringType, true);
                    if (preSetPlusGet)
                    {
                        il.Emit(OpCodes.Dup);
                    }
                    flag = false;
                }
                if (!preSet)
                {
                    object obj4 = (member is JSField) ? ((JSField) member).GetMetaData() : ((member is JSFieldInfo) ? ((JSFieldInfo) member).field : member);
                    if ((obj4 is FieldInfo) && !((FieldInfo) obj4).IsLiteral)
                    {
                        il.Emit(flag ? OpCodes.Ldsfld : OpCodes.Ldfld, (FieldInfo) obj4);
                    }
                    else if (obj4 is LocalBuilder)
                    {
                        il.Emit(OpCodes.Ldloc, (LocalBuilder) obj4);
                    }
                    else
                    {
                        if (member.IsLiteral)
                        {
                            new ConstantWrapper(TypeReferences.GetConstantValue(member), base.context).TranslateToIL(il, rtype);
                            return;
                        }
                        Microsoft.JScript.Convert.EmitLdarg(il, (short) obj4);
                    }
                    Microsoft.JScript.Convert.Emit(this, il, member.FieldType, rtype);
                }
            }
            else if (this.member is PropertyInfo)
            {
                PropertyInfo prop = (PropertyInfo) this.member;
                MethodInfo method = preSet ? JSProperty.GetSetMethod(prop, true) : JSProperty.GetGetMethod(prop, true);
                if (method == null)
                {
                    if (!preSet)
                    {
                        if (this is Lookup)
                        {
                            il.Emit(OpCodes.Ldc_I4, 0x13b1);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.scriptExceptionConstructor);
                            il.Emit(OpCodes.Throw);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                    }
                }
                else
                {
                    bool flag2 = method.IsStatic && !(method is JSClosureMethod);
                    if (!flag2)
                    {
                        Type declaringType = method.DeclaringType;
                        if ((declaringType == Typeob.StringObject) && method.Name.Equals("get_length"))
                        {
                            this.TranslateToILObject(il, Typeob.String, false);
                            method = CompilerGlobals.stringLengthMethod;
                        }
                        else
                        {
                            this.TranslateToILObject(il, declaringType, true);
                        }
                    }
                    if (!preSet)
                    {
                        method = this.GetMethodInfoMetadata(method);
                        if (flag2)
                        {
                            il.Emit(OpCodes.Call, method);
                        }
                        else
                        {
                            if (preSetPlusGet)
                            {
                                il.Emit(OpCodes.Dup);
                            }
                            if (((!this.isNonVirtual && method.IsVirtual) && !method.IsFinal) && (!method.ReflectedType.IsSealed || !method.ReflectedType.IsValueType))
                            {
                                il.Emit(OpCodes.Callvirt, method);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, method);
                            }
                        }
                        Microsoft.JScript.Convert.Emit(this, il, method.ReturnType, rtype);
                    }
                }
            }
            else if (this.member is MethodInfo)
            {
                MethodInfo methodInfoMetadata = this.GetMethodInfoMetadata((MethodInfo) this.member);
                if (Typeob.Delegate.IsAssignableFrom(rtype))
                {
                    if (!methodInfoMetadata.IsStatic)
                    {
                        Type obtype = methodInfoMetadata.DeclaringType;
                        this.TranslateToILObject(il, obtype, false);
                        if (obtype.IsValueType)
                        {
                            il.Emit(OpCodes.Box, obtype);
                        }
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    if ((methodInfoMetadata.IsVirtual && !methodInfoMetadata.IsFinal) && (!methodInfoMetadata.ReflectedType.IsSealed || !methodInfoMetadata.ReflectedType.IsValueType))
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldvirtftn, methodInfoMetadata);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldftn, methodInfoMetadata);
                    }
                    ConstructorInfo constructor = rtype.GetConstructor(new Type[] { Typeob.Object, Typeob.UIntPtr });
                    if (constructor == null)
                    {
                        constructor = rtype.GetConstructor(new Type[] { Typeob.Object, Typeob.IntPtr });
                    }
                    il.Emit(OpCodes.Newobj, constructor);
                }
                else if (this.member is JSExpandoIndexerMethod)
                {
                    MemberInfo info6 = this.member;
                    this.member = this.defaultMember;
                    this.TranslateToIL(il, Typeob.Object);
                    this.member = info6;
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
            else
            {
                object obj5 = null;
                if (this is Lookup)
                {
                    ((Lookup) this).TranslateToLateBinding(il);
                }
                else
                {
                    if ((!this.isFullyResolved && !preSet) && !preSetPlusGet)
                    {
                        obj5 = this.TranslateToSpeculativeEarlyBindings(il, rtype, false);
                    }
                    ((Member) this).TranslateToLateBinding(il, obj5 != null);
                    if (!this.isFullyResolved && preSetPlusGet)
                    {
                        obj5 = this.TranslateToSpeculativeEarlyBindings(il, rtype, true);
                    }
                }
                if (preSetPlusGet)
                {
                    il.Emit(OpCodes.Dup);
                }
                if (!preSet)
                {
                    if ((this is Lookup) && !calledFromDelete)
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.getValue2Method);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.getNonMissingValueMethod);
                    }
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                    if (obj5 != null)
                    {
                        il.MarkLabel((Label) obj5);
                    }
                }
            }
        }

        internal override void TranslateToILCall(ILGenerator il, Type rtype, ASTList argList, bool construct, bool brackets)
        {
            MemberInfo member = this.member;
            if (this.defaultMember != null)
            {
                if (this.isArrayConstructor)
                {
                    TypedArray array = (TypedArray) this.defaultMemberReturnIR;
                    Type cls = Microsoft.JScript.Convert.ToType(array.elementType);
                    int rank = array.rank;
                    if (rank == 1)
                    {
                        argList[0].TranslateToIL(il, Typeob.Int32);
                        il.Emit(OpCodes.Newarr, cls);
                    }
                    else
                    {
                        Type arrayClass = array.ToType();
                        Type[] parameterTypes = new Type[rank];
                        for (int i = 0; i < rank; i++)
                        {
                            parameterTypes[i] = Typeob.Int32;
                        }
                        int num3 = 0;
                        int count = argList.count;
                        while (num3 < count)
                        {
                            argList[num3].TranslateToIL(il, Typeob.Int32);
                            num3++;
                        }
                        TypeBuilder builder = cls as TypeBuilder;
                        if (builder != null)
                        {
                            MethodInfo meth = ((ModuleBuilder) arrayClass.Module).GetArrayMethod(arrayClass, ".ctor", CallingConventions.HasThis, Typeob.Void, parameterTypes);
                            il.Emit(OpCodes.Newobj, meth);
                        }
                        else
                        {
                            ConstructorInfo constructor = arrayClass.GetConstructor(parameterTypes);
                            il.Emit(OpCodes.Newobj, constructor);
                        }
                    }
                    Microsoft.JScript.Convert.Emit(this, il, array.ToType(), rtype);
                    return;
                }
                this.member = this.defaultMember;
                IReflect defaultMemberReturnIR = this.defaultMemberReturnIR;
                Type type3 = (defaultMemberReturnIR is Type) ? ((Type) defaultMemberReturnIR) : Microsoft.JScript.Convert.ToType(defaultMemberReturnIR);
                this.TranslateToIL(il, type3);
                if (this.isArrayElementAccess)
                {
                    int num5 = 0;
                    int num6 = argList.count;
                    while (num5 < num6)
                    {
                        argList[num5].TranslateToIL(il, Typeob.Int32);
                        num5++;
                    }
                    Type elementType = type3.GetElementType();
                    int arrayRank = type3.GetArrayRank();
                    if (arrayRank == 1)
                    {
                        TranslateToLdelem(il, elementType);
                    }
                    else
                    {
                        Type[] typeArray2 = new Type[arrayRank];
                        for (int j = 0; j < arrayRank; j++)
                        {
                            typeArray2[j] = Typeob.Int32;
                        }
                        MethodInfo info4 = base.compilerGlobals.module.GetArrayMethod(type3, "Get", CallingConventions.HasThis, elementType, typeArray2);
                        il.Emit(OpCodes.Call, info4);
                    }
                    Microsoft.JScript.Convert.Emit(this, il, elementType, rtype);
                    return;
                }
                this.member = member;
            }
            if (member is MethodInfo)
            {
                MethodInfo target = (MethodInfo) member;
                Type declaringType = target.DeclaringType;
                Type reflectedType = target.ReflectedType;
                ParameterInfo[] parameters = target.GetParameters();
                if (!target.IsStatic && (this.defaultMember == null))
                {
                    this.TranslateToILObject(il, declaringType, true);
                }
                if (target is JSClosureMethod)
                {
                    this.TranslateToILObject(il, declaringType, false);
                }
                ConstantWrapper missing = null;
                int offset = 0;
                if ((target is JSFieldMethod) || Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(JSFunctionAttribute), false))
                {
                    offset = this.PlaceValuesForHiddenParametersOnStack(il, target, parameters);
                    missing = JScriptMissingCW;
                }
                else
                {
                    missing = ReflectionMissingCW;
                }
                if (((argList.count == 1) && (missing == JScriptMissingCW)) && (this.defaultMember is PropertyInfo))
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Newarr, Typeob.Object);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4_0);
                    argList[0].TranslateToIL(il, Typeob.Object);
                    il.Emit(OpCodes.Stelem_Ref);
                }
                else
                {
                    PlaceArgumentsOnStack(il, parameters, argList, offset, 0, missing);
                }
                target = this.GetMethodInfoMetadata(target);
                if (((!this.isNonVirtual && target.IsVirtual) && !target.IsFinal) && (!reflectedType.IsSealed || !reflectedType.IsValueType))
                {
                    il.Emit(OpCodes.Callvirt, target);
                }
                else
                {
                    il.Emit(OpCodes.Call, target);
                }
                Microsoft.JScript.Convert.Emit(this, il, target.ReturnType, rtype);
            }
            else if (member is ConstructorInfo)
            {
                ConstructorInfo constructorInfo = (ConstructorInfo) member;
                ParameterInfo[] pars = constructorInfo.GetParameters();
                bool flag2 = false;
                if (Microsoft.JScript.CustomAttribute.IsDefined(constructorInfo, typeof(JSFunctionAttribute), false))
                {
                    flag2 = (((JSFunctionAttribute) Microsoft.JScript.CustomAttribute.GetCustomAttributes(constructorInfo, typeof(JSFunctionAttribute), false)[0]).attributeValue & JSFunctionAttributeEnum.IsInstanceNestedClassConstructor) != JSFunctionAttributeEnum.None;
                }
                if (flag2)
                {
                    PlaceArgumentsOnStack(il, pars, argList, 0, 1, ReflectionMissingCW);
                    this.TranslateToILObject(il, pars[pars.Length - 1].ParameterType, false);
                }
                else
                {
                    PlaceArgumentsOnStack(il, pars, argList, 0, 0, ReflectionMissingCW);
                }
                Type obtype = null;
                if ((member is JSConstructor) && ((obtype = ((JSConstructor) member).OuterClassType()) != null))
                {
                    this.TranslateToILObject(il, obtype, false);
                }
                bool flag3 = false;
                Type c = constructorInfo.DeclaringType;
                if (constructorInfo is JSConstructor)
                {
                    constructorInfo = ((JSConstructor) constructorInfo).GetConstructorInfo(base.compilerGlobals);
                    flag3 = true;
                }
                else
                {
                    flag3 = Typeob.INeedEngine.IsAssignableFrom(c);
                }
                il.Emit(OpCodes.Newobj, constructorInfo);
                if (flag3)
                {
                    il.Emit(OpCodes.Dup);
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Callvirt, CompilerGlobals.setEngineMethod);
                }
                Microsoft.JScript.Convert.Emit(this, il, c, rtype);
            }
            else
            {
                Type type9 = member as Type;
                if (type9 != null)
                {
                    AST ast = argList[0];
                    if (ast is NullLiteral)
                    {
                        ast.TranslateToIL(il, type9);
                        Microsoft.JScript.Convert.Emit(this, il, type9, rtype);
                    }
                    else
                    {
                        IReflect ir = ast.InferType(null);
                        if ((ir == Typeob.ScriptFunction) && Typeob.Delegate.IsAssignableFrom(type9))
                        {
                            ast.TranslateToIL(il, type9);
                        }
                        else
                        {
                            Type type10 = Microsoft.JScript.Convert.ToType(ir);
                            ast.TranslateToIL(il, type10);
                            Microsoft.JScript.Convert.Emit(this, il, type10, type9, true);
                        }
                        Microsoft.JScript.Convert.Emit(this, il, type9, rtype);
                    }
                }
                else
                {
                    if ((member is FieldInfo) && ((FieldInfo) member).IsLiteral)
                    {
                        object obj2 = (member is JSVariableField) ? ((JSVariableField) member).value : TypeReferences.GetConstantValue((FieldInfo) member);
                        if (((obj2 is Type) || (obj2 is ClassScope)) || (obj2 is TypedArray))
                        {
                            AST ast2 = argList[0];
                            if (ast2 is NullLiteral)
                            {
                                il.Emit(OpCodes.Ldnull);
                                return;
                            }
                            ClassScope scope = obj2 as ClassScope;
                            if (scope != null)
                            {
                                EnumDeclaration owner = scope.owner as EnumDeclaration;
                                if (owner != null)
                                {
                                    obj2 = owner.baseType.ToType();
                                }
                            }
                            Type type11 = Microsoft.JScript.Convert.ToType(ast2.InferType(null));
                            ast2.TranslateToIL(il, type11);
                            Type type12 = (obj2 is Type) ? ((Type) obj2) : ((obj2 is ClassScope) ? Microsoft.JScript.Convert.ToType((ClassScope) obj2) : ((TypedArray) obj2).ToType());
                            Microsoft.JScript.Convert.Emit(this, il, type11, type12, true);
                            if (!rtype.IsEnum)
                            {
                                Microsoft.JScript.Convert.Emit(this, il, type12, rtype);
                            }
                            return;
                        }
                    }
                    LocalBuilder local = null;
                    int num10 = 0;
                    int num11 = argList.count;
                    while (num10 < num11)
                    {
                        if (argList[num10] is AddressOf)
                        {
                            local = il.DeclareLocal(Typeob.ArrayOfObject);
                            break;
                        }
                        num10++;
                    }
                    object obj3 = null;
                    if ((member == null) && ((this.members == null) || (this.members.Length == 0)))
                    {
                        if (this is Lookup)
                        {
                            ((Lookup) this).TranslateToLateBinding(il);
                        }
                        else
                        {
                            obj3 = this.TranslateToSpeculativeEarlyBoundCalls(il, rtype, argList, construct, brackets);
                            ((Member) this).TranslateToLateBinding(il, obj3 != null);
                        }
                        argList.TranslateToIL(il, Typeob.ArrayOfObject);
                        if (local != null)
                        {
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Stloc, local);
                        }
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
                        il.Emit(OpCodes.Call, CompilerGlobals.callMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                        if (local != null)
                        {
                            int num12 = 0;
                            int num13 = argList.count;
                            while (num12 < num13)
                            {
                                AddressOf of = argList[num12] as AddressOf;
                                if (of != null)
                                {
                                    of.TranslateToILPreSet(il);
                                    il.Emit(OpCodes.Ldloc, local);
                                    ConstantWrapper.TranslateToILInt(il, num12);
                                    il.Emit(OpCodes.Ldelem_Ref);
                                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, Microsoft.JScript.Convert.ToType(of.InferType(null)));
                                    of.TranslateToILSet(il, null);
                                }
                                num12++;
                            }
                        }
                        if (obj3 != null)
                        {
                            il.MarkLabel((Label) obj3);
                        }
                    }
                    else
                    {
                        this.TranslateToILWithDupOfThisOb(il);
                        argList.TranslateToIL(il, Typeob.ArrayOfObject);
                        if (local != null)
                        {
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Stloc, local);
                        }
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
                        il.Emit(OpCodes.Call, CompilerGlobals.callValueMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                        if (local != null)
                        {
                            int num14 = 0;
                            int num15 = argList.count;
                            while (num14 < num15)
                            {
                                AddressOf of2 = argList[num14] as AddressOf;
                                if (of2 != null)
                                {
                                    of2.TranslateToILPreSet(il);
                                    il.Emit(OpCodes.Ldloc, local);
                                    ConstantWrapper.TranslateToILInt(il, num14);
                                    il.Emit(OpCodes.Ldelem_Ref);
                                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, Microsoft.JScript.Convert.ToType(of2.InferType(null)));
                                    of2.TranslateToILSet(il, null);
                                }
                                num14++;
                            }
                        }
                    }
                }
            }
        }

        internal override void TranslateToILDelete(ILGenerator il, Type rtype)
        {
            if (this is Lookup)
            {
                ((Lookup) this).TranslateToLateBinding(il);
            }
            else
            {
                ((Member) this).TranslateToLateBinding(il, false);
            }
            il.Emit(OpCodes.Call, CompilerGlobals.deleteMethod);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
        }

        protected abstract void TranslateToILObject(ILGenerator il, Type obtype, bool noValue);
        internal override void TranslateToILPreSet(ILGenerator il)
        {
            this.TranslateToIL(il, null, true, false);
        }

        internal override void TranslateToILPreSet(ILGenerator il, ASTList argList)
        {
            if (this.isArrayElementAccess)
            {
                this.member = this.defaultMember;
                IReflect defaultMemberReturnIR = this.defaultMemberReturnIR;
                Type rtype = (defaultMemberReturnIR is Type) ? ((Type) defaultMemberReturnIR) : Microsoft.JScript.Convert.ToType(defaultMemberReturnIR);
                this.TranslateToIL(il, rtype);
                int num = 0;
                int count = argList.count;
                while (num < count)
                {
                    argList[num].TranslateToIL(il, Typeob.Int32);
                    num++;
                }
                if (rtype.GetArrayRank() == 1)
                {
                    Type elementType = rtype.GetElementType();
                    if ((elementType.IsValueType && !elementType.IsPrimitive) && !elementType.IsEnum)
                    {
                        il.Emit(OpCodes.Ldelema, elementType);
                    }
                }
            }
            else if ((this.member is PropertyInfo) && (this.defaultMember != null))
            {
                PropertyInfo member = (PropertyInfo) this.member;
                this.member = this.defaultMember;
                this.TranslateToIL(il, Microsoft.JScript.Convert.ToType(this.defaultMemberReturnIR));
                this.member = member;
                PlaceArgumentsOnStack(il, member.GetIndexParameters(), argList, 0, 0, ReflectionMissingCW);
            }
            else
            {
                base.TranslateToILPreSet(il, argList);
            }
        }

        internal override void TranslateToILPreSetPlusGet(ILGenerator il)
        {
            this.TranslateToIL(il, Microsoft.JScript.Convert.ToType(this.InferType(null)), false, true);
        }

        internal override void TranslateToILPreSetPlusGet(ILGenerator il, ASTList argList, bool inBrackets)
        {
            if (this.isArrayElementAccess)
            {
                this.member = this.defaultMember;
                IReflect defaultMemberReturnIR = this.defaultMemberReturnIR;
                Type rtype = (defaultMemberReturnIR is Type) ? ((Type) defaultMemberReturnIR) : Microsoft.JScript.Convert.ToType(defaultMemberReturnIR);
                this.TranslateToIL(il, rtype);
                il.Emit(OpCodes.Dup);
                int arrayRank = rtype.GetArrayRank();
                LocalBuilder[] builderArray = new LocalBuilder[arrayRank];
                int index = 0;
                int count = argList.count;
                while (index < count)
                {
                    argList[index].TranslateToIL(il, Typeob.Int32);
                    builderArray[index] = il.DeclareLocal(Typeob.Int32);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc, builderArray[index]);
                    index++;
                }
                Type elementType = rtype.GetElementType();
                if (arrayRank == 1)
                {
                    TranslateToLdelem(il, elementType);
                }
                else
                {
                    Type[] types = new Type[arrayRank];
                    for (int j = 0; j < arrayRank; j++)
                    {
                        types[j] = Typeob.Int32;
                    }
                    MethodInfo method = rtype.GetMethod("Get", types);
                    il.Emit(OpCodes.Call, method);
                }
                LocalBuilder local = il.DeclareLocal(elementType);
                il.Emit(OpCodes.Stloc, local);
                for (int i = 0; i < arrayRank; i++)
                {
                    il.Emit(OpCodes.Ldloc, builderArray[i]);
                }
                if (((arrayRank == 1) && elementType.IsValueType) && !elementType.IsPrimitive)
                {
                    il.Emit(OpCodes.Ldelema, elementType);
                }
                il.Emit(OpCodes.Ldloc, local);
            }
            else
            {
                if ((this.member != null) && (this.defaultMember != null))
                {
                    this.member = this.defaultMember;
                    this.defaultMember = null;
                }
                base.TranslateToILPreSetPlusGet(il, argList, inBrackets);
            }
        }

        internal override object TranslateToILReference(ILGenerator il, Type rtype)
        {
            if (this.member is FieldInfo)
            {
                FieldInfo member = (FieldInfo) this.member;
                Type fieldType = member.FieldType;
                if (rtype == fieldType)
                {
                    bool isStatic = member.IsStatic;
                    if (!isStatic)
                    {
                        this.TranslateToILObject(il, member.DeclaringType, true);
                    }
                    object obj2 = (member is JSField) ? ((JSField) member).GetMetaData() : ((member is JSFieldInfo) ? ((JSFieldInfo) member).field : member);
                    if (obj2 is FieldInfo)
                    {
                        if (member.IsInitOnly)
                        {
                            LocalBuilder local = il.DeclareLocal(fieldType);
                            il.Emit(isStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, (FieldInfo) obj2);
                            il.Emit(OpCodes.Stloc, local);
                            il.Emit(OpCodes.Ldloca, local);
                        }
                        else
                        {
                            il.Emit(isStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, (FieldInfo) obj2);
                        }
                    }
                    else if (obj2 is LocalBuilder)
                    {
                        il.Emit(OpCodes.Ldloca, (LocalBuilder) obj2);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarga, (short) obj2);
                    }
                    return null;
                }
            }
            return base.TranslateToILReference(il, rtype);
        }

        internal override void TranslateToILSet(ILGenerator il, AST rhvalue)
        {
            if (this.isArrayElementAccess)
            {
                IReflect defaultMemberReturnIR = this.defaultMemberReturnIR;
                Type arrayClass = (defaultMemberReturnIR is Type) ? ((Type) defaultMemberReturnIR) : Microsoft.JScript.Convert.ToType(defaultMemberReturnIR);
                int arrayRank = arrayClass.GetArrayRank();
                Type elementType = arrayClass.GetElementType();
                if (rhvalue != null)
                {
                    rhvalue.TranslateToIL(il, elementType);
                }
                if (arrayRank == 1)
                {
                    TranslateToStelem(il, elementType);
                }
                else
                {
                    Type[] parameterTypes = new Type[arrayRank + 1];
                    for (int i = 0; i < arrayRank; i++)
                    {
                        parameterTypes[i] = Typeob.Int32;
                    }
                    parameterTypes[arrayRank] = elementType;
                    MethodInfo meth = base.compilerGlobals.module.GetArrayMethod(arrayClass, "Set", CallingConventions.HasThis, Typeob.Void, parameterTypes);
                    il.Emit(OpCodes.Call, meth);
                }
            }
            else if (this.isAssignmentToDefaultIndexedProperty)
            {
                if ((this.member is PropertyInfo) && (this.defaultMember != null))
                {
                    PropertyInfo member = (PropertyInfo) this.member;
                    MethodInfo setMethod = JSProperty.GetSetMethod(member, false);
                    JSWrappedMethod method = setMethod as JSWrappedMethod;
                    if ((method == null) || !(method.GetWrappedObject() is GlobalObject))
                    {
                        setMethod = this.GetMethodInfoMetadata(setMethod);
                        if (rhvalue != null)
                        {
                            rhvalue.TranslateToIL(il, member.PropertyType);
                        }
                        if ((setMethod.IsVirtual && !setMethod.IsFinal) && (!setMethod.ReflectedType.IsSealed || !setMethod.ReflectedType.IsValueType))
                        {
                            il.Emit(OpCodes.Callvirt, setMethod);
                            return;
                        }
                        il.Emit(OpCodes.Call, setMethod);
                        return;
                    }
                }
                base.TranslateToILSet(il, rhvalue);
            }
            else if (this.member is FieldInfo)
            {
                FieldInfo info4 = (FieldInfo) this.member;
                if (rhvalue != null)
                {
                    rhvalue.TranslateToIL(il, info4.FieldType);
                }
                if (info4.IsLiteral || info4.IsInitOnly)
                {
                    il.Emit(OpCodes.Pop);
                }
                else
                {
                    object obj2 = (info4 is JSField) ? ((JSField) info4).GetMetaData() : ((info4 is JSFieldInfo) ? ((JSFieldInfo) info4).field : info4);
                    FieldInfo field = obj2 as FieldInfo;
                    if (field != null)
                    {
                        il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                    }
                    else if (obj2 is LocalBuilder)
                    {
                        il.Emit(OpCodes.Stloc, (LocalBuilder) obj2);
                    }
                    else
                    {
                        il.Emit(OpCodes.Starg, (short) obj2);
                    }
                }
            }
            else if (this.member is PropertyInfo)
            {
                PropertyInfo prop = (PropertyInfo) this.member;
                if (rhvalue != null)
                {
                    rhvalue.TranslateToIL(il, prop.PropertyType);
                }
                MethodInfo methodInfoMetadata = JSProperty.GetSetMethod(prop, true);
                if (methodInfoMetadata == null)
                {
                    il.Emit(OpCodes.Pop);
                }
                else
                {
                    methodInfoMetadata = this.GetMethodInfoMetadata(methodInfoMetadata);
                    if (methodInfoMetadata.IsStatic && !(methodInfoMetadata is JSClosureMethod))
                    {
                        il.Emit(OpCodes.Call, methodInfoMetadata);
                    }
                    else if (((!this.isNonVirtual && methodInfoMetadata.IsVirtual) && !methodInfoMetadata.IsFinal) && (!methodInfoMetadata.ReflectedType.IsSealed || !methodInfoMetadata.ReflectedType.IsValueType))
                    {
                        il.Emit(OpCodes.Callvirt, methodInfoMetadata);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, methodInfoMetadata);
                    }
                }
            }
            else
            {
                object obj3 = this.TranslateToSpeculativeEarlyBoundSet(il, rhvalue);
                if (rhvalue != null)
                {
                    rhvalue.TranslateToIL(il, Typeob.Object);
                }
                il.Emit(OpCodes.Call, CompilerGlobals.setValueMethod);
                if (obj3 != null)
                {
                    il.MarkLabel((Label) obj3);
                }
            }
        }

        protected abstract void TranslateToILWithDupOfThisOb(ILGenerator il);
        private static void TranslateToLdelem(ILGenerator il, Type etype)
        {
            switch (Type.GetTypeCode(etype))
            {
                case TypeCode.Object:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    if (!etype.IsValueType)
                    {
                        il.Emit(OpCodes.Ldelem_Ref);
                        break;
                    }
                    il.Emit(OpCodes.Ldelema, etype);
                    il.Emit(OpCodes.Ldobj, etype);
                    return;

                case TypeCode.DBNull:
                case (TypeCode.DateTime | TypeCode.Object):
                    break;

                case TypeCode.Boolean:
                case TypeCode.Byte:
                    il.Emit(OpCodes.Ldelem_U1);
                    return;

                case TypeCode.Char:
                case TypeCode.UInt16:
                    il.Emit(OpCodes.Ldelem_U2);
                    return;

                case TypeCode.SByte:
                    il.Emit(OpCodes.Ldelem_I1);
                    return;

                case TypeCode.Int16:
                    il.Emit(OpCodes.Ldelem_I2);
                    return;

                case TypeCode.Int32:
                    il.Emit(OpCodes.Ldelem_I4);
                    return;

                case TypeCode.UInt32:
                    il.Emit(OpCodes.Ldelem_U4);
                    return;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Ldelem_I8);
                    return;

                case TypeCode.Single:
                    il.Emit(OpCodes.Ldelem_R4);
                    return;

                case TypeCode.Double:
                    il.Emit(OpCodes.Ldelem_R8);
                    return;

                default:
                    return;
            }
        }

        private object TranslateToSpeculativeEarlyBindings(ILGenerator il, Type rtype, bool getObjectFromLateBindingInstance)
        {
            this.giveErrors = false;
            object obj2 = null;
            bool flag = true;
            LocalBuilder local = null;
            Label label = il.DefineLabel();
            MemberInfoList allKnownInstanceBindingsForThisName = this.GetAllKnownInstanceBindingsForThisName();
            int num = 0;
            int count = allKnownInstanceBindingsForThisName.count;
            while (num < count)
            {
                MemberInfo info = allKnownInstanceBindingsForThisName[num];
                if ((info is FieldInfo) || (((info is PropertyInfo) && (((PropertyInfo) info).GetIndexParameters().Length <= 0)) && (JSProperty.GetGetMethod((PropertyInfo) info, true) != null)))
                {
                    this.member = info;
                    if (this.Accessible(false))
                    {
                        if (flag)
                        {
                            flag = false;
                            if (getObjectFromLateBindingInstance)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Ldfld, CompilerGlobals.objectField);
                            }
                            else
                            {
                                this.TranslateToILObject(il, Typeob.Object, false);
                            }
                            local = il.DeclareLocal(Typeob.Object);
                            il.Emit(OpCodes.Stloc, local);
                            obj2 = il.DefineLabel();
                        }
                        Type declaringType = info.DeclaringType;
                        il.Emit(OpCodes.Ldloc, local);
                        il.Emit(OpCodes.Isinst, declaringType);
                        LocalBuilder builder2 = il.DeclareLocal(declaringType);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Stloc, builder2);
                        il.Emit(OpCodes.Brfalse_S, label);
                        il.Emit(OpCodes.Ldloc, builder2);
                        if (info is FieldInfo)
                        {
                            FieldInfo field = (FieldInfo) info;
                            if (field.IsLiteral)
                            {
                                il.Emit(OpCodes.Pop);
                                goto Label_0265;
                            }
                            if (field is JSField)
                            {
                                il.Emit(OpCodes.Ldfld, (FieldInfo) ((JSField) field).GetMetaData());
                            }
                            else if (field is JSFieldInfo)
                            {
                                il.Emit(OpCodes.Ldfld, ((JSFieldInfo) field).field);
                            }
                            else
                            {
                                il.Emit(OpCodes.Ldfld, field);
                            }
                            Microsoft.JScript.Convert.Emit(this, il, field.FieldType, rtype);
                        }
                        else if (info is PropertyInfo)
                        {
                            MethodInfo getMethod = JSProperty.GetGetMethod((PropertyInfo) info, true);
                            getMethod = this.GetMethodInfoMetadata(getMethod);
                            if ((getMethod.IsVirtual && !getMethod.IsFinal) && (!declaringType.IsSealed || declaringType.IsValueType))
                            {
                                il.Emit(OpCodes.Callvirt, getMethod);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, getMethod);
                            }
                            Microsoft.JScript.Convert.Emit(this, il, getMethod.ReturnType, rtype);
                        }
                        il.Emit(OpCodes.Br, (Label) obj2);
                        il.MarkLabel(label);
                        label = il.DefineLabel();
                    }
                }
            Label_0265:
                num++;
            }
            il.MarkLabel(label);
            if (!flag && !getObjectFromLateBindingInstance)
            {
                il.Emit(OpCodes.Ldloc, local);
            }
            this.member = null;
            return obj2;
        }

        private object TranslateToSpeculativeEarlyBoundCalls(ILGenerator il, Type rtype, ASTList argList, bool construct, bool brackets)
        {
            this.giveErrors = false;
            object obj2 = null;
            bool flag = true;
            LocalBuilder local = null;
            Label label = il.DefineLabel();
            IReflect[] allEligibleClasses = this.GetAllEligibleClasses();
            if (!construct)
            {
                foreach (IReflect reflect in allEligibleClasses)
                {
                    MemberInfo[] member = reflect.GetMember(this.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    try
                    {
                        MethodInfo getMethod;
                        MemberInfo info2 = JSBinder.SelectCallableMember(member, this.argIRs);
                        if ((info2 != null) && (info2.MemberType == MemberTypes.Property))
                        {
                            ParameterInfo[] infoArray2;
                            getMethod = ((PropertyInfo) info2).GetGetMethod(true);
                            if (((getMethod != null) && ((infoArray2 = getMethod.GetParameters()) != null)) && (infoArray2.Length != 0))
                            {
                                goto Label_00A3;
                            }
                            continue;
                        }
                        getMethod = info2 as MethodInfo;
                    Label_00A3:
                        if ((getMethod == null) || !CheckParameters(getMethod.GetParameters(), this.argIRs, argList, base.context, 0, true, false))
                        {
                            continue;
                        }
                        if (getMethod is JSFieldMethod)
                        {
                            FunctionObject func = ((JSFieldMethod) getMethod).func;
                            if (((func == null) || ((func.attributes & MethodAttributes.NewSlot) != MethodAttributes.PrivateScope)) || !((ClassScope) reflect).ParentIsInSamePackage())
                            {
                                goto Label_014B;
                            }
                            continue;
                        }
                        if (((getMethod is JSWrappedMethod) && (((JSWrappedMethod) getMethod).obj is ClassScope)) && (((JSWrappedMethod) getMethod).GetPackage() == ((ClassScope) reflect).package))
                        {
                            continue;
                        }
                    Label_014B:
                        this.member = getMethod;
                        if (this.Accessible(false))
                        {
                            if (flag)
                            {
                                flag = false;
                                this.TranslateToILObject(il, Typeob.Object, false);
                                local = il.DeclareLocal(Typeob.Object);
                                il.Emit(OpCodes.Stloc, local);
                                obj2 = il.DefineLabel();
                            }
                            Type declaringType = getMethod.DeclaringType;
                            il.Emit(OpCodes.Ldloc, local);
                            il.Emit(OpCodes.Isinst, declaringType);
                            LocalBuilder builder2 = il.DeclareLocal(declaringType);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Stloc, builder2);
                            il.Emit(OpCodes.Brfalse, label);
                            il.Emit(OpCodes.Ldloc, builder2);
                            PlaceArgumentsOnStack(il, getMethod.GetParameters(), argList, 0, 0, ReflectionMissingCW);
                            getMethod = this.GetMethodInfoMetadata(getMethod);
                            if ((getMethod.IsVirtual && !getMethod.IsFinal) && (!declaringType.IsSealed || declaringType.IsValueType))
                            {
                                il.Emit(OpCodes.Callvirt, getMethod);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, getMethod);
                            }
                            Microsoft.JScript.Convert.Emit(this, il, getMethod.ReturnType, rtype);
                            il.Emit(OpCodes.Br, (Label) obj2);
                            il.MarkLabel(label);
                            label = il.DefineLabel();
                        }
                    }
                    catch (AmbiguousMatchException)
                    {
                    }
                }
                il.MarkLabel(label);
                if (!flag)
                {
                    il.Emit(OpCodes.Ldloc, local);
                }
                this.member = null;
            }
            return obj2;
        }

        private object TranslateToSpeculativeEarlyBoundSet(ILGenerator il, AST rhvalue)
        {
            this.giveErrors = false;
            object obj2 = null;
            bool flag = true;
            LocalBuilder local = null;
            LocalBuilder builder2 = null;
            Label label = il.DefineLabel();
            MemberInfoList allKnownInstanceBindingsForThisName = this.GetAllKnownInstanceBindingsForThisName();
            int num = 0;
            int count = allKnownInstanceBindingsForThisName.count;
            while (num < count)
            {
                MemberInfo info = allKnownInstanceBindingsForThisName[num];
                FieldInfo field = null;
                MethodInfo method = null;
                PropertyInfo prop = null;
                if (info is FieldInfo)
                {
                    field = (FieldInfo) info;
                    if (!field.IsLiteral && !field.IsInitOnly)
                    {
                        goto Label_00AA;
                    }
                    goto Label_02B4;
                }
                if (!(info is PropertyInfo))
                {
                    goto Label_02B4;
                }
                prop = (PropertyInfo) info;
                if ((prop.GetIndexParameters().Length > 0) || ((method = JSProperty.GetSetMethod(prop, true)) == null))
                {
                    goto Label_02B4;
                }
            Label_00AA:
                this.member = info;
                if (this.Accessible(true))
                {
                    if (flag)
                    {
                        flag = false;
                        if (rhvalue == null)
                        {
                            builder2 = il.DeclareLocal(Typeob.Object);
                            il.Emit(OpCodes.Stloc, builder2);
                        }
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldfld, CompilerGlobals.objectField);
                        local = il.DeclareLocal(Typeob.Object);
                        il.Emit(OpCodes.Stloc, local);
                        obj2 = il.DefineLabel();
                    }
                    Type declaringType = info.DeclaringType;
                    il.Emit(OpCodes.Ldloc, local);
                    il.Emit(OpCodes.Isinst, declaringType);
                    LocalBuilder builder3 = il.DeclareLocal(declaringType);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc, builder3);
                    il.Emit(OpCodes.Brfalse, label);
                    il.Emit(OpCodes.Ldloc, builder3);
                    if (rhvalue == null)
                    {
                        il.Emit(OpCodes.Ldloc, builder2);
                    }
                    if (field != null)
                    {
                        if (rhvalue == null)
                        {
                            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, field.FieldType);
                        }
                        else
                        {
                            rhvalue.TranslateToIL(il, field.FieldType);
                        }
                        if (field is JSField)
                        {
                            il.Emit(OpCodes.Stfld, (FieldInfo) ((JSField) field).GetMetaData());
                        }
                        else if (field is JSFieldInfo)
                        {
                            il.Emit(OpCodes.Stfld, ((JSFieldInfo) field).field);
                        }
                        else
                        {
                            il.Emit(OpCodes.Stfld, field);
                        }
                    }
                    else
                    {
                        if (rhvalue == null)
                        {
                            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, prop.PropertyType);
                        }
                        else
                        {
                            rhvalue.TranslateToIL(il, prop.PropertyType);
                        }
                        method = this.GetMethodInfoMetadata(method);
                        if ((method.IsVirtual && !method.IsFinal) && (!declaringType.IsSealed || !declaringType.IsValueType))
                        {
                            il.Emit(OpCodes.Callvirt, method);
                        }
                        else
                        {
                            il.Emit(OpCodes.Call, method);
                        }
                    }
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Br, (Label) obj2);
                    il.MarkLabel(label);
                    label = il.DefineLabel();
                }
            Label_02B4:
                num++;
            }
            if (builder2 != null)
            {
                il.Emit(OpCodes.Ldloc, builder2);
            }
            this.member = null;
            return obj2;
        }

        internal static void TranslateToStelem(ILGenerator il, Type etype)
        {
            switch (Type.GetTypeCode(etype))
            {
                case TypeCode.Object:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    if (!etype.IsValueType)
                    {
                        il.Emit(OpCodes.Stelem_Ref);
                        break;
                    }
                    il.Emit(OpCodes.Stobj, etype);
                    return;

                case TypeCode.DBNull:
                case (TypeCode.DateTime | TypeCode.Object):
                    break;

                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    il.Emit(OpCodes.Stelem_I1);
                    return;

                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    il.Emit(OpCodes.Stelem_I2);
                    return;

                case TypeCode.Int32:
                case TypeCode.UInt32:
                    il.Emit(OpCodes.Stelem_I4);
                    return;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Stelem_I8);
                    return;

                case TypeCode.Single:
                    il.Emit(OpCodes.Stelem_R4);
                    return;

                case TypeCode.Double:
                    il.Emit(OpCodes.Stelem_R8);
                    return;

                default:
                    return;
            }
        }

        private void WarnIfNotFullyResolved()
        {
            if (((!this.isFullyResolved && (this.member != null)) && (!(this.member is JSVariableField) || (((JSVariableField) this.member).type != null))) && (base.Engine.doFast || !(this.member is IWrappedMember)))
            {
                for (ScriptObject obj2 = base.Globals.ScopeStack.Peek(); obj2 != null; obj2 = obj2.GetParent())
                {
                    if ((obj2 is WithObject) && !((WithObject) obj2).isKnownAtCompileTime)
                    {
                        base.context.HandleError(JSError.AmbiguousBindingBecauseOfWith);
                        return;
                    }
                    if ((obj2 is ActivationObject) && !((ActivationObject) obj2).isKnownAtCompileTime)
                    {
                        base.context.HandleError(JSError.AmbiguousBindingBecauseOfEval);
                        return;
                    }
                }
            }
        }

        private void WarnIfObsolete()
        {
            WarnIfObsolete(this.member, base.context);
        }

        internal static void WarnIfObsolete(MemberInfo member, Context context)
        {
            string message;
            bool isError;
            if (member != null)
            {
                message = null;
                isError = false;
                object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(member, typeof(ObsoleteAttribute), false);
                if ((objArray != null) && (objArray.Length > 0))
                {
                    ObsoleteAttribute attribute = (ObsoleteAttribute) objArray[0];
                    message = attribute.Message;
                    isError = attribute.IsError;
                    goto Label_007E;
                }
                objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(member, typeof(NotRecommended), false);
                if ((objArray != null) && (objArray.Length > 0))
                {
                    NotRecommended recommended = (NotRecommended) objArray[0];
                    message = ": " + recommended.Message;
                    isError = false;
                    goto Label_007E;
                }
            }
            return;
        Label_007E:
            context.HandleError(JSError.Deprecated, message, isError);
        }
    }
}

