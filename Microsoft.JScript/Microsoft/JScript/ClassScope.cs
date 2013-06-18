namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class ClassScope : ActivationObject, IComparable
    {
        internal Type classwriter;
        internal ConstructorInfo[] constructors;
        internal bool instanceInitializerUsesEval;
        internal bool inStaticInitializerCode;
        internal JSProperty itemProp;
        internal string name;
        internal bool noExpando;
        internal FieldInfo outerClassField;
        internal Class owner;
        internal PackageScope package;
        internal bool staticInitializerUsesEval;

        internal ClassScope(AST name, GlobalScope scope) : base(scope)
        {
            this.name = name.ToString();
            base.engine = scope.engine;
            base.fast = scope.fast;
            this.noExpando = true;
            base.isKnownAtCompileTime = true;
            this.owner = null;
            this.constructors = new JSConstructor[0];
            ScriptObject parent = base.engine.ScriptObjectStackTop();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is ClassScope)
            {
                this.package = ((ClassScope) parent).GetPackage();
            }
            else if (parent is PackageScope)
            {
                this.package = (PackageScope) parent;
            }
            else
            {
                this.package = null;
            }
            this.itemProp = null;
            this.outerClassField = null;
            this.inStaticInitializerCode = false;
            this.staticInitializerUsesEval = false;
            this.instanceInitializerUsesEval = false;
        }

        internal void AddClassesFromInheritanceChain(string name, ArrayList result)
        {
            IReflect superType = this;
            bool flag = true;
            while (superType is ClassScope)
            {
                if (superType.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0)
                {
                    result.Add(superType);
                    flag = false;
                }
                if (superType is ClassScope)
                {
                    superType = ((ClassScope) superType).GetSuperType();
                }
                else
                {
                    superType = ((Type) superType).BaseType;
                }
            }
            if ((flag && (superType is Type)) && (superType.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Length > 0))
            {
                result.Add(superType);
            }
        }

        public int CompareTo(object ob)
        {
            if (ob == this)
            {
                return 0;
            }
            if ((ob is ClassScope) && ((ClassScope) ob).IsSameOrDerivedFrom(this))
            {
                return 1;
            }
            return -1;
        }

        protected override JSVariableField CreateField(string name, FieldAttributes attributeFlags, object value)
        {
            return new JSMemberField(this, name, value, attributeFlags);
        }

        internal object FakeCallToTypeMethod(MethodInfo method, object[] arguments, Exception e)
        {
            ParameterInfo[] parameters = method.GetParameters();
            int length = parameters.Length;
            Type[] types = new Type[length];
            for (int i = 0; i < length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            MethodInfo info = typeof(ClassScope).GetMethod(method.Name, types);
            if (info == null)
            {
                throw e;
            }
            return info.Invoke(this, arguments);
        }

        internal Type GetBakedSuperType()
        {
            this.owner.PartiallyEvaluate();
            if (this.owner is EnumDeclaration)
            {
                return ((EnumDeclaration) this.owner).baseType.ToType();
            }
            object obj2 = ((WithObject) base.parent).contained_object;
            if (obj2 is ClassScope)
            {
                return ((ClassScope) obj2).GetBakedSuperType();
            }
            if (obj2 is Type)
            {
                return (Type) obj2;
            }
            return Globals.TypeRefs.ToReferenceContext(obj2.GetType());
        }

        public ConstructorInfo[] GetConstructors()
        {
            return this.constructors;
        }

        public object[] GetCustomAttributes(bool inherit)
        {
            CustomAttributeList customAttributes = this.owner.customAttributes;
            if (customAttributes == null)
            {
                return new object[0];
            }
            return (object[]) customAttributes.Evaluate();
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            return this.GetFullName();
        }

        public FieldInfo GetField(string name)
        {
            return base.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        }

        internal string GetFullName()
        {
            PackageScope package = this.GetPackage();
            if (package != null)
            {
                return (package.GetName() + "." + this.name);
            }
            if (this.owner.enclosingScope is ClassScope)
            {
                return (((ClassScope) this.owner.enclosingScope).GetFullName() + "." + this.name);
            }
            return this.name;
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            MemberInfoList mems = new MemberInfoList();
            FieldInfo elem = (FieldInfo) base.name_table[name];
            if (elem == null)
            {
                goto Label_0139;
            }
            if (elem.IsPublic)
            {
                if ((bindingAttr & BindingFlags.Public) != BindingFlags.Default)
                {
                    goto Label_0040;
                }
                goto Label_0139;
            }
            if ((bindingAttr & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                goto Label_0139;
            }
        Label_0040:
            if (!elem.IsLiteral)
            {
                goto Label_011E;
            }
            object obj2 = ((JSMemberField) elem).value;
            if (obj2 is FunctionObject)
            {
                FunctionObject obj3 = (FunctionObject) obj2;
                if (obj3.isConstructor)
                {
                    return new MemberInfo[0];
                }
                if (obj3.isExpandoMethod)
                {
                    if ((bindingAttr & BindingFlags.Instance) != BindingFlags.Default)
                    {
                        mems.Add(elem);
                    }
                }
                else
                {
                    ((JSMemberField) elem).AddOverloadedMembers(mems, this, bindingAttr | BindingFlags.DeclaredOnly);
                }
                goto Label_0139;
            }
            if (!(obj2 is JSProperty))
            {
                if (((obj2 is ClassScope) && ((bindingAttr & BindingFlags.Instance) != BindingFlags.Default)) && !((ClassScope) obj2).owner.isStatic)
                {
                    mems.Add(elem);
                    goto Label_0139;
                }
                goto Label_011E;
            }
            JSProperty property = (JSProperty) obj2;
            MethodInfo info2 = (property.getter != null) ? property.getter : property.setter;
            if (info2.IsStatic)
            {
                if ((bindingAttr & BindingFlags.Static) != BindingFlags.Default)
                {
                    goto Label_00EC;
                }
                goto Label_0139;
            }
            if ((bindingAttr & BindingFlags.Instance) == BindingFlags.Default)
            {
                goto Label_0139;
            }
        Label_00EC:
            mems.Add(property);
            goto Label_0139;
        Label_011E:
            if (elem.IsStatic)
            {
                if ((bindingAttr & BindingFlags.Static) != BindingFlags.Default)
                {
                    goto Label_0132;
                }
                goto Label_0139;
            }
            if ((bindingAttr & BindingFlags.Instance) == BindingFlags.Default)
            {
                goto Label_0139;
            }
        Label_0132:
            mems.Add(elem);
        Label_0139:
            if (((this.owner != null) && this.owner.isInterface) && ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default))
            {
                return this.owner.GetInterfaceMember(name);
            }
            if ((base.parent != null) && ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default))
            {
                MemberInfo[] member = base.parent.GetMember(name, bindingAttr);
                if (member != null)
                {
                    foreach (MemberInfo info3 in member)
                    {
                        if (info3.MemberType == MemberTypes.Field)
                        {
                            elem = (FieldInfo) info3;
                            if ((!elem.IsStatic && !elem.IsLiteral) && !(elem is JSWrappedField))
                            {
                                elem = new JSWrappedField(elem, base.parent);
                            }
                            mems.Add(elem);
                        }
                        else
                        {
                            mems.Add(ScriptObject.WrapMember(info3, base.parent));
                        }
                    }
                }
            }
            return mems.ToArray();
        }

        internal JSMemberField[] GetMemberFields()
        {
            int count = base.field_table.Count;
            JSMemberField[] fieldArray = new JSMemberField[count];
            for (int i = 0; i < count; i++)
            {
                fieldArray[i] = (JSMemberField) base.field_table[i];
            }
            return fieldArray;
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            MemberInfoList mems = new MemberInfoList();
            IEnumerator enumerator = base.field_table.GetEnumerator();
            while (enumerator.MoveNext())
            {
                FieldInfo current = (FieldInfo) enumerator.Current;
                if (current.IsLiteral && (current is JSMemberField))
                {
                    object obj2 = null;
                    if ((obj2 = ((JSMemberField) current).value) is FunctionObject)
                    {
                        if (!((FunctionObject) obj2).isConstructor)
                        {
                            ((JSMemberField) current).AddOverloadedMembers(mems, this, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        }
                    }
                    else if (obj2 is JSProperty)
                    {
                        mems.Add((MemberInfo) obj2);
                    }
                    else
                    {
                        mems.Add(current);
                    }
                }
                else
                {
                    mems.Add(current);
                }
            }
            if (base.parent != null)
            {
                mems.AddRange(base.parent.GetMembers(bindingAttr));
            }
            return mems.ToArray();
        }

        public MethodInfo GetMethod(string name)
        {
            return base.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        }

        internal override string GetName()
        {
            return this.name;
        }

        internal PackageScope GetPackage()
        {
            return this.package;
        }

        public PropertyInfo GetProperty(string name)
        {
            return base.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        }

        internal override void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
        }

        internal IReflect GetSuperType()
        {
            this.owner.PartiallyEvaluate();
            return (IReflect) ((WithObject) base.parent).contained_object;
        }

        internal TypeBuilder GetTypeBuilder()
        {
            return (TypeBuilder) this.GetTypeBuilderOrEnumBuilder();
        }

        internal Type GetTypeBuilderOrEnumBuilder()
        {
            if (this.classwriter == null)
            {
                this.classwriter = this.owner.GetTypeBuilderOrEnumBuilder();
            }
            return this.classwriter;
        }

        internal IReflect GetUnderlyingTypeIfEnum()
        {
            if (this.owner is EnumDeclaration)
            {
                return ((EnumDeclaration) this.owner.PartiallyEvaluate()).baseType.ToIReflect();
            }
            return this;
        }

        internal bool HasInstance(object ob)
        {
            if (ob is JSObject)
            {
                for (ScriptObject obj2 = ((JSObject) ob).GetParent(); obj2 != null; obj2 = obj2.GetParent())
                {
                    if (obj2 == this)
                    {
                        return true;
                    }
                    if ((obj2 is WithObject) && (((WithObject) obj2).contained_object == this))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool ImplementsInterface(IReflect iface)
        {
            this.owner.PartiallyEvaluate();
            object obj2 = ((WithObject) base.parent).contained_object;
            if (obj2 is ClassScope)
            {
                if (!((ClassScope) obj2).ImplementsInterface(iface))
                {
                    return this.owner.ImplementsInterface(iface);
                }
                return true;
            }
            if (!(obj2 is Type) || !(iface is Type))
            {
                return this.owner.ImplementsInterface(iface);
            }
            if (!((Type) iface).IsAssignableFrom((Type) obj2))
            {
                return this.owner.ImplementsInterface(iface);
            }
            return true;
        }

        internal bool IsCLSCompliant()
        {
            this.owner.PartiallyEvaluate();
            TypeAttributes attributes = this.owner.attributes & TypeAttributes.NestedFamORAssem;
            if ((attributes != TypeAttributes.Public) && (attributes != TypeAttributes.NestedPublic))
            {
                return false;
            }
            if (this.owner.clsCompliance == CLSComplianceSpec.NotAttributed)
            {
                return this.owner.Engine.isCLSCompliant;
            }
            return (this.owner.clsCompliance == CLSComplianceSpec.CLSCompliant);
        }

        internal bool IsNestedIn(ClassScope other, bool isStatic)
        {
            if (base.parent == null)
            {
                return false;
            }
            this.owner.PartiallyEvaluate();
            if (this.owner.enclosingScope == other)
            {
                if (!isStatic)
                {
                    return !this.owner.isStatic;
                }
                return true;
            }
            return ((this.owner.enclosingScope is ClassScope) && ((ClassScope) this.owner.enclosingScope).IsNestedIn(other, isStatic));
        }

        internal bool IsPromotableTo(Type other)
        {
            Type bakedSuperType = this.GetBakedSuperType();
            if (other.IsAssignableFrom(bakedSuperType))
            {
                return true;
            }
            if (other.IsInterface && this.ImplementsInterface(other))
            {
                return true;
            }
            EnumDeclaration owner = this.owner as EnumDeclaration;
            return ((owner != null) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) owner.baseType.ToType(), (IReflect) other));
        }

        internal bool IsSameOrDerivedFrom(ClassScope other)
        {
            if (this == other)
            {
                return true;
            }
            if (other.owner.isInterface)
            {
                return this.ImplementsInterface(other);
            }
            if (base.parent == null)
            {
                return false;
            }
            this.owner.PartiallyEvaluate();
            object obj2 = ((WithObject) base.parent).contained_object;
            return ((obj2 is ClassScope) && ((ClassScope) obj2).IsSameOrDerivedFrom(other));
        }

        internal bool IsSameOrDerivedFrom(Type other)
        {
            if (this.owner.GetTypeBuilder() == other)
            {
                return true;
            }
            if (base.parent == null)
            {
                return false;
            }
            this.owner.PartiallyEvaluate();
            object obj2 = ((WithObject) base.parent).contained_object;
            if (obj2 is ClassScope)
            {
                return ((ClassScope) obj2).IsSameOrDerivedFrom(other);
            }
            return other.IsAssignableFrom((Type) obj2);
        }

        internal bool ParentIsInSamePackage()
        {
            object obj2 = ((WithObject) base.parent).contained_object;
            return ((obj2 is ClassScope) && (((ClassScope) obj2).package == this.package));
        }

        internal static ClassScope ScopeOfClassMemberInitializer(ScriptObject scope)
        {
            while (scope != null)
            {
                if (scope is FunctionScope)
                {
                    return null;
                }
                ClassScope scope2 = scope as ClassScope;
                if (scope2 != null)
                {
                    return scope2;
                }
                scope = scope.GetParent();
            }
            return null;
        }
    }
}

