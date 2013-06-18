namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading;

    internal class Class : AST
    {
        internal bool allowMultiple;
        internal TypeAttributes attributes;
        private static int badTypeNameCount;
        internal Block body;
        protected ClassScope classob;
        internal CLSComplianceSpec clsCompliance;
        private Type cookedType;
        internal CustomAttributeList customAttributes;
        internal MethodBuilder deleteOpMethod;
        internal ScriptObject enclosingScope;
        private PropertyBuilder expandoItemProp;
        private MethodInfo fieldInitializer;
        protected JSMemberField[] fields;
        private SimpleHashtable firstIndex;
        private bool generateCodeForExpando;
        private MethodBuilder getHashTableMethod;
        private MethodBuilder getItem;
        private bool hasAlreadyBeenAskedAboutExpando;
        private FunctionObject implicitDefaultConstructor;
        private TypeExpression[] interfaces;
        internal bool isAbstract;
        private bool isAlreadyPartiallyEvaluated;
        private bool isCooked;
        private bool isExpando;
        internal bool isInterface;
        internal bool isStatic;
        internal string name;
        protected bool needsEngine;
        private JSVariableField ownField;
        private MethodBuilder setItem;
        private Class superClass;
        private IReflect superIR;
        private object[] superMembers;
        private TypeExpression superTypeExpression;
        internal AttributeTargets validOn;

        internal Class(Context context, AST id, TypeExpression superTypeExpression, TypeExpression[] interfaces, Block body, FieldAttributes attributes, bool isAbstract, bool isFinal, bool isStatic, bool isInterface, CustomAttributeList customAttributes) : base(context)
        {
            this.name = id.ToString();
            this.superTypeExpression = superTypeExpression;
            this.interfaces = interfaces;
            this.body = body;
            this.enclosingScope = (ScriptObject) base.Globals.ScopeStack.Peek(1);
            this.attributes = TypeAttributes.Serializable;
            this.SetAccessibility(attributes);
            if (isAbstract)
            {
                this.attributes |= TypeAttributes.Abstract;
            }
            this.isAbstract = isAbstract || isInterface;
            this.isAlreadyPartiallyEvaluated = false;
            if (isFinal)
            {
                this.attributes |= TypeAttributes.Sealed;
            }
            if (isInterface)
            {
                this.attributes |= TypeAttributes.Abstract | TypeAttributes.ClassSemanticsMask;
            }
            this.isCooked = false;
            this.cookedType = null;
            this.isExpando = false;
            this.isInterface = isInterface;
            this.isStatic = isStatic;
            this.needsEngine = !isInterface;
            this.validOn = 0;
            this.allowMultiple = true;
            this.classob = (ClassScope) base.Globals.ScopeStack.Peek();
            this.classob.name = this.name;
            this.classob.owner = this;
            this.implicitDefaultConstructor = null;
            if (!isInterface && !(this is EnumDeclaration))
            {
                this.SetupConstructors();
            }
            this.EnterNameIntoEnclosingScopeAndGetOwnField(id, isStatic);
            this.fields = this.classob.GetMemberFields();
            this.superClass = null;
            this.superIR = null;
            this.superMembers = null;
            this.firstIndex = null;
            this.fieldInitializer = null;
            this.customAttributes = customAttributes;
            this.clsCompliance = CLSComplianceSpec.NotAttributed;
            this.generateCodeForExpando = false;
            this.expandoItemProp = null;
            this.getHashTableMethod = null;
            this.getItem = null;
            this.setItem = null;
        }

        private void AddImplicitInterfaces(IReflect iface, IReflect[] explicitInterfaces, ArrayList implicitInterfaces)
        {
            Type type = iface as Type;
            if (type == null)
            {
                foreach (TypeExpression expression in ((ClassScope) iface).owner.interfaces)
                {
                    IReflect reflect = expression.ToIReflect();
                    if ((Array.IndexOf<IReflect>(explicitInterfaces, reflect, 0) >= 0) || (implicitInterfaces.IndexOf(reflect, 0) >= 0))
                    {
                        return;
                    }
                    implicitInterfaces.Add(reflect);
                }
            }
            else
            {
                foreach (Type type2 in type.GetInterfaces())
                {
                    if ((Array.IndexOf<IReflect>(explicitInterfaces, type2, 0) >= 0) || (implicitInterfaces.IndexOf(type2, 0) >= 0))
                    {
                        break;
                    }
                    implicitInterfaces.Add(type2);
                }
            }
        }

        private void AllocateImplicitDefaultConstructor()
        {
            this.implicitDefaultConstructor = new FunctionObject(".ctor", new ParameterDeclaration[0], null, new Block(base.context), new FunctionScope(this.classob, true), this.classob, base.context, MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public, null, true);
            this.implicitDefaultConstructor.isImplicitCtor = true;
            this.implicitDefaultConstructor.isConstructor = true;
            this.implicitDefaultConstructor.proto = this.classob;
        }

        private bool CanSee(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                {
                    MethodBase getMethod = JSProperty.GetGetMethod((PropertyInfo) member, true);
                    if (getMethod == null)
                    {
                        getMethod = JSProperty.GetSetMethod((PropertyInfo) member, true);
                    }
                    if (getMethod != null)
                    {
                        MethodAttributes attributes3 = getMethod.Attributes & MethodAttributes.MemberAccessMask;
                        if (((attributes3 != MethodAttributes.Private) && (attributes3 != MethodAttributes.PrivateScope)) && (attributes3 != MethodAttributes.FamANDAssem))
                        {
                            if (attributes3 == MethodAttributes.Assembly)
                            {
                                return this.IsInTheSamePackage(member);
                            }
                            return true;
                        }
                    }
                    return false;
                }
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                {
                    TypeAttributes attributes5 = ((Type) member).Attributes & TypeAttributes.NestedFamORAssem;
                    if ((attributes5 != TypeAttributes.NestedPrivate) && (attributes5 != TypeAttributes.NestedFamANDAssem))
                    {
                        if (attributes5 == TypeAttributes.NestedAssembly)
                        {
                            return this.IsInTheSamePackage(member);
                        }
                        return true;
                    }
                    return false;
                }
                case MemberTypes.Event:
                {
                    MethodBase addMethod = ((EventInfo) member).GetAddMethod();
                    if (addMethod != null)
                    {
                        switch ((addMethod.Attributes & MethodAttributes.MemberAccessMask))
                        {
                            case MethodAttributes.Private:
                            case MethodAttributes.PrivateScope:
                            case MethodAttributes.FamANDAssem:
                                return false;

                            case MethodAttributes.Assembly:
                                return this.IsInTheSamePackage(member);
                        }
                        return true;
                    }
                    return false;
                }
                case MemberTypes.Field:
                    switch ((((FieldInfo) member).Attributes & FieldAttributes.FieldAccessMask))
                    {
                        case FieldAttributes.Private:
                        case FieldAttributes.PrivateScope:
                        case FieldAttributes.FamANDAssem:
                            return false;

                        case FieldAttributes.Assembly:
                            return this.IsInTheSamePackage(member);
                    }
                    return true;

                case MemberTypes.Method:
                    switch ((((MethodBase) member).Attributes & MethodAttributes.MemberAccessMask))
                    {
                        case MethodAttributes.Private:
                        case MethodAttributes.PrivateScope:
                        case MethodAttributes.FamANDAssem:
                            return false;

                        case MethodAttributes.Assembly:
                            return this.IsInTheSamePackage(member);
                    }
                    return true;
            }
            return true;
        }

        private void CheckFieldDeclarationConsistency(JSMemberField field)
        {
            object obj2 = this.firstIndex[field.Name];
            if (obj2 != null)
            {
                int index = (int) obj2;
                int length = this.superMembers.Length;
                while (index < length)
                {
                    object obj3 = this.superMembers[index];
                    if (!(obj3 is MemberInfo))
                    {
                        return;
                    }
                    MemberInfo member = (MemberInfo) obj3;
                    if (!member.Name.Equals(field.Name))
                    {
                        return;
                    }
                    if (this.CanSee(member))
                    {
                        string fullNameFor = this.GetFullNameFor(member);
                        field.originalContext.HandleError(JSError.HidesParentMember, fullNameFor, this.IsInTheSameCompilationUnit(member));
                        return;
                    }
                    index++;
                }
            }
        }

        private void CheckIfOKToGenerateCodeForExpando(bool superClassIsExpando)
        {
            if (superClassIsExpando)
            {
                base.context.HandleError(JSError.BaseClassIsExpandoAlready);
                this.generateCodeForExpando = false;
            }
            else if (this.classob.GetMember("Item", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0)
            {
                base.context.HandleError(JSError.ItemNotAllowedOnExpandoClass);
                this.generateCodeForExpando = false;
            }
            else if ((this.classob.GetMember("get_Item", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0) || (this.classob.GetMember("set_Item", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0))
            {
                base.context.HandleError(JSError.MethodNotAllowedOnExpandoClass);
                this.generateCodeForExpando = false;
            }
            else if (this.ImplementsInterface(Typeob.IEnumerable))
            {
                base.context.HandleError(JSError.ExpandoClassShouldNotImpleEnumerable);
                this.generateCodeForExpando = false;
            }
            else if (((this.superIR.GetMember("Item", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Length > 0) || (this.superIR.GetMember("get_Item", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Length > 0)) || (this.superIR.GetMember("set_Item", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Length > 0))
            {
                base.context.HandleError(JSError.MethodClashOnExpandoSuperClass);
                this.generateCodeForExpando = false;
            }
            else
            {
                JSProperty property = this.classob.itemProp = new JSProperty("Item");
                property.getter = new JSExpandoIndexerMethod(this.classob, true);
                property.setter = new JSExpandoIndexerMethod(this.classob, false);
                this.classob.AddNewField("Item", property, FieldAttributes.Literal);
            }
        }

        private void CheckIfValidExtensionOfSuperType()
        {
            this.GetIRForSuperType();
            ClassScope superIR = this.superIR as ClassScope;
            if (superIR != null)
            {
                if (this.IsStatic)
                {
                    if (!superIR.owner.IsStatic)
                    {
                        this.superTypeExpression.context.HandleError(JSError.NestedInstanceTypeCannotBeExtendedByStatic);
                        this.superIR = Typeob.Object;
                        this.superTypeExpression = null;
                    }
                }
                else if (!superIR.owner.IsStatic && (this.enclosingScope != superIR.owner.enclosingScope))
                {
                    this.superTypeExpression.context.HandleError(JSError.NestedInstanceTypeCannotBeExtendedByStatic);
                    this.superIR = Typeob.Object;
                    this.superTypeExpression = null;
                }
            }
            this.GetSuperTypeMembers();
            this.GetStartIndexForEachName();
            bool classIsCLSCompliant = this.NeedsToBeCheckedForCLSCompliance();
            if (classIsCLSCompliant)
            {
                this.CheckMemberNamesForCLSCompliance();
            }
            int index = 0;
            int length = this.fields.Length;
            while (index < length)
            {
                JSMemberField nextOverload = this.fields[index];
                if (nextOverload.IsLiteral)
                {
                    object obj2 = nextOverload.value;
                    if (obj2 is FunctionObject)
                    {
                        while (true)
                        {
                            FunctionObject func = (FunctionObject) obj2;
                            if (func.implementedIface == null)
                            {
                                goto Label_0174;
                            }
                            this.CheckMethodDeclarationConsistency(func);
                            if (func.implementedIfaceMethod == null)
                            {
                                func.funcContext.HandleError(JSError.NoMethodInBaseToOverride);
                            }
                            if ((nextOverload.IsPublic || nextOverload.IsFamily) || nextOverload.IsFamilyOrAssembly)
                            {
                                func.CheckCLSCompliance(classIsCLSCompliant);
                            }
                            nextOverload = nextOverload.nextOverload;
                            if (nextOverload == null)
                            {
                                goto Label_0174;
                            }
                            obj2 = nextOverload.value;
                        }
                    }
                    JSProperty property1 = obj2 as JSProperty;
                }
            Label_0174:
                index++;
            }
            int num3 = 0;
            int num4 = this.fields.Length;
            while (num3 < num4)
            {
                JSMemberField field = this.fields[num3];
                if (field.IsLiteral)
                {
                    object obj4 = field.value;
                    if (obj4 is FunctionObject)
                    {
                        while (true)
                        {
                            FunctionObject obj5 = (FunctionObject) obj4;
                            if (obj5.implementedIface != null)
                            {
                                goto Label_0246;
                            }
                            this.CheckMethodDeclarationConsistency(obj5);
                            if ((field.IsPublic || field.IsFamily) || field.IsFamilyOrAssembly)
                            {
                                obj5.CheckCLSCompliance(classIsCLSCompliant);
                            }
                            field = field.nextOverload;
                            if (field == null)
                            {
                                goto Label_0246;
                            }
                            obj4 = field.value;
                        }
                    }
                    if (obj4 is JSProperty)
                    {
                        goto Label_0246;
                    }
                }
                this.CheckFieldDeclarationConsistency(field);
                if ((field.IsPublic || field.IsFamily) || field.IsFamilyOrAssembly)
                {
                    field.CheckCLSCompliance(classIsCLSCompliant);
                }
            Label_0246:
                num3++;
            }
        }

        private void CheckMatchingMethodForConsistency(MethodInfo matchingMethod, FunctionObject func, int i, int n)
        {
            IReflect reflect = func.ReturnType(null);
            IReflect reflect2 = (matchingMethod is JSFieldMethod) ? ((JSFieldMethod) matchingMethod).func.ReturnType(null) : matchingMethod.ReturnType;
            if (!reflect.Equals(reflect2))
            {
                func.funcContext.HandleError(JSError.DifferentReturnTypeFromBase, func.name, true);
            }
            else if (func.implementedIface != null)
            {
                func.implementedIfaceMethod = matchingMethod;
                this.superMembers[i] = func.name;
            }
            else
            {
                MethodAttributes attributes = func.attributes & MethodAttributes.MemberAccessMask;
                if (((matchingMethod.Attributes & MethodAttributes.MemberAccessMask) != attributes) && (((matchingMethod.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.FamORAssem) || (attributes != MethodAttributes.Family)))
                {
                    func.funcContext.HandleError(JSError.CannotChangeVisibility);
                }
                if (func.noVersionSafeAttributeSpecified)
                {
                    if (base.Engine.versionSafe)
                    {
                        if ((matchingMethod.Attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope)
                        {
                            func.funcContext.HandleError(JSError.HidesAbstractInBase, this.name + "." + func.name);
                            func.attributes &= ~MethodAttributes.NewSlot;
                        }
                        else
                        {
                            func.funcContext.HandleError(JSError.NewNotSpecifiedInMethodDeclaration, this.IsInTheSameCompilationUnit(matchingMethod));
                            i = -1;
                        }
                    }
                    else if (((matchingMethod.Attributes & MethodAttributes.Virtual) == MethodAttributes.PrivateScope) || ((matchingMethod.Attributes & MethodAttributes.Final) != MethodAttributes.PrivateScope))
                    {
                        i = -1;
                    }
                    else
                    {
                        func.attributes &= ~MethodAttributes.NewSlot;
                        if ((matchingMethod.Attributes & MethodAttributes.Abstract) == MethodAttributes.PrivateScope)
                        {
                            i = -1;
                        }
                    }
                }
                else if ((func.attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope)
                {
                    if (((matchingMethod.Attributes & MethodAttributes.Virtual) == MethodAttributes.PrivateScope) || ((matchingMethod.Attributes & MethodAttributes.Final) != MethodAttributes.PrivateScope))
                    {
                        func.funcContext.HandleError(JSError.MethodInBaseIsNotVirtual);
                        i = -1;
                    }
                    else
                    {
                        func.attributes &= ~MethodAttributes.NewSlot;
                        if ((matchingMethod.Attributes & MethodAttributes.Abstract) == MethodAttributes.PrivateScope)
                        {
                            i = -1;
                        }
                    }
                }
                else if ((matchingMethod.Attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope)
                {
                    func.funcContext.HandleError(JSError.HidesAbstractInBase, this.name + "." + func.name);
                    func.attributes &= ~MethodAttributes.NewSlot;
                }
                else
                {
                    i = -1;
                }
                if (i >= 0)
                {
                    this.superMembers[i] = func.name;
                    for (int j = i + 1; j < n; j++)
                    {
                        MemberInfo info = this.superMembers[j] as MemberInfo;
                        if (info != null)
                        {
                            if (info.Name != matchingMethod.Name)
                            {
                                return;
                            }
                            MethodInfo info2 = info as MethodInfo;
                            if (((info2 != null) && info2.IsAbstract) && ParametersMatch(info2.GetParameters(), matchingMethod.GetParameters()))
                            {
                                IReflect reflect3 = (matchingMethod is JSFieldMethod) ? ((JSFieldMethod) matchingMethod).ReturnIR() : matchingMethod.ReturnType;
                                IReflect reflect4 = (info2 is JSFieldMethod) ? ((JSFieldMethod) info2).ReturnIR() : info2.ReturnType;
                                if (reflect3 == reflect4)
                                {
                                    this.superMembers[j] = func.name;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void CheckMemberNamesForCLSCompliance()
        {
            if (!(this.enclosingScope is ClassScope))
            {
                base.Engine.CheckTypeNameForCLSCompliance(this.name, this.GetFullName(), base.context);
            }
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            int index = 0;
            int length = this.fields.Length;
            while (index < length)
            {
                JSMemberField field = this.fields[index];
                if (!field.IsPrivate)
                {
                    if (!VsaEngine.CheckIdentifierForCLSCompliance(field.Name))
                    {
                        field.originalContext.HandleError(JSError.NonCLSCompliantMember);
                    }
                    else if (((JSMemberField) hashtable[field.Name]) == null)
                    {
                        hashtable.Add(field.Name, field);
                    }
                    else
                    {
                        field.originalContext.HandleError(JSError.NonCLSCompliantMember);
                    }
                }
                index++;
            }
        }

        private void CheckMethodDeclarationConsistency(FunctionObject func)
        {
            if ((!func.isStatic || func.isExpandoMethod) && !func.isConstructor)
            {
                object obj2 = this.firstIndex[func.name];
                if (obj2 == null)
                {
                    this.CheckThatMethodIsNotMarkedWithOverrideOrHide(func);
                    if ((func.attributes & MethodAttributes.Final) != MethodAttributes.PrivateScope)
                    {
                        func.attributes &= ~(MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final);
                    }
                }
                else
                {
                    MemberInfo supMem = null;
                    int index = (int) obj2;
                    int length = this.superMembers.Length;
                    while (index < length)
                    {
                        MemberInfo member = this.superMembers[index] as MemberInfo;
                        if (member == null)
                        {
                            goto Label_015E;
                        }
                        if (!member.Name.Equals(func.name))
                        {
                            break;
                        }
                        if (!this.CanSee(member))
                        {
                            goto Label_015E;
                        }
                        if (member.MemberType != MemberTypes.Method)
                        {
                            supMem = member;
                            goto Label_015E;
                        }
                        if (func.isExpandoMethod)
                        {
                            supMem = member;
                            break;
                        }
                        MethodInfo matchingMethod = (MethodInfo) member;
                        if (func.implementedIface != null)
                        {
                            if (matchingMethod is JSFieldMethod)
                            {
                                if (((JSFieldMethod) matchingMethod).EnclosingScope() == func.implementedIface)
                                {
                                    goto Label_010C;
                                }
                                goto Label_015E;
                            }
                            if (matchingMethod.DeclaringType != func.implementedIface)
                            {
                                goto Label_015E;
                            }
                        }
                    Label_010C:
                        if (ParametersMatch(matchingMethod.GetParameters(), func.parameter_declarations))
                        {
                            if (matchingMethod is JSWrappedMethod)
                            {
                                matchingMethod = ((JSWrappedMethod) matchingMethod).method;
                            }
                            if (func.noVersionSafeAttributeSpecified || ((func.attributes & MethodAttributes.NewSlot) != MethodAttributes.NewSlot))
                            {
                                this.CheckMatchingMethodForConsistency(matchingMethod, func, index, length);
                            }
                            return;
                        }
                    Label_015E:
                        index++;
                    }
                    if (supMem != null)
                    {
                        if (func.noVersionSafeAttributeSpecified || (((func.attributes & MethodAttributes.NewSlot) != MethodAttributes.NewSlot) && !func.isExpandoMethod))
                        {
                            string fullNameFor = this.GetFullNameFor(supMem);
                            func.funcContext.HandleError(JSError.HidesParentMember, fullNameFor, this.IsInTheSameCompilationUnit(supMem));
                        }
                    }
                    else
                    {
                        this.CheckThatMethodIsNotMarkedWithOverrideOrHide(func);
                        if ((func.attributes & MethodAttributes.Final) != MethodAttributes.PrivateScope)
                        {
                            func.attributes &= ~(MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final);
                        }
                    }
                }
            }
        }

        private void CheckThatAllAbstractSuperClassMethodsAreImplemented()
        {
            int index = 0;
            int length = this.superMembers.Length;
            while (index < length)
            {
                object obj2 = this.superMembers[index];
                MethodInfo meth = obj2 as MethodInfo;
                if ((meth != null) && meth.IsAbstract)
                {
                    for (int i = index - 1; i >= 0; i--)
                    {
                        object obj3 = this.superMembers[i];
                        if (obj3 is MethodInfo)
                        {
                            MethodInfo info2 = (MethodInfo) obj3;
                            if (info2.Name != meth.Name)
                            {
                                break;
                            }
                            if (!info2.IsAbstract && ParametersMatch(info2.GetParameters(), meth.GetParameters()))
                            {
                                IReflect reflect = (meth is JSFieldMethod) ? ((JSFieldMethod) meth).ReturnIR() : meth.ReturnType;
                                IReflect reflect2 = (info2 is JSFieldMethod) ? ((JSFieldMethod) info2).ReturnIR() : info2.ReturnType;
                                if (reflect == reflect2)
                                {
                                    this.superMembers[index] = meth.Name;
                                    goto Label_01FE;
                                }
                            }
                        }
                    }
                    if (!this.isAbstract || (!this.isInterface && DefinedOnInterface(meth)))
                    {
                        StringBuilder builder = new StringBuilder(meth.DeclaringType.ToString());
                        builder.Append('.');
                        builder.Append(meth.Name);
                        builder.Append('(');
                        ParameterInfo[] parameters = meth.GetParameters();
                        int num4 = 0;
                        int num5 = parameters.Length;
                        while (num4 < num5)
                        {
                            builder.Append(parameters[num4].ParameterType.FullName);
                            if (num4 < (num5 - 1))
                            {
                                builder.Append(", ");
                            }
                            num4++;
                        }
                        builder.Append(")");
                        if (meth.ReturnType != Typeob.Void)
                        {
                            builder.Append(" : ");
                            builder.Append(meth.ReturnType.FullName);
                        }
                        base.context.HandleError(JSError.MustImplementMethod, builder.ToString());
                        this.attributes |= TypeAttributes.Abstract;
                    }
                }
            Label_01FE:
                index++;
            }
        }

        private void CheckThatMethodIsNotMarkedWithOverrideOrHide(FunctionObject func)
        {
            if (!func.noVersionSafeAttributeSpecified)
            {
                if ((func.attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope)
                {
                    func.funcContext.HandleError(JSError.NoMethodInBaseToOverride);
                }
                else
                {
                    func.funcContext.HandleError(JSError.NoMethodInBaseToNew);
                }
            }
        }

        private static bool DefinedOnInterface(MethodInfo meth)
        {
            JSFieldMethod method = meth as JSFieldMethod;
            if (method != null)
            {
                return ((ClassScope) method.func.enclosing_scope).owner.isInterface;
            }
            return meth.DeclaringType.IsInterface;
        }

        private void EmitILForINeedEngineMethods()
        {
            if (this.needsEngine)
            {
                TypeBuilder classwriter = (TypeBuilder) this.classob.classwriter;
                FieldBuilder field = classwriter.DefineField("vsa Engine", Typeob.VsaEngine, FieldAttributes.NotSerialized | FieldAttributes.Private);
                MethodBuilder methodInfoBody = classwriter.DefineMethod("GetEngine", MethodAttributes.Virtual | MethodAttributes.Private, Typeob.VsaEngine, null);
                ILGenerator iLGenerator = methodInfoBody.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, field);
                iLGenerator.Emit(OpCodes.Ldnull);
                Label label = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Bne_Un_S, label);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                if (this.body.Engine.doCRS)
                {
                    iLGenerator.Emit(OpCodes.Ldsfld, CompilerGlobals.contextEngineField);
                }
                else if (base.context.document.engine.PEFileKind == PEFileKinds.Dll)
                {
                    iLGenerator.Emit(OpCodes.Ldtoken, classwriter);
                    iLGenerator.Emit(OpCodes.Call, CompilerGlobals.createVsaEngineWithType);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Call, CompilerGlobals.createVsaEngine);
                }
                iLGenerator.Emit(OpCodes.Stfld, field);
                iLGenerator.MarkLabel(label);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, field);
                iLGenerator.Emit(OpCodes.Ret);
                classwriter.DefineMethodOverride(methodInfoBody, CompilerGlobals.getEngineMethod);
                MethodBuilder builder4 = classwriter.DefineMethod("SetEngine", MethodAttributes.Virtual | MethodAttributes.Private, Typeob.Void, new Type[] { Typeob.VsaEngine });
                iLGenerator = builder4.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Stfld, field);
                iLGenerator.Emit(OpCodes.Ret);
                classwriter.DefineMethodOverride(builder4, CompilerGlobals.setEngineMethod);
            }
        }

        internal void EmitInitialCalls(ILGenerator il, MethodBase supcons, ParameterInfo[] pars, ASTList argAST, int callerParameterCount)
        {
            bool flag = true;
            if (supcons != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                int length = pars.Length;
                int num2 = (argAST == null) ? 0 : argAST.count;
                object[] objArray = new object[length];
                for (int i = 0; i < length; i++)
                {
                    AST ast = (i < num2) ? argAST[i] : new ConstantWrapper(null, null);
                    if (pars[i].ParameterType.IsByRef)
                    {
                        objArray[i] = ast.TranslateToILReference(il, pars[i].ParameterType.GetElementType());
                    }
                    else
                    {
                        ast.TranslateToIL(il, pars[i].ParameterType);
                        objArray[i] = null;
                    }
                }
                if (supcons is JSConstructor)
                {
                    JSConstructor constructor = (JSConstructor) supcons;
                    flag = constructor.GetClassScope() != this.classob;
                    supcons = constructor.GetConstructorInfo(base.compilerGlobals);
                    if (constructor.GetClassScope().outerClassField != null)
                    {
                        Microsoft.JScript.Convert.EmitLdarg(il, (short) callerParameterCount);
                    }
                }
                il.Emit(OpCodes.Call, (ConstructorInfo) supcons);
                for (int j = 0; j < num2; j++)
                {
                    AST ast2 = argAST[j];
                    if ((ast2 is AddressOf) && (objArray[j] != null))
                    {
                        Type type = Microsoft.JScript.Convert.ToType(ast2.InferType(null));
                        ast2.TranslateToILPreSet(il);
                        il.Emit(OpCodes.Ldloc, (LocalBuilder) objArray[j]);
                        Microsoft.JScript.Convert.Emit(this, il, pars[j].ParameterType, type);
                        ast2.TranslateToILSet(il);
                    }
                }
            }
            if (this.classob.outerClassField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                Microsoft.JScript.Convert.EmitLdarg(il, (short) callerParameterCount);
                il.Emit(OpCodes.Stfld, this.classob.outerClassField);
            }
            if (flag)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, this.fieldInitializer);
                this.body.TranslateToILInitOnlyInitializers(il);
            }
        }

        private void EmitUsingNamespaces(ILGenerator il)
        {
            if (this.body.Engine.GenerateDebugInfo)
            {
                for (ScriptObject obj2 = this.enclosingScope; obj2 != null; obj2 = obj2.GetParent())
                {
                    if (obj2 is PackageScope)
                    {
                        il.UsingNamespace(((PackageScope) obj2).name);
                    }
                    else if ((obj2 is WrappedNamespace) && !((WrappedNamespace) obj2).name.Equals(""))
                    {
                        il.UsingNamespace(((WrappedNamespace) obj2).name);
                    }
                }
            }
        }

        private void EnterNameIntoEnclosingScopeAndGetOwnField(AST id, bool isStatic)
        {
            if (((IActivationObject) this.enclosingScope).GetLocalField(this.name) != null)
            {
                id.context.HandleError(JSError.DuplicateName, true);
                this.name = this.name + " class";
            }
            FieldAttributes literal = FieldAttributes.Literal;
            switch ((this.attributes & TypeAttributes.NestedFamORAssem))
            {
                case TypeAttributes.NestedPrivate:
                    literal |= FieldAttributes.Private;
                    break;

                case TypeAttributes.NestedFamily:
                    literal |= FieldAttributes.Family;
                    break;

                case TypeAttributes.NestedAssembly:
                    literal |= FieldAttributes.Assembly;
                    break;

                case TypeAttributes.NestedFamANDAssem:
                    literal |= FieldAttributes.FamANDAssem;
                    break;

                case TypeAttributes.NestedFamORAssem:
                    literal |= FieldAttributes.FamORAssem;
                    break;

                default:
                    literal |= FieldAttributes.Public;
                    break;
            }
            ScriptObject enclosingScope = this.enclosingScope;
            while (enclosingScope is BlockScope)
            {
                enclosingScope = enclosingScope.GetParent();
            }
            if ((!(enclosingScope is GlobalScope) && !(enclosingScope is PackageScope)) && !(enclosingScope is ClassScope))
            {
                isStatic = false;
                if (this is EnumDeclaration)
                {
                    base.context.HandleError(JSError.EnumNotAllowed);
                }
                else
                {
                    base.context.HandleError(JSError.ClassNotAllowed);
                }
            }
            if (isStatic)
            {
                literal |= FieldAttributes.Static;
            }
            if (this.enclosingScope is ActivationObject)
            {
                if ((this.enclosingScope is ClassScope) && (this.name == ((ClassScope) this.enclosingScope).name))
                {
                    base.context.HandleError(JSError.CannotUseNameOfClass);
                    this.name = this.name + " nested class";
                }
                this.ownField = ((ActivationObject) this.enclosingScope).AddNewField(this.name, this.classob, literal);
                if (this.ownField is JSLocalField)
                {
                    ((JSLocalField) this.ownField).isDefined = true;
                }
            }
            else
            {
                this.ownField = ((StackFrame) this.enclosingScope).AddNewField(this.name, this.classob, literal);
            }
            this.ownField.originalContext = id.context;
        }

        internal override object Evaluate()
        {
            base.Globals.ScopeStack.GuardedPush(this.classob);
            try
            {
                this.body.EvaluateStaticVariableInitializers();
            }
            finally
            {
                base.Globals.ScopeStack.Pop();
            }
            return new Completion();
        }

        private void GenerateGetEnumerator()
        {
            TypeBuilder typeBuilder = this.classob.GetTypeBuilder();
            MethodBuilder methodInfoBody = typeBuilder.DefineMethod("get enumerator", MethodAttributes.Virtual | MethodAttributes.Private, Typeob.IEnumerator, null);
            ILGenerator iLGenerator = methodInfoBody.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, this.getHashTableMethod);
            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.hashTableGetEnumerator);
            iLGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(methodInfoBody, CompilerGlobals.getEnumeratorMethod);
        }

        private void GetExpandoDeleteMethod()
        {
            TypeBuilder typeBuilder = this.classob.GetTypeBuilder();
            MethodBuilder builder2 = this.deleteOpMethod = typeBuilder.DefineMethod("op_Delete", MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.Public, Typeob.Boolean, new Type[] { typeBuilder, Typeob.ArrayOfObject });
            builder2.DefineParameter(2, ParameterAttributes.None, null).SetCustomAttribute(new CustomAttributeBuilder(Typeob.ParamArrayAttribute.GetConstructor(Type.EmptyTypes), new object[0]));
            ILGenerator iLGenerator = builder2.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, this.getHashTableMethod);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Dup);
            iLGenerator.Emit(OpCodes.Ldlen);
            iLGenerator.Emit(OpCodes.Ldc_I4_1);
            iLGenerator.Emit(OpCodes.Sub);
            iLGenerator.Emit(OpCodes.Ldelem_Ref);
            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.hashtableRemove);
            iLGenerator.Emit(OpCodes.Ldc_I4_1);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void GetExpandoFieldGetter(TypeBuilder classwriter)
        {
            if (this.expandoItemProp == null)
            {
                this.expandoItemProp = classwriter.DefineProperty("Item", PropertyAttributes.None, Typeob.Object, new Type[] { Typeob.String });
                FieldInfo field = classwriter.DefineField("expando table", Typeob.SimpleHashtable, FieldAttributes.Private);
                this.getHashTableMethod = classwriter.DefineMethod("get expando table", MethodAttributes.Private, Typeob.SimpleHashtable, null);
                ILGenerator iLGenerator = this.getHashTableMethod.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, field);
                iLGenerator.Emit(OpCodes.Ldnull);
                Label label = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Bne_Un_S, label);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldc_I4_8);
                iLGenerator.Emit(OpCodes.Newobj, CompilerGlobals.hashtableCtor);
                iLGenerator.Emit(OpCodes.Stfld, field);
                iLGenerator.MarkLabel(label);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, field);
                iLGenerator.Emit(OpCodes.Ret);
            }
        }

        internal MethodInfo GetExpandoIndexerGetter()
        {
            if (this.getItem == null)
            {
                TypeBuilder typeBuilder = this.classob.GetTypeBuilder();
                this.GetExpandoFieldGetter(typeBuilder);
                this.getItem = typeBuilder.DefineMethod("get_Item", MethodAttributes.SpecialName | MethodAttributes.Public, Typeob.Object, new Type[] { Typeob.String });
                ILGenerator iLGenerator = this.getItem.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Call, this.getHashTableMethod);
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.hashtableGetItem);
                iLGenerator.Emit(OpCodes.Dup);
                Label label = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Brtrue_S, label);
                iLGenerator.Emit(OpCodes.Pop);
                iLGenerator.Emit(OpCodes.Ldsfld, CompilerGlobals.missingField);
                iLGenerator.MarkLabel(label);
                iLGenerator.Emit(OpCodes.Ret);
                this.expandoItemProp.SetGetMethod(this.getItem);
            }
            return this.getItem;
        }

        internal MethodInfo GetExpandoIndexerSetter()
        {
            if (this.setItem == null)
            {
                TypeBuilder typeBuilder = this.classob.GetTypeBuilder();
                this.GetExpandoFieldGetter(typeBuilder);
                this.setItem = typeBuilder.DefineMethod("set_Item", MethodAttributes.SpecialName | MethodAttributes.Public, Typeob.Void, new Type[] { Typeob.String, Typeob.Object });
                ILGenerator iLGenerator = this.setItem.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Call, this.getHashTableMethod);
                iLGenerator.Emit(OpCodes.Ldarg_2);
                iLGenerator.Emit(OpCodes.Ldsfld, CompilerGlobals.missingField);
                Label label = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Beq_S, label);
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Ldarg_2);
                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.hashtableSetItem);
                iLGenerator.Emit(OpCodes.Ret);
                iLGenerator.MarkLabel(label);
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.hashtableRemove);
                iLGenerator.Emit(OpCodes.Ret);
                this.expandoItemProp.SetSetMethod(this.setItem);
            }
            return this.setItem;
        }

        internal override Context GetFirstExecutableContext()
        {
            return null;
        }

        private string GetFullName()
        {
            string name = ((ActivationObject) this.enclosingScope).GetName();
            if (name == null)
            {
                VsaEngine engine = base.context.document.engine;
                if ((engine != null) && engine.genStartupClass)
                {
                    name = engine.RootNamespace;
                }
            }
            if (name != null)
            {
                return (name + "." + this.name);
            }
            return this.name;
        }

        private string GetFullNameFor(MemberInfo supMem)
        {
            string classFullName;
            if (supMem is JSField)
            {
                classFullName = ((JSField) supMem).GetClassFullName();
            }
            else if (supMem is JSConstructor)
            {
                classFullName = ((JSConstructor) supMem).GetClassFullName();
            }
            else if (supMem is JSMethod)
            {
                classFullName = ((JSMethod) supMem).GetClassFullName();
            }
            else if (supMem is JSProperty)
            {
                classFullName = ((JSProperty) supMem).GetClassFullName();
            }
            else if (supMem is JSWrappedProperty)
            {
                classFullName = ((JSWrappedProperty) supMem).GetClassFullName();
            }
            else
            {
                classFullName = supMem.DeclaringType.FullName;
            }
            return (classFullName + "." + supMem.Name);
        }

        internal MemberInfo[] GetInterfaceMember(string name)
        {
            MemberInfo[] member;
            this.PartiallyEvaluate();
            if (this.isInterface)
            {
                member = this.classob.GetMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if ((member != null) && (member.Length > 0))
                {
                    return member;
                }
            }
            foreach (TypeExpression expression in this.interfaces)
            {
                member = expression.ToIReflect().GetMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if ((member != null) && (member.Length > 0))
                {
                    return member;
                }
            }
            return new MemberInfo[0];
        }

        private void GetIRForSuperType()
        {
            IReflect reflect = this.superIR = Typeob.Object;
            if (this.superTypeExpression != null)
            {
                this.superTypeExpression.PartiallyEvaluate();
                reflect = this.superIR = this.superTypeExpression.ToIReflect();
            }
            Type c = reflect as Type;
            if (c != null)
            {
                if ((c.IsSealed || c.IsInterface) || ((c == Typeob.ValueType) || (c == Typeob.ArrayObject)))
                {
                    if (this.superTypeExpression.Evaluate() is Namespace)
                    {
                        this.superTypeExpression.context.HandleError(JSError.NeedType);
                    }
                    else
                    {
                        this.superTypeExpression.context.HandleError(JSError.TypeCannotBeExtended, c.FullName);
                    }
                    this.superTypeExpression = null;
                    this.superIR = Typeob.Object;
                }
                else if (Typeob.INeedEngine.IsAssignableFrom(c))
                {
                    this.needsEngine = false;
                }
            }
            else if (reflect is ClassScope)
            {
                if (((ClassScope) reflect).owner.IsASubClassOf(this))
                {
                    this.superTypeExpression.context.HandleError(JSError.CircularDefinition);
                    this.superTypeExpression = null;
                    this.superIR = Typeob.Object;
                }
                else
                {
                    this.needsEngine = false;
                    this.superClass = ((ClassScope) reflect).owner;
                    if ((this.superClass.attributes & TypeAttributes.Sealed) != TypeAttributes.AnsiClass)
                    {
                        this.superTypeExpression.context.HandleError(JSError.TypeCannotBeExtended, this.superClass.name);
                        this.superClass.attributes &= ~TypeAttributes.Sealed;
                        this.superTypeExpression = null;
                    }
                    else if (this.superClass.isInterface)
                    {
                        this.superTypeExpression.context.HandleError(JSError.TypeCannotBeExtended, this.superClass.name);
                        this.superIR = Typeob.Object;
                        this.superTypeExpression = null;
                    }
                }
            }
            else
            {
                this.superTypeExpression.context.HandleError(JSError.TypeCannotBeExtended);
                this.superIR = Typeob.Object;
                this.superTypeExpression = null;
            }
        }

        private void GetStartIndexForEachName()
        {
            SimpleHashtable hashtable = new SimpleHashtable(0x20);
            string str = null;
            int index = 0;
            int length = this.superMembers.Length;
            while (index < length)
            {
                string name = ((MemberInfo) this.superMembers[index]).Name;
                if (name != str)
                {
                    hashtable[str = name] = index;
                }
                index++;
            }
            this.firstIndex = hashtable;
        }

        internal ConstructorInfo GetSuperConstructor(IReflect[] argIRs)
        {
            object obj2 = null;
            if (this.superTypeExpression != null)
            {
                obj2 = this.superTypeExpression.Evaluate();
            }
            else
            {
                obj2 = Typeob.Object;
            }
            if (obj2 is ClassScope)
            {
                return JSBinder.SelectConstructor(((ClassScope) obj2).constructors, argIRs);
            }
            return JSBinder.SelectConstructor(((Type) obj2).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance), argIRs);
        }

        private void GetSuperTypeMembers()
        {
            SuperTypeMembersSorter sorter = new SuperTypeMembersSorter();
            IReflect superIR = this.superIR;
            while (superIR != null)
            {
                sorter.Add(superIR.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                if (superIR is Type)
                {
                    superIR = ((Type) superIR).BaseType;
                }
                else
                {
                    superIR = ((ClassScope) superIR).GetSuperType();
                }
            }
            ArrayList implicitInterfaces = new ArrayList();
            int length = this.interfaces.Length;
            IReflect[] explicitInterfaces = new IReflect[length];
            for (int i = 0; i < length; i++)
            {
                bool isInterface;
                IReflect reflect2 = explicitInterfaces[i] = this.interfaces[i].ToIReflect();
                Type type = reflect2 as Type;
                if (type != null)
                {
                    isInterface = type.IsInterface;
                }
                else
                {
                    ClassScope scope = (ClassScope) reflect2;
                    isInterface = scope.owner.isInterface;
                }
                if (!isInterface)
                {
                    this.interfaces[i].context.HandleError(JSError.NeedInterface);
                }
            }
            foreach (IReflect reflect3 in explicitInterfaces)
            {
                this.AddImplicitInterfaces(reflect3, explicitInterfaces, implicitInterfaces);
            }
            for (int j = 0; j < implicitInterfaces.Count; j++)
            {
                IReflect iface = (IReflect) implicitInterfaces[j];
                this.AddImplicitInterfaces(iface, explicitInterfaces, implicitInterfaces);
            }
            int count = implicitInterfaces.Count;
            if (count > 0)
            {
                TypeExpression[] expressionArray = new TypeExpression[length + count];
                for (int k = 0; k < length; k++)
                {
                    expressionArray[k] = this.interfaces[k];
                }
                for (int m = 0; m < count; m++)
                {
                    expressionArray[m + length] = new TypeExpression(new ConstantWrapper(implicitInterfaces[m], null));
                }
                this.interfaces = expressionArray;
            }
            foreach (TypeExpression expression in this.interfaces)
            {
                ClassScope scope2 = expression.ToIReflect() as ClassScope;
                if ((scope2 != null) && scope2.owner.ImplementsInterface(this.classob))
                {
                    base.context.HandleError(JSError.CircularDefinition);
                    this.interfaces = new TypeExpression[0];
                    break;
                }
                sorter.Add(expression.ToIReflect().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            }
            superIR = this.superIR;
            while (superIR != null)
            {
                Type type2 = superIR as Type;
                if (type2 != null)
                {
                    if (!type2.IsAbstract)
                    {
                        break;
                    }
                    GetUnimplementedInferfaceMembersFor(type2, sorter);
                    superIR = type2.BaseType;
                }
                else
                {
                    ClassScope scope3 = (ClassScope) superIR;
                    if (!scope3.owner.isAbstract)
                    {
                        break;
                    }
                    scope3.owner.GetUnimplementedInferfaceMembers(sorter);
                    superIR = null;
                }
            }
            this.superMembers = sorter.GetMembers();
        }

        internal TypeBuilder GetTypeBuilder()
        {
            return (TypeBuilder) this.GetTypeBuilderOrEnumBuilder();
        }

        internal virtual Type GetTypeBuilderOrEnumBuilder()
        {
            TypeBuilder classwriter;
            if (this.classob.classwriter != null)
            {
                return this.classob.classwriter;
            }
            if (!this.isAlreadyPartiallyEvaluated)
            {
                this.PartiallyEvaluate();
            }
            Type parent = null;
            if (this.superTypeExpression != null)
            {
                parent = this.superTypeExpression.ToType();
            }
            else
            {
                parent = this.isInterface ? null : Typeob.Object;
            }
            int num = (this.needsEngine ? 1 : 0) + (this.generateCodeForExpando ? 1 : 0);
            int num2 = this.interfaces.Length + num;
            Type[] interfaces = new Type[num2];
            for (int i = num; i < num2; i++)
            {
                interfaces[i] = this.interfaces[i - num].ToType();
            }
            if (this.needsEngine)
            {
                interfaces[--num] = Typeob.INeedEngine;
            }
            if (this.generateCodeForExpando)
            {
                interfaces[--num] = Typeob.IEnumerable;
            }
            if (this.enclosingScope is ClassScope)
            {
                classwriter = (TypeBuilder) this.classob.classwriter;
                if (classwriter == null)
                {
                    TypeBuilder typeBuilder = ((ClassScope) this.enclosingScope).owner.GetTypeBuilder();
                    if (this.classob.classwriter != null)
                    {
                        return this.classob.classwriter;
                    }
                    classwriter = typeBuilder.DefineNestedType(this.name, this.attributes, parent, interfaces);
                    this.classob.classwriter = classwriter;
                    if (!this.isStatic && !this.isInterface)
                    {
                        this.classob.outerClassField = classwriter.DefineField("outer class instance", typeBuilder, FieldAttributes.Private);
                    }
                }
            }
            else
            {
                string name = ((ActivationObject) this.enclosingScope).GetName();
                if (name == null)
                {
                    VsaEngine engine = base.context.document.engine;
                    if ((engine != null) && engine.genStartupClass)
                    {
                        name = engine.RootNamespace;
                    }
                }
                classwriter = (TypeBuilder) this.classob.classwriter;
                if (classwriter == null)
                {
                    string message = this.name;
                    if (name != null)
                    {
                        message = name + "." + message;
                    }
                    if (message.Length >= 0x400)
                    {
                        base.context.HandleError(JSError.TypeNameTooLong, message);
                        message = "bad type name " + badTypeNameCount.ToString(CultureInfo.InvariantCulture);
                        badTypeNameCount++;
                    }
                    classwriter = base.compilerGlobals.module.DefineType(message, this.attributes, parent, interfaces);
                    this.classob.classwriter = classwriter;
                }
            }
            if (this.customAttributes != null)
            {
                CustomAttributeBuilder[] customAttributeBuilders = this.customAttributes.GetCustomAttributeBuilders(false);
                for (int j = 0; j < customAttributeBuilders.Length; j++)
                {
                    classwriter.SetCustomAttribute(customAttributeBuilders[j]);
                }
            }
            if (this.clsCompliance == CLSComplianceSpec.CLSCompliant)
            {
                classwriter.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { true }));
            }
            else if (this.clsCompliance == CLSComplianceSpec.NonCLSCompliant)
            {
                classwriter.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { false }));
            }
            if (this.generateCodeForExpando)
            {
                classwriter.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.defaultMemberAttributeCtor, new object[] { "Item" }));
            }
            int index = 0;
            int length = this.fields.Length;
            while (index < length)
            {
                JSMemberField nextOverload = this.fields[index];
                if (nextOverload.IsLiteral)
                {
                    object ob = nextOverload.value;
                    if (ob is JSProperty)
                    {
                        JSProperty property = (JSProperty) ob;
                        ParameterInfo[] indexParameters = property.GetIndexParameters();
                        int num7 = indexParameters.Length;
                        Type[] parameterTypes = new Type[num7];
                        for (int k = 0; k < num7; k++)
                        {
                            parameterTypes[k] = indexParameters[k].ParameterType;
                        }
                        PropertyBuilder builder3 = property.metaData = classwriter.DefineProperty(nextOverload.Name, property.Attributes, property.PropertyType, parameterTypes);
                        if (property.getter != null)
                        {
                            CustomAttributeList customAttributes = ((JSFieldMethod) property.getter).func.customAttributes;
                            if (customAttributes != null)
                            {
                                foreach (CustomAttributeBuilder builder4 in customAttributes.GetCustomAttributeBuilders(true))
                                {
                                    builder3.SetCustomAttribute(builder4);
                                }
                            }
                            builder3.SetGetMethod((MethodBuilder) property.getter.GetMethodInfo(base.compilerGlobals));
                        }
                        if (property.setter != null)
                        {
                            CustomAttributeList list2 = ((JSFieldMethod) property.setter).func.customAttributes;
                            if (list2 != null)
                            {
                                foreach (CustomAttributeBuilder builder5 in list2.GetCustomAttributeBuilders(true))
                                {
                                    builder3.SetCustomAttribute(builder5);
                                }
                            }
                            builder3.SetSetMethod((MethodBuilder) property.setter.GetMethodInfo(base.compilerGlobals));
                        }
                        goto Label_0603;
                    }
                    if (ob is ClassScope)
                    {
                        ((ClassScope) ob).GetTypeBuilderOrEnumBuilder();
                        goto Label_0603;
                    }
                    if (Microsoft.JScript.Convert.GetTypeCode(ob) != TypeCode.Object)
                    {
                        FieldBuilder builder6 = classwriter.DefineField(nextOverload.Name, nextOverload.FieldType, nextOverload.Attributes);
                        builder6.SetConstant(nextOverload.value);
                        nextOverload.metaData = builder6;
                        nextOverload.WriteCustomAttribute(base.Engine.doCRS);
                        goto Label_0603;
                    }
                    if (!(ob is FunctionObject))
                    {
                        goto Label_0603;
                    }
                    FunctionObject obj3 = (FunctionObject) ob;
                    if (obj3.isExpandoMethod)
                    {
                        nextOverload.metaData = classwriter.DefineField(nextOverload.Name, Typeob.ScriptFunction, nextOverload.Attributes & ~(FieldAttributes.Literal | FieldAttributes.Static));
                        obj3.isStatic = false;
                    }
                    if (!this.isInterface)
                    {
                        goto Label_0603;
                    }
                    while (true)
                    {
                        obj3.GetMethodInfo(base.compilerGlobals);
                        nextOverload = nextOverload.nextOverload;
                        if (nextOverload == null)
                        {
                            goto Label_0603;
                        }
                        obj3 = (FunctionObject) nextOverload.value;
                    }
                }
                nextOverload.metaData = classwriter.DefineField(nextOverload.Name, nextOverload.FieldType, nextOverload.Attributes);
                nextOverload.WriteCustomAttribute(base.Engine.doCRS);
            Label_0603:
                index++;
            }
            return classwriter;
        }

        private void GetUnimplementedInferfaceMembers(SuperTypeMembersSorter sorter)
        {
            int index = 0;
            int length = this.superMembers.Length;
            while (index < length)
            {
                MethodInfo member = this.superMembers[index] as MethodInfo;
                if ((member != null) && member.DeclaringType.IsInterface)
                {
                    sorter.Add(member);
                }
                index++;
            }
        }

        private static void GetUnimplementedInferfaceMembersFor(Type type, SuperTypeMembersSorter sorter)
        {
            foreach (Type type2 in type.GetInterfaces())
            {
                InterfaceMapping interfaceMap = type.GetInterfaceMap(type2);
                MethodInfo[] interfaceMethods = interfaceMap.InterfaceMethods;
                MethodInfo[] targetMethods = interfaceMap.TargetMethods;
                int index = 0;
                int length = interfaceMethods.Length;
                while (index < length)
                {
                    if ((targetMethods[index] == null) || targetMethods[index].IsAbstract)
                    {
                        sorter.Add(interfaceMethods[index]);
                    }
                    index++;
                }
            }
        }

        internal bool ImplementsInterface(IReflect iface)
        {
            foreach (TypeExpression expression in this.interfaces)
            {
                IReflect reflect = expression.ToIReflect();
                if (reflect == iface)
                {
                    return true;
                }
                if ((reflect is ClassScope) && ((ClassScope) reflect).ImplementsInterface(iface))
                {
                    return true;
                }
                if (((reflect is Type) && (iface is Type)) && ((Type) iface).IsAssignableFrom((Type) reflect))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsASubClassOf(Class cl)
        {
            if (this.superTypeExpression != null)
            {
                this.superTypeExpression.PartiallyEvaluate();
                IReflect reflect = this.superTypeExpression.ToIReflect();
                if (reflect is ClassScope)
                {
                    Class owner = ((ClassScope) reflect).owner;
                    return ((owner == cl) || owner.IsASubClassOf(cl));
                }
            }
            return false;
        }

        internal bool IsCustomAttribute()
        {
            this.GetIRForSuperType();
            if (this.superIR != Typeob.Attribute)
            {
                return false;
            }
            if (this.customAttributes == null)
            {
                return false;
            }
            this.customAttributes.PartiallyEvaluate();
            if (this.validOn == 0)
            {
                return false;
            }
            return true;
        }

        internal bool IsExpando()
        {
            if (this.hasAlreadyBeenAskedAboutExpando)
            {
                return this.isExpando;
            }
            if (this.customAttributes != null)
            {
                this.customAttributes.PartiallyEvaluate();
                if (this.customAttributes.GetAttribute(Typeob.Expando) != null)
                {
                    this.generateCodeForExpando = this.isExpando = true;
                }
            }
            bool superClassIsExpando = false;
            this.GetIRForSuperType();
            ClassScope superIR = this.superIR as ClassScope;
            if (superIR != null)
            {
                superIR.owner.PartiallyEvaluate();
                if (superIR.owner.IsExpando())
                {
                    this.isExpando = superClassIsExpando = true;
                }
            }
            else if (Microsoft.JScript.CustomAttribute.IsDefined((Type) this.superIR, typeof(Expando), true))
            {
                this.isExpando = superClassIsExpando = true;
            }
            this.hasAlreadyBeenAskedAboutExpando = true;
            if (this.generateCodeForExpando)
            {
                this.CheckIfOKToGenerateCodeForExpando(superClassIsExpando);
            }
            if (this.isExpando)
            {
                this.classob.noExpando = false;
                return true;
            }
            return false;
        }

        private bool IsInTheSameCompilationUnit(MemberInfo member)
        {
            return ((member is JSField) || (member is JSMethod));
        }

        private bool IsInTheSamePackage(MemberInfo member)
        {
            if (!(member is JSMethod) && !(member is JSField))
            {
                return false;
            }
            PackageScope package = null;
            if (member is JSMethod)
            {
                package = ((JSMethod) member).GetPackage();
            }
            else if (member is JSConstructor)
            {
                package = ((JSConstructor) member).GetPackage();
            }
            else
            {
                package = ((JSField) member).GetPackage();
            }
            return (this.classob.GetPackage() == package);
        }

        protected bool NeedsToBeCheckedForCLSCompliance()
        {
            bool isCLSCompliant = false;
            this.clsCompliance = CLSComplianceSpec.NotAttributed;
            if (this.customAttributes != null)
            {
                Microsoft.JScript.CustomAttribute elem = this.customAttributes.GetAttribute(Typeob.CLSCompliantAttribute);
                if (elem != null)
                {
                    this.clsCompliance = elem.GetCLSComplianceValue();
                    isCLSCompliant = this.clsCompliance == CLSComplianceSpec.CLSCompliant;
                    this.customAttributes.Remove(elem);
                }
            }
            if ((this.clsCompliance == CLSComplianceSpec.CLSCompliant) && !base.Engine.isCLSCompliant)
            {
                base.context.HandleError(JSError.TypeAssemblyCLSCompliantMismatch);
            }
            if ((this.clsCompliance == CLSComplianceSpec.NotAttributed) && ((this.attributes & TypeAttributes.Public) != TypeAttributes.AnsiClass))
            {
                isCLSCompliant = base.Engine.isCLSCompliant;
            }
            return isCLSCompliant;
        }

        internal static bool ParametersMatch(ParameterInfo[] suppars, ParameterInfo[] pars)
        {
            if (suppars.Length != pars.Length)
            {
                return false;
            }
            int index = 0;
            int length = pars.Length;
            while (index < length)
            {
                IReflect reflect = (suppars[index] is ParameterDeclaration) ? ((ParameterDeclaration) suppars[index]).ParameterIReflect : suppars[index].ParameterType;
                IReflect reflect2 = (pars[index] is ParameterDeclaration) ? ((ParameterDeclaration) pars[index]).ParameterIReflect : pars[index].ParameterType;
                if (!reflect2.Equals(reflect))
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        internal override AST PartiallyEvaluate()
        {
            if (!this.isAlreadyPartiallyEvaluated)
            {
                this.isAlreadyPartiallyEvaluated = true;
                this.IsExpando();
                this.classob.SetParent(new WithObject(this.enclosingScope, this.superIR, true));
                base.Globals.ScopeStack.Push(this.classob);
                try
                {
                    this.body.PartiallyEvaluate();
                    if (this.implicitDefaultConstructor != null)
                    {
                        this.implicitDefaultConstructor.PartiallyEvaluate();
                    }
                }
                finally
                {
                    base.Globals.ScopeStack.Pop();
                }
                foreach (JSMemberField field in this.fields)
                {
                    field.CheckOverloadsForDuplicates();
                }
                this.CheckIfValidExtensionOfSuperType();
                this.CheckThatAllAbstractSuperClassMethodsAreImplemented();
            }
            return this;
        }

        private Assembly ResolveEnum(object sender, ResolveEventArgs args)
        {
            FieldInfo field = this.classob.GetField(args.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if ((field != null) && field.IsLiteral)
            {
                ClassScope constantValue = TypeReferences.GetConstantValue(field) as ClassScope;
                if (constantValue != null)
                {
                    constantValue.owner.TranslateToCreateTypeCall();
                }
            }
            return base.compilerGlobals.assemblyBuilder;
        }

        private void SetAccessibility(FieldAttributes attributes)
        {
            FieldAttributes attributes2 = attributes & FieldAttributes.FieldAccessMask;
            if (this.enclosingScope is ClassScope)
            {
                switch (attributes2)
                {
                    case FieldAttributes.Public:
                        this.attributes |= TypeAttributes.NestedPublic;
                        return;

                    case FieldAttributes.Family:
                        this.attributes |= TypeAttributes.NestedFamily;
                        return;

                    case FieldAttributes.Assembly:
                        this.attributes |= TypeAttributes.NestedAssembly;
                        return;

                    case FieldAttributes.Private:
                        this.attributes |= TypeAttributes.NestedPrivate;
                        return;

                    case FieldAttributes.FamORAssem:
                        this.attributes |= TypeAttributes.NestedFamORAssem;
                        return;
                }
                this.attributes |= TypeAttributes.NestedPublic;
            }
            else
            {
                switch (attributes2)
                {
                    case FieldAttributes.Public:
                    case FieldAttributes.PrivateScope:
                        this.attributes |= TypeAttributes.Public;
                        break;
                }
            }
        }

        private void SetupConstructors()
        {
            MemberInfo[] member = this.classob.GetMember(this.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (member == null)
            {
                this.AllocateImplicitDefaultConstructor();
                this.classob.AddNewField(this.name, this.implicitDefaultConstructor, FieldAttributes.Literal);
                this.classob.constructors = new ConstructorInfo[] { new JSConstructor(this.implicitDefaultConstructor) };
            }
            else
            {
                MemberInfo info = null;
                foreach (MemberInfo info2 in member)
                {
                    if (info2 is JSFieldMethod)
                    {
                        FunctionObject func = ((JSFieldMethod) info2).func;
                        if (info == null)
                        {
                            info = info2;
                        }
                        if (func.return_type_expr != null)
                        {
                            func.return_type_expr.context.HandleError(JSError.ConstructorMayNotHaveReturnType);
                        }
                        if (((func.attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope) || ((func.attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope))
                        {
                            func.isStatic = false;
                            JSVariableField field = (JSVariableField) ((JSFieldMethod) info2).field;
                            field.attributeFlags &= ~FieldAttributes.Static;
                            field.originalContext.HandleError(JSError.NotValidForConstructor);
                        }
                        func.return_type_expr = new TypeExpression(new ConstantWrapper(Typeob.Void, base.context));
                        func.own_scope.AddReturnValueField();
                    }
                }
                if (info != null)
                {
                    this.classob.constructors = ((JSMemberField) ((JSFieldMethod) info).field).GetAsConstructors(this.classob);
                }
                else
                {
                    this.AllocateImplicitDefaultConstructor();
                    this.classob.constructors = new ConstructorInfo[] { new JSConstructor(this.implicitDefaultConstructor) };
                }
            }
        }

        private void TranslateToCOMPlusClass()
        {
            if (!this.isCooked)
            {
                this.isCooked = true;
                if (this is EnumDeclaration)
                {
                    if (!(this.enclosingScope is ClassScope))
                    {
                        this.TranslateToCreateTypeCall();
                    }
                }
                else
                {
                    if (this.superClass != null)
                    {
                        this.superClass.TranslateToCOMPlusClass();
                    }
                    int index = 0;
                    int length = this.interfaces.Length;
                    while (index < length)
                    {
                        IReflect reflect = this.interfaces[index].ToIReflect();
                        if (reflect is ClassScope)
                        {
                            ((ClassScope) reflect).owner.TranslateToCOMPlusClass();
                        }
                        index++;
                    }
                    base.Globals.ScopeStack.Push(this.classob);
                    TypeBuilder classwriter = base.compilerGlobals.classwriter;
                    base.compilerGlobals.classwriter = (TypeBuilder) this.classob.classwriter;
                    if (!this.isInterface)
                    {
                        ILGenerator iLGenerator = base.compilerGlobals.classwriter.DefineTypeInitializer().GetILGenerator();
                        LocalBuilder local = null;
                        if (this.classob.staticInitializerUsesEval)
                        {
                            local = iLGenerator.DeclareLocal(Typeob.VsaEngine);
                            iLGenerator.Emit(OpCodes.Ldtoken, this.classob.GetTypeBuilder());
                            ConstantWrapper.TranslateToILInt(iLGenerator, 0);
                            iLGenerator.Emit(OpCodes.Newarr, Typeob.JSLocalField);
                            if (base.Engine.PEFileKind == PEFileKinds.Dll)
                            {
                                iLGenerator.Emit(OpCodes.Ldtoken, this.classob.GetTypeBuilder());
                                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.createVsaEngineWithType);
                            }
                            else
                            {
                                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.createVsaEngine);
                            }
                            iLGenerator.Emit(OpCodes.Dup);
                            iLGenerator.Emit(OpCodes.Stloc, local);
                            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.pushStackFrameForStaticMethod);
                            iLGenerator.BeginExceptionBlock();
                        }
                        this.body.TranslateToILStaticInitializers(iLGenerator);
                        if (this.classob.staticInitializerUsesEval)
                        {
                            iLGenerator.BeginFinallyBlock();
                            iLGenerator.Emit(OpCodes.Ldloc, local);
                            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.popScriptObjectMethod);
                            iLGenerator.Emit(OpCodes.Pop);
                            iLGenerator.EndExceptionBlock();
                        }
                        iLGenerator.Emit(OpCodes.Ret);
                        this.EmitUsingNamespaces(iLGenerator);
                        MethodBuilder builder4 = base.compilerGlobals.classwriter.DefineMethod(".init", MethodAttributes.Private, Typeob.Void, new Type[0]);
                        this.fieldInitializer = builder4;
                        iLGenerator = builder4.GetILGenerator();
                        if (this.classob.instanceInitializerUsesEval)
                        {
                            iLGenerator.Emit(OpCodes.Ldarg_0);
                            ConstantWrapper.TranslateToILInt(iLGenerator, 0);
                            iLGenerator.Emit(OpCodes.Newarr, Typeob.JSLocalField);
                            iLGenerator.Emit(OpCodes.Ldarg_0);
                            iLGenerator.Emit(OpCodes.Callvirt, CompilerGlobals.getEngineMethod);
                            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.pushStackFrameForMethod);
                            iLGenerator.BeginExceptionBlock();
                        }
                        this.body.TranslateToILInstanceInitializers(iLGenerator);
                        if (this.classob.instanceInitializerUsesEval)
                        {
                            iLGenerator.BeginFinallyBlock();
                            iLGenerator.Emit(OpCodes.Ldarg_0);
                            iLGenerator.Emit(OpCodes.Callvirt, CompilerGlobals.getEngineMethod);
                            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.popScriptObjectMethod);
                            iLGenerator.Emit(OpCodes.Pop);
                            iLGenerator.EndExceptionBlock();
                        }
                        iLGenerator.Emit(OpCodes.Ret);
                        this.EmitUsingNamespaces(iLGenerator);
                        if (this.implicitDefaultConstructor != null)
                        {
                            this.implicitDefaultConstructor.TranslateToIL(base.compilerGlobals);
                        }
                        if (this.generateCodeForExpando)
                        {
                            this.GetExpandoIndexerGetter();
                            this.GetExpandoIndexerSetter();
                            this.GetExpandoDeleteMethod();
                            this.GenerateGetEnumerator();
                        }
                        this.EmitILForINeedEngineMethods();
                    }
                    if (!(this.enclosingScope is ClassScope))
                    {
                        this.TranslateToCreateTypeCall();
                    }
                    base.compilerGlobals.classwriter = classwriter;
                    base.Globals.ScopeStack.Pop();
                }
            }
        }

        private void TranslateToCreateTypeCall()
        {
            if (this.cookedType == null)
            {
                if (this is EnumDeclaration)
                {
                    EnumBuilder classwriter = this.classob.classwriter as EnumBuilder;
                    if (classwriter != null)
                    {
                        this.cookedType = classwriter.CreateType();
                    }
                    else
                    {
                        this.cookedType = ((TypeBuilder) this.classob.classwriter).CreateType();
                    }
                }
                else
                {
                    if (this.superClass != null)
                    {
                        this.superClass.TranslateToCreateTypeCall();
                    }
                    AppDomain domain = Thread.GetDomain();
                    ResolveEventHandler handler = new ResolveEventHandler(this.ResolveEnum);
                    domain.TypeResolve += handler;
                    this.cookedType = ((TypeBuilder) this.classob.classwriter).CreateType();
                    domain.TypeResolve -= handler;
                    foreach (JSMemberField field in this.fields)
                    {
                        ClassScope scope = field.value as ClassScope;
                        if (scope != null)
                        {
                            scope.owner.TranslateToCreateTypeCall();
                        }
                    }
                }
            }
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            this.GetTypeBuilderOrEnumBuilder();
            this.TranslateToCOMPlusClass();
            object metaData = this.ownField.GetMetaData();
            if (metaData != null)
            {
                il.Emit(OpCodes.Ldtoken, this.classob.classwriter);
                il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
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

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }

        internal bool IsStatic
        {
            get
            {
                if (!this.isStatic)
                {
                    return !(this.enclosingScope is ClassScope);
                }
                return true;
            }
        }
    }
}

