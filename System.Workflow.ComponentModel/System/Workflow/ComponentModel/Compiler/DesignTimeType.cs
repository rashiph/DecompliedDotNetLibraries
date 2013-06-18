namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Text;

    internal sealed class DesignTimeType : Type, ICloneable
    {
        private Attribute[] attributes;
        private List<CodeTypeDeclaration> codeDomTypes;
        private CodeNamespaceImportCollection codeNamespaceImports;
        private ConstructorInfo[] constructors;
        private Type declaringType;
        private static readonly char[] elementDecorators = new char[] { '[', '*', '&' };
        private EventInfo[] events;
        private FieldInfo[] fields;
        private string fullName;
        private Guid guid;
        private MethodInfo[] methods;
        private static readonly char[] nameSeparators = new char[] { '.', '+' };
        private Type[] nestedTypes;
        private PropertyInfo[] properties;
        private TypeAttributes typeAttributes;
        private ITypeProvider typeProvider;

        internal DesignTimeType(Type declaringType, string elementTypeFullName, ITypeProvider typeProvider)
        {
            this.nestedTypes = new Type[0];
            this.guid = Guid.Empty;
            if (typeProvider == null)
            {
                throw new ArgumentNullException("typeProvider");
            }
            if (elementTypeFullName.LastIndexOfAny(elementDecorators) == -1)
            {
                throw new ArgumentException(SR.GetString("NotElementType"), "elementTypeFullName");
            }
            if (elementTypeFullName == null)
            {
                throw new ArgumentNullException("FullName");
            }
            this.fullName = Helper.EnsureTypeName(elementTypeFullName);
            this.codeDomTypes = null;
            this.nestedTypes = new Type[0];
            this.codeNamespaceImports = null;
            this.typeProvider = typeProvider;
            this.declaringType = declaringType;
            Type elementType = this.GetElementType();
            if (elementType == null)
            {
                throw new ArgumentException(SR.GetString("NotElementType"), "elementTypeFullName");
            }
            if (base.IsArray)
            {
                this.typeAttributes = ((elementType.Attributes & TypeAttributes.NestedFamORAssem) | TypeAttributes.Sealed) | TypeAttributes.Serializable;
            }
            else
            {
                this.typeAttributes = TypeAttributes.AnsiClass;
            }
        }

        internal DesignTimeType(Type declaringType, string typeName, CodeNamespaceImportCollection codeNamespaceImports, string namespaceName, ITypeProvider typeProvider)
        {
            this.nestedTypes = new Type[0];
            this.guid = Guid.Empty;
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (codeNamespaceImports == null)
            {
                throw new ArgumentNullException("codeNamespaceImports");
            }
            if (typeProvider == null)
            {
                throw new ArgumentNullException("typeProvider");
            }
            if ((namespaceName == null) && (declaringType == null))
            {
                throw new InvalidOperationException(SR.GetString("NamespaceAndDeclaringTypeCannotBeNull"));
            }
            typeName = Helper.EnsureTypeName(typeName);
            namespaceName = Helper.EnsureTypeName(namespaceName);
            if (declaringType == null)
            {
                if (namespaceName.Length == 0)
                {
                    this.fullName = typeName;
                }
                else
                {
                    this.fullName = namespaceName + "." + typeName;
                }
            }
            else
            {
                this.fullName = declaringType.FullName + "+" + typeName;
            }
            this.codeDomTypes = new List<CodeTypeDeclaration>();
            this.codeNamespaceImports = codeNamespaceImports;
            this.typeProvider = typeProvider;
            this.declaringType = declaringType;
            this.typeAttributes = TypeAttributes.AnsiClass;
        }

        internal void AddCodeTypeDeclaration(CodeTypeDeclaration codeDomType)
        {
            if (codeDomType == null)
            {
                throw new ArgumentNullException("codeDomType");
            }
            this.typeAttributes |= codeDomType.TypeAttributes & ~TypeAttributes.Public;
            this.typeAttributes |= Helper.ConvertToTypeAttributes(codeDomType.Attributes, this.declaringType);
            foreach (CodeAttributeDeclaration declaration in codeDomType.CustomAttributes)
            {
                if ((string.Equals(declaration.Name, "System.SerializableAttribute", StringComparison.Ordinal) || string.Equals(declaration.Name, "System.Serializable", StringComparison.Ordinal)) || (string.Equals(declaration.Name, "SerializableAttribute", StringComparison.Ordinal) || string.Equals(declaration.Name, "Serializable", StringComparison.Ordinal)))
                {
                    this.typeAttributes |= TypeAttributes.Serializable;
                    break;
                }
            }
            this.codeDomTypes.Add(codeDomType);
            this.attributes = null;
            this.constructors = null;
            this.fields = null;
            this.events = null;
            this.properties = null;
            this.methods = null;
            this.LoadNestedTypes(codeDomType);
        }

        public object Clone()
        {
            return this;
        }

        private MemberInfo CreateMemberInfo(Type memberInfoType, CodeTypeMember member)
        {
            MemberInfo info = null;
            if ((memberInfoType == typeof(PropertyInfo)) && (member is CodeMemberProperty))
            {
                return new DesignTimePropertyInfo(this, member as CodeMemberProperty);
            }
            if ((memberInfoType == typeof(EventInfo)) && (member is CodeMemberEvent))
            {
                return new DesignTimeEventInfo(this, member as CodeMemberEvent);
            }
            if ((memberInfoType == typeof(FieldInfo)) && (member is CodeMemberField))
            {
                return new DesignTimeFieldInfo(this, member as CodeMemberField);
            }
            if ((memberInfoType == typeof(ConstructorInfo)) && ((member is CodeConstructor) || (member is CodeTypeConstructor)))
            {
                return new DesignTimeConstructorInfo(this, member as CodeMemberMethod);
            }
            if ((memberInfoType == typeof(MethodInfo)) && (member.GetType() == typeof(CodeMemberMethod)))
            {
                info = new DesignTimeMethodInfo(this, member as CodeMemberMethod);
            }
            return info;
        }

        private void EnsureMembers(Type type)
        {
            if ((type == typeof(PropertyInfo)) && (this.properties == null))
            {
                this.properties = this.GetCodeDomMembers<PropertyInfo>().ToArray();
            }
            else if ((type == typeof(FieldInfo)) && (this.fields == null))
            {
                this.fields = this.GetCodeDomMembers<FieldInfo>().ToArray();
            }
            else if ((type == typeof(ConstructorInfo)) && (this.constructors == null))
            {
                this.constructors = this.GetCodeDomConstructors().ToArray();
            }
            else if ((type == typeof(EventInfo)) && (this.events == null))
            {
                this.events = this.GetCodeDomMembers<EventInfo>().ToArray();
            }
            else if ((type == typeof(MethodInfo)) && (this.methods == null))
            {
                this.EnsureMembers(typeof(PropertyInfo));
                this.EnsureMembers(typeof(EventInfo));
                List<MethodInfo> codeDomMembers = this.GetCodeDomMembers<MethodInfo>();
                MethodInfo item = null;
                foreach (PropertyInfo info2 in this.properties)
                {
                    item = info2.GetGetMethod();
                    if (item != null)
                    {
                        codeDomMembers.Add(item);
                    }
                    item = info2.GetSetMethod();
                    if (item != null)
                    {
                        codeDomMembers.Add(item);
                    }
                }
                foreach (EventInfo info3 in this.events)
                {
                    item = info3.GetAddMethod();
                    if (item != null)
                    {
                        codeDomMembers.Add(item);
                    }
                    item = info3.GetRemoveMethod();
                    if (item != null)
                    {
                        codeDomMembers.Add(item);
                    }
                    item = info3.GetRaiseMethod();
                    if (item != null)
                    {
                        codeDomMembers.Add(item);
                    }
                }
                this.methods = codeDomMembers.ToArray();
            }
        }

        private bool FilterMember(MemberInfo memberInfo, BindingFlags bindingFlags)
        {
            bool isPublic = false;
            bool isStatic = false;
            if (base.IsInterface)
            {
                isPublic = true;
                isStatic = false;
            }
            else if (memberInfo is MethodBase)
            {
                isPublic = (memberInfo as MethodBase).IsPublic;
                isStatic = (memberInfo as MethodBase).IsStatic;
            }
            else if (memberInfo is DesignTimeEventInfo)
            {
                isPublic = (memberInfo as DesignTimeEventInfo).IsPublic;
                isStatic = (memberInfo as DesignTimeEventInfo).IsStatic;
            }
            else if (memberInfo is FieldInfo)
            {
                isPublic = (memberInfo as FieldInfo).IsPublic;
                isStatic = (memberInfo as FieldInfo).IsStatic;
            }
            else if (memberInfo is PropertyInfo)
            {
                PropertyInfo info = memberInfo as PropertyInfo;
                MethodInfo getMethod = null;
                if (info.CanRead)
                {
                    getMethod = info.GetGetMethod(true);
                }
                else
                {
                    getMethod = info.GetSetMethod(true);
                }
                if (getMethod != null)
                {
                    isPublic = getMethod.IsPublic;
                    isStatic = getMethod.IsStatic;
                }
            }
            else if (memberInfo is Type)
            {
                isPublic = (memberInfo as Type).IsPublic || (memberInfo as Type).IsNestedPublic;
                return ((isPublic && ((bindingFlags & BindingFlags.Public) != BindingFlags.Default)) || (!isPublic && ((bindingFlags & BindingFlags.NonPublic) != BindingFlags.Default)));
            }
            if ((!isPublic || ((bindingFlags & BindingFlags.Public) == BindingFlags.Default)) && (isPublic || ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.Default)))
            {
                return false;
            }
            return ((isStatic && ((bindingFlags & BindingFlags.Static) != BindingFlags.Default)) || (!isStatic && ((bindingFlags & BindingFlags.Instance) != BindingFlags.Default)));
        }

        public override int GetArrayRank()
        {
            if (!base.IsArray)
            {
                throw new ArgumentException(TypeSystemSR.GetString("Error_TypeIsNotArray"));
            }
            int num = this.Name.LastIndexOf('[');
            int num2 = 1;
            while (this.Name[num] != ']')
            {
                if (this.Name[num] == ',')
                {
                    num2++;
                }
                num++;
            }
            return num2;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.typeAttributes;
        }

        internal MemberInfo GetBaseMember(Type type, Type baseType, BindingFlags bindingAttr, MemberSignature memberSignature)
        {
            if (memberSignature == null)
            {
                throw new ArgumentNullException("memberSignature");
            }
            if (baseType == null)
            {
                return null;
            }
            MemberInfo nestedType = null;
            if (typeof(PropertyInfo).IsAssignableFrom(type))
            {
                if (memberSignature.Parameters != null)
                {
                    return baseType.GetProperty(memberSignature.Name, bindingAttr, null, memberSignature.ReturnType, memberSignature.Parameters, null);
                }
                return baseType.GetProperty(memberSignature.Name, bindingAttr);
            }
            if (typeof(EventInfo).IsAssignableFrom(type))
            {
                return baseType.GetEvent(memberSignature.Name, bindingAttr);
            }
            if (typeof(ConstructorInfo).IsAssignableFrom(type))
            {
                return baseType.GetConstructor(bindingAttr, null, memberSignature.Parameters, null);
            }
            if (typeof(MethodInfo).IsAssignableFrom(type))
            {
                if (memberSignature.Parameters != null)
                {
                    return baseType.GetMethod(memberSignature.Name, bindingAttr, null, memberSignature.Parameters, null);
                }
                return baseType.GetMethod(memberSignature.Name, bindingAttr);
            }
            if (typeof(FieldInfo).IsAssignableFrom(type))
            {
                return baseType.GetField(memberSignature.Name, bindingAttr);
            }
            if (typeof(Type).IsAssignableFrom(type))
            {
                nestedType = baseType.GetNestedType(memberSignature.Name, bindingAttr);
            }
            return nestedType;
        }

        private MemberInfo[] GetBaseMembers(Type type, Type baseType, BindingFlags bindingAttr)
        {
            MemberInfo[] nestedTypes = null;
            if (type == typeof(PropertyInfo))
            {
                return baseType.GetProperties(bindingAttr);
            }
            if (type == typeof(EventInfo))
            {
                return baseType.GetEvents(bindingAttr);
            }
            if (type == typeof(ConstructorInfo))
            {
                return baseType.GetConstructors(bindingAttr);
            }
            if (type == typeof(MethodInfo))
            {
                return baseType.GetMethods(bindingAttr);
            }
            if (type == typeof(FieldInfo))
            {
                return baseType.GetFields(bindingAttr);
            }
            if (type == typeof(Type))
            {
                nestedTypes = baseType.GetNestedTypes(bindingAttr);
            }
            return nestedTypes;
        }

        private List<ConstructorInfo> GetCodeDomConstructors()
        {
            List<ConstructorInfo> codeDomMembers = this.GetCodeDomMembers<ConstructorInfo>();
            if (base.IsValueType || ((codeDomMembers.Count == 0) && !base.IsAbstract))
            {
                CodeConstructor codeConstructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
                ConstructorInfo item = new DesignTimeConstructorInfo(this, codeConstructor);
                codeDomMembers.Add(item);
            }
            return codeDomMembers;
        }

        private List<T> GetCodeDomMembers<T>() where T: MemberInfo
        {
            List<T> list = new List<T>();
            if (this.codeDomTypes != null)
            {
                foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                {
                    if ((declaration is CodeTypeDelegate) && (typeof(T) == typeof(MethodInfo)))
                    {
                        CodeMemberMethod method = new CodeMemberMethod {
                            Name = "Invoke",
                            Attributes = MemberAttributes.Public
                        };
                        foreach (CodeParameterDeclarationExpression expression in ((CodeTypeDelegate) declaration).Parameters)
                        {
                            method.Parameters.Add(expression);
                        }
                        method.ReturnType = ((CodeTypeDelegate) declaration).ReturnType;
                        list.Add((T) this.CreateMemberInfo(typeof(MethodInfo), method));
                    }
                    foreach (CodeTypeMember member in declaration.Members)
                    {
                        T item = (T) this.CreateMemberInfo(typeof(T), member);
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return this.GetMemberHelper<ConstructorInfo>(bindingAttr, new MemberSignature(null, types, null), ref this.constructors);
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return this.GetMembersHelper<ConstructorInfo>(bindingAttr, ref this.constructors, false);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if ((this.codeDomTypes != null) && (this.attributes == null))
            {
                CodeAttributeDeclarationCollection codeAttributeCollection = new CodeAttributeDeclarationCollection();
                foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                {
                    codeAttributeCollection.AddRange(declaration.CustomAttributes);
                }
                this.attributes = Helper.LoadCustomAttributes(codeAttributeCollection, this);
            }
            if (this.attributes != null)
            {
                return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
            }
            return new object[0];
        }

        public override MemberInfo[] GetDefaultMembers()
        {
            DefaultMemberAttribute attribute = null;
            for (Type type = this; type != null; type = type.BaseType)
            {
                object[] customAttributes = this.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    attribute = customAttributes[0] as DefaultMemberAttribute;
                }
                if (attribute != null)
                {
                    break;
                }
            }
            if (attribute == null)
            {
                return new MemberInfo[0];
            }
            string memberName = attribute.MemberName;
            MemberInfo[] member = base.GetMember(memberName);
            if (member == null)
            {
                member = new MemberInfo[0];
            }
            return member;
        }

        public override Type GetElementType()
        {
            Type type = null;
            int length = this.fullName.LastIndexOfAny(elementDecorators);
            if (length >= 0)
            {
                type = this.ResolveType(this.fullName.Substring(0, length));
            }
            return type;
        }

        public Type GetEnumType()
        {
            if (this.codeDomTypes != null)
            {
                foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                {
                    Type type = declaration.UserData[typeof(Enum)] as Type;
                    if (type != null)
                    {
                        return type;
                    }
                    if (declaration.BaseTypes.Count > 1)
                    {
                        CodeTypeReference reference = declaration.BaseTypes[1];
                        Type type2 = reference.UserData[typeof(Enum)] as Type;
                        if (type2 != null)
                        {
                            return type2;
                        }
                    }
                }
            }
            return typeof(int);
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return this.GetMemberHelper<EventInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.events);
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return this.GetMembersHelper<EventInfo>(bindingAttr, ref this.events, true);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return this.GetMemberHelper<FieldInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.fields);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return this.GetMembersHelper<FieldInfo>(bindingAttr, ref this.fields, true);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (this.codeDomTypes != null)
            {
                StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                {
                    foreach (CodeTypeReference reference in declaration.BaseTypes)
                    {
                        Type type = this.ResolveType(GetTypeNameFromCodeTypeReference(reference, this));
                        if (type != null)
                        {
                            if (type.IsInterface && string.Equals(type.FullName, name, comparisonType))
                            {
                                return type;
                            }
                            Type type2 = type.GetInterface(name, ignoreCase);
                            if (type2 != null)
                            {
                                return type2;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public override Type[] GetInterfaces()
        {
            ArrayList list = new ArrayList();
            if (this.codeDomTypes != null)
            {
                foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                {
                    foreach (CodeTypeReference reference in declaration.BaseTypes)
                    {
                        Type item = this.ResolveType(GetTypeNameFromCodeTypeReference(reference, this));
                        if (item != null)
                        {
                            if (item.IsInterface && !list.Contains(item))
                            {
                                list.Add(item);
                            }
                            foreach (Type type2 in item.GetInterfaces())
                            {
                                if ((type2 != null) && !list.Contains(type2))
                                {
                                    list.Add(type2);
                                }
                            }
                        }
                    }
                }
            }
            return (Type[]) list.ToArray(typeof(Type));
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            this.VerifyGetMemberArguments(name, bindingAttr);
            List<MemberInfo> list = new List<MemberInfo>();
            if ((type & MemberTypes.Method) != 0)
            {
                list.AddRange(this.GetMembersHelper<MethodInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.methods));
            }
            if ((type & MemberTypes.Constructor) != 0)
            {
                list.AddRange(this.GetMembersHelper<ConstructorInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.constructors));
            }
            if ((type & MemberTypes.Property) != 0)
            {
                list.AddRange(this.GetMembersHelper<PropertyInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.properties));
            }
            if ((type & MemberTypes.Event) != 0)
            {
                list.AddRange(this.GetMembersHelper<EventInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.events));
            }
            if ((type & MemberTypes.Field) != 0)
            {
                list.AddRange(this.GetMembersHelper<FieldInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.fields));
            }
            if ((type & MemberTypes.NestedType) != 0)
            {
                list.AddRange(this.GetMembersHelper<Type>(bindingAttr, new MemberSignature(name, null, null), ref this.nestedTypes));
            }
            return list.ToArray();
        }

        private T GetMemberHelper<T>(BindingFlags bindingAttr, MemberSignature memberSignature, ref T[] members) where T: MemberInfo
        {
            this.VerifyGetMemberArguments(bindingAttr);
            this.EnsureMembers(typeof(T));
            foreach (T local in members)
            {
                MemberSignature signature = new MemberSignature(local);
                if (signature.FilterSignature(memberSignature) && this.FilterMember(local, bindingAttr))
                {
                    return local;
                }
            }
            if ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default)
            {
                if ((bindingAttr & BindingFlags.FlattenHierarchy) == BindingFlags.Default)
                {
                    bindingAttr &= ~BindingFlags.Static;
                }
                Type baseType = this.BaseType;
                if (baseType != null)
                {
                    T local2 = (T) this.GetBaseMember(typeof(T), baseType, bindingAttr, memberSignature);
                    if (local2 != null)
                    {
                        if (((!(local2 is FieldInfo) || !(local2 as FieldInfo).IsPrivate) && (!(local2 is MethodBase) || !(local2 as MethodBase).IsPrivate)) && (!(local2 is Type) || !(local2 as Type).IsNestedPrivate))
                        {
                            return local2;
                        }
                        return default(T);
                    }
                }
            }
            return default(T);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            this.VerifyGetMemberArguments(bindingAttr);
            ArrayList list = new ArrayList();
            list.AddRange(this.GetMethods(bindingAttr));
            list.AddRange(this.GetProperties(bindingAttr));
            list.AddRange(this.GetEvents(bindingAttr));
            list.AddRange(this.GetFields(bindingAttr));
            list.AddRange(this.GetNestedTypes(bindingAttr));
            return (MemberInfo[]) list.ToArray(typeof(MemberInfo));
        }

        private T[] GetMembersHelper<T>(BindingFlags bindingAttr, ref T[] members, bool searchBase) where T: MemberInfo
        {
            this.VerifyGetMemberArguments(bindingAttr);
            this.EnsureMembers(typeof(T));
            Dictionary<MemberSignature, T> dictionary = new Dictionary<MemberSignature, T>();
            foreach (T local in members)
            {
                MemberSignature key = new MemberSignature(local);
                if (this.FilterMember(local, bindingAttr) && !dictionary.ContainsKey(key))
                {
                    dictionary.Add(new MemberSignature(local), local);
                }
            }
            if (searchBase && ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default))
            {
                if ((bindingAttr & BindingFlags.FlattenHierarchy) == BindingFlags.Default)
                {
                    bindingAttr &= ~BindingFlags.Static;
                }
                Type baseType = this.BaseType;
                if (baseType != null)
                {
                    T[] localArray = this.GetBaseMembers(typeof(T), baseType, bindingAttr) as T[];
                    foreach (T local2 in localArray)
                    {
                        if (((!(local2 is FieldInfo) || !(local2 as FieldInfo).IsPrivate) && (!(local2 is MethodBase) || !(local2 as MethodBase).IsPrivate)) && (!(local2 is Type) || !(local2 as Type).IsNestedPrivate))
                        {
                            MemberSignature signature2 = new MemberSignature(local2);
                            if (!dictionary.ContainsKey(signature2))
                            {
                                dictionary.Add(signature2, local2);
                            }
                        }
                    }
                }
            }
            List<T> list = new List<T>(dictionary.Values);
            return list.ToArray();
        }

        private T[] GetMembersHelper<T>(BindingFlags bindingAttr, MemberSignature memberSignature, ref T[] members) where T: MemberInfo
        {
            List<T> list = new List<T>();
            foreach (T local in this.GetMembersHelper<T>(bindingAttr, ref members, true))
            {
                MemberSignature signature = new MemberSignature(local);
                if (signature.FilterSignature(memberSignature))
                {
                    list.Add(local);
                }
            }
            return list.ToArray();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return this.GetMemberHelper<MethodInfo>(bindingAttr, new MemberSignature(name, types, null), ref this.methods);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return this.GetMembersHelper<MethodInfo>(bindingAttr, ref this.methods, true);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return this.GetMemberHelper<Type>(bindingAttr, new MemberSignature(name, null, null), ref this.nestedTypes);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return this.GetMembersHelper<Type>(bindingAttr, ref this.nestedTypes, false);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return this.GetMembersHelper<PropertyInfo>(bindingAttr, ref this.properties, true);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return this.GetMemberHelper<PropertyInfo>(bindingAttr, new MemberSignature(name, types, null), ref this.properties);
        }

        internal static string GetTypeNameFromCodeTypeReference(CodeTypeReference codeTypeReference, DesignTimeType declaringType)
        {
            StringBuilder builder = new StringBuilder();
            if (codeTypeReference.ArrayRank == 0)
            {
                Type type = null;
                if (declaringType != null)
                {
                    type = declaringType.ResolveType(codeTypeReference.BaseType);
                }
                if (type != null)
                {
                    builder.Append(type.FullName);
                }
                else
                {
                    builder.Append(codeTypeReference.BaseType);
                }
                if ((codeTypeReference.TypeArguments != null) && (codeTypeReference.TypeArguments.Count > 0))
                {
                    if (codeTypeReference.BaseType.IndexOf('`') == -1)
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "`{0}", new object[] { codeTypeReference.TypeArguments.Count }));
                    }
                    builder.Append("[");
                    foreach (CodeTypeReference reference in codeTypeReference.TypeArguments)
                    {
                        builder.Append("[");
                        builder.Append(GetTypeNameFromCodeTypeReference(reference, declaringType));
                        builder.Append("],");
                    }
                    builder.Length--;
                    builder.Append("]");
                }
            }
            else
            {
                builder.Append(GetTypeNameFromCodeTypeReference(codeTypeReference.ArrayElementType, declaringType));
                builder.Append("[");
                for (int i = 0; i < (codeTypeReference.ArrayRank - 1); i++)
                {
                    builder.Append(',');
                }
                builder.Append("]");
            }
            return builder.ToString();
        }

        protected override bool HasElementTypeImpl()
        {
            return (this.Name.LastIndexOfAny(elementDecorators) != -1);
        }

        public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        protected override bool IsArrayImpl()
        {
            int num = this.Name.LastIndexOfAny(elementDecorators);
            return ((num != -1) && (this.Name[num] == '['));
        }

        public override bool IsAssignableFrom(Type c)
        {
            return TypeProvider.IsAssignable(this, c);
        }

        protected override bool IsByRefImpl()
        {
            return (this.fullName[this.fullName.Length - 1] == '&');
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsContextfulImpl()
        {
            return false;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            this.GetCustomAttributes(true);
            return Helper.IsDefined(attributeType, inherit, this.attributes, this);
        }

        protected override bool IsMarshalByRefImpl()
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return (this.fullName[this.fullName.Length - 1] == '*');
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        public override bool IsSubclassOf(Type c)
        {
            if (c == null)
            {
                return false;
            }
            return TypeProvider.IsSubclassOf(this, c);
        }

        private void LoadNestedTypes(CodeTypeDeclaration codeDomType)
        {
            List<Type> list = new List<Type>();
            foreach (Type type in this.nestedTypes)
            {
                list.Add(type);
            }
            foreach (CodeTypeMember member in codeDomType.Members)
            {
                if (!(member is CodeTypeDeclaration))
                {
                    continue;
                }
                CodeTypeDeclaration declaration = member as CodeTypeDeclaration;
                Type item = null;
                foreach (Type type3 in list)
                {
                    if (type3.Name.Equals(Helper.EnsureTypeName(declaration.Name)))
                    {
                        item = type3;
                        break;
                    }
                }
                if (item == null)
                {
                    item = new DesignTimeType(this, declaration.Name, this.codeNamespaceImports, this.fullName, this.typeProvider);
                    list.Add(item);
                    ((TypeProvider) this.typeProvider).AddType(item);
                }
                ((DesignTimeType) item).AddCodeTypeDeclaration(declaration);
            }
            this.nestedTypes = list.ToArray();
        }

        public override Type MakeArrayType()
        {
            return this.typeProvider.GetType(string.Format(CultureInfo.InvariantCulture, "{0}[]", new object[] { this.FullName }));
        }

        public override Type MakeByRefType()
        {
            return this.ResolveType(this.fullName + "&");
        }

        internal Type ResolveType(string name)
        {
            Type type = null;
            type = this.typeProvider.GetType(name);
            if ((type == null) && !string.IsNullOrEmpty(this.Namespace))
            {
                type = this.typeProvider.GetType(this.Namespace + "." + name);
            }
            if (type == null)
            {
                type = this.typeProvider.GetType(this.fullName + "+" + name);
            }
            if ((type == null) && (this.codeNamespaceImports != null))
            {
                foreach (CodeNamespaceImport import in this.codeNamespaceImports)
                {
                    type = this.typeProvider.GetType(import.Namespace + "." + name);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            if (type == null)
            {
                string str = name;
                int index = name.IndexOf('.');
                int length = -1;
                while (((length = str.LastIndexOf('.')) != index) && (type == null))
                {
                    str = str.Substring(0, length) + "+" + str.Substring(length + 1);
                    type = this.typeProvider.GetType(str);
                }
            }
            return type;
        }

        public override string ToString()
        {
            return this.fullName;
        }

        private void VerifyGetMemberArguments(BindingFlags bindingAttr)
        {
            BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase;
            if ((bindingAttr & ~flags) != BindingFlags.Default)
            {
                throw new ArgumentException(TypeSystemSR.GetString("Error_GetMemberBindingOptions"));
            }
        }

        private void VerifyGetMemberArguments(string name, BindingFlags bindingAttr)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.VerifyGetMemberArguments(bindingAttr);
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return null;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return this.FullName;
            }
        }

        public override Type BaseType
        {
            get
            {
                Type type = null;
                if (this.codeDomTypes != null)
                {
                    foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                    {
                        foreach (CodeTypeReference reference in declaration.BaseTypes)
                        {
                            Type type2 = this.ResolveType(GetTypeNameFromCodeTypeReference(reference, this));
                            if ((type2 != null) && !type2.IsInterface)
                            {
                                type = type2;
                                break;
                            }
                        }
                        if ((type != null) && !type.Equals(this.ResolveType("System.Object")))
                        {
                            break;
                        }
                    }
                }
                if (type == null)
                {
                    if (base.IsArray)
                    {
                        return this.ResolveType("System.Array");
                    }
                    if ((this.codeDomTypes == null) || (this.codeDomTypes.Count <= 0))
                    {
                        return type;
                    }
                    if (this.codeDomTypes[0].IsStruct)
                    {
                        return this.ResolveType("System.ValueType");
                    }
                    if (this.codeDomTypes[0].IsEnum)
                    {
                        return this.ResolveType("System.Enum");
                    }
                    if ((this.codeDomTypes[0].IsClass && !base.IsByRef) && !base.IsPointer)
                    {
                        return this.ResolveType("System.Object");
                    }
                    if (this.codeDomTypes[0] is CodeTypeDelegate)
                    {
                        type = this.ResolveType("System.Delegate");
                    }
                }
                return type;
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }

        public override string FullName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                if (this.guid == Guid.Empty)
                {
                    this.guid = Guid.NewGuid();
                }
                return this.guid;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return null;
            }
        }

        public override string Name
        {
            get
            {
                string fullName = this.fullName;
                int index = fullName.IndexOf('[');
                if (index != -1)
                {
                    index = fullName.Substring(0, index).LastIndexOfAny(nameSeparators);
                }
                else
                {
                    index = fullName.LastIndexOfAny(nameSeparators);
                }
                if (index != -1)
                {
                    fullName = this.fullName.Substring(index + 1);
                }
                return fullName;
            }
        }

        public override string Namespace
        {
            get
            {
                if (this.fullName == this.Name)
                {
                    return string.Empty;
                }
                if (this.declaringType != null)
                {
                    return this.declaringType.Namespace;
                }
                return this.fullName.Substring(0, (this.fullName.Length - this.Name.Length) - 1);
            }
        }

        internal ITypeProvider Provider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.typeProvider;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this;
            }
        }

        internal class MemberSignature
        {
            private readonly int hashCode;
            private string name;
            private Type[] parameters;
            private Type returnType;

            internal MemberSignature(MemberInfo memberInfo)
            {
                this.name = memberInfo.Name;
                if (memberInfo is MethodBase)
                {
                    List<Type> list = new List<Type>();
                    foreach (ParameterInfo info in (memberInfo as MethodBase).GetParameters())
                    {
                        list.Add(info.ParameterType);
                    }
                    this.parameters = list.ToArray();
                    if (memberInfo is MethodInfo)
                    {
                        this.returnType = ((MethodInfo) memberInfo).ReturnType;
                    }
                }
                else if (memberInfo is PropertyInfo)
                {
                    PropertyInfo info2 = memberInfo as PropertyInfo;
                    List<Type> list2 = new List<Type>();
                    foreach (ParameterInfo info3 in info2.GetIndexParameters())
                    {
                        list2.Add(info3.ParameterType);
                    }
                    this.parameters = list2.ToArray();
                    this.returnType = info2.PropertyType;
                }
                this.hashCode = this.GetHashCodeImpl();
            }

            internal MemberSignature(string name, Type[] parameters, Type returnType)
            {
                this.name = name;
                this.returnType = returnType;
                if (parameters != null)
                {
                    this.parameters = (Type[]) parameters.Clone();
                }
                this.hashCode = this.GetHashCodeImpl();
            }

            public override bool Equals(object obj)
            {
                DesignTimeType.MemberSignature signature = obj as DesignTimeType.MemberSignature;
                if (((signature == null) || (this.name != signature.name)) || (this.returnType != signature.returnType))
                {
                    return false;
                }
                if (((this.Parameters == null) && (signature.Parameters != null)) || ((this.Parameters != null) && (signature.Parameters == null)))
                {
                    return false;
                }
                if (this.Parameters != null)
                {
                    if (this.parameters.Length != signature.parameters.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < this.parameters.Length; i++)
                    {
                        if (this.parameters[i] != signature.parameters[i])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public bool FilterSignature(DesignTimeType.MemberSignature maskSignature)
            {
                if (maskSignature == null)
                {
                    throw new ArgumentNullException("maskSignature");
                }
                if (((maskSignature.Name != null) && (this.name != maskSignature.name)) || ((maskSignature.returnType != null) && (this.returnType != maskSignature.returnType)))
                {
                    return false;
                }
                if (maskSignature.parameters != null)
                {
                    if (this.parameters == null)
                    {
                        return false;
                    }
                    if (this.parameters.Length != maskSignature.parameters.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < this.parameters.Length; i++)
                    {
                        if (!this.parameters[i].Equals(maskSignature.parameters[i]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }

            private int GetHashCodeImpl()
            {
                int hashCode = 0;
                if (this.name != null)
                {
                    hashCode = this.name.GetHashCode();
                }
                if ((this.parameters != null) && (this.parameters.Length > 0))
                {
                    for (int i = 0; i < this.parameters.Length; i++)
                    {
                        if (this.parameters[i] != null)
                        {
                            hashCode ^= this.parameters[i].GetHashCode();
                        }
                    }
                }
                if (this.returnType != null)
                {
                    hashCode ^= this.returnType.GetHashCode();
                }
                return hashCode;
            }

            public override string ToString()
            {
                string str = string.Empty;
                if (this.returnType != null)
                {
                    str = this.returnType.FullName + " ";
                }
                if ((this.name != null) && (this.name.Length != 0))
                {
                    str = str + this.name;
                }
                if ((this.parameters == null) || (this.parameters.Length <= 0))
                {
                    return str;
                }
                str = str + "(";
                for (int i = 0; i < this.parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        str = str + ", ";
                    }
                    if (this.parameters[i] != null)
                    {
                        if ((this.parameters[i].GetType() != null) && this.parameters[i].GetType().IsByRef)
                        {
                            str = str + "ref ";
                        }
                        str = str + this.parameters[i].FullName;
                    }
                }
                return (str + ")");
            }

            public string Name
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.name;
                }
            }

            public Type[] Parameters
            {
                get
                {
                    if (this.parameters == null)
                    {
                        return null;
                    }
                    return (Type[]) this.parameters.Clone();
                }
            }

            public Type ReturnType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.returnType;
                }
            }
        }
    }
}

