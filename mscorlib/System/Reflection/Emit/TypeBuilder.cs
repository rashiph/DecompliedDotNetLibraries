namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComDefaultInterface(typeof(_TypeBuilder)), ComVisible(true), ClassInterface(ClassInterfaceType.None), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class TypeBuilder : Type, _TypeBuilder
    {
        private bool m_bIsGenParam;
        private bool m_bIsGenTypeDef;
        internal List<CustAttr> m_ca;
        private int m_constructorCount;
        private TypeBuilder m_DeclaringType;
        private MethodBuilder m_declMeth;
        internal System.Reflection.GenericParameterAttributes m_genParamAttributes;
        private int m_genParamPos;
        private TypeBuilder m_genTypeDef;
        internal bool m_hasBeenCreated;
        internal TypeAttributes m_iAttr;
        private GenericTypeParameterBuilder[] m_inst;
        private System.Reflection.Emit.PackingSize m_iPackingSize;
        internal bool m_isHiddenGlobalType;
        internal bool m_isHiddenType;
        private int m_iTypeSize;
        internal int m_lastTokenizedMethod;
        internal List<MethodBuilder> m_listMethods;
        private ModuleBuilder m_module;
        internal RuntimeType m_runtimeType;
        private string m_strFullQualName;
        internal string m_strName;
        private string m_strNameSpace;
        private System.Reflection.Emit.TypeToken m_tdType;
        private List<Type> m_typeInterfaces;
        private Type m_typeParent;
        private Type m_underlyingSystemType;
        public const int UnspecifiedTypeSize = 0;

        internal TypeBuilder(ModuleBuilder module)
        {
            this.m_tdType = new System.Reflection.Emit.TypeToken(0x2000000);
            this.m_isHiddenGlobalType = true;
            this.m_module = module;
            this.m_listMethods = new List<MethodBuilder>();
            this.m_lastTokenizedMethod = -1;
        }

        internal TypeBuilder(string szName, int genParamPos, MethodBuilder declMeth)
        {
            this.m_declMeth = declMeth;
            this.m_DeclaringType = this.m_declMeth.GetTypeBuilder();
            this.m_module = declMeth.GetModuleBuilder();
            this.InitAsGenericParam(szName, genParamPos);
        }

        private TypeBuilder(string szName, int genParamPos, TypeBuilder declType)
        {
            this.m_DeclaringType = declType;
            this.m_module = declType.GetModuleBuilder();
            this.InitAsGenericParam(szName, genParamPos);
        }

        [SecurityCritical]
        internal TypeBuilder(string name, TypeAttributes attr, Type parent, System.Reflection.Module module, System.Reflection.Emit.PackingSize iPackingSize, int iTypeSize, TypeBuilder enclosingType)
        {
            this.Init(name, attr, parent, null, module, iPackingSize, iTypeSize, enclosingType);
        }

        [SecurityCritical]
        internal TypeBuilder(string name, TypeAttributes attr, Type parent, Type[] interfaces, System.Reflection.Module module, System.Reflection.Emit.PackingSize iPackingSize, TypeBuilder enclosingType)
        {
            this.Init(name, attr, parent, interfaces, module, iPackingSize, 0, enclosingType);
        }

        [SecuritySafeCritical]
        public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
        {
            lock (this.SyncRoot)
            {
                this.AddDeclarativeSecurityNoLock(action, pset);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void AddDeclarativeSecurity(RuntimeModule module, int parent, SecurityAction action, byte[] blob, int cb);
        [SecurityCritical]
        private void AddDeclarativeSecurityNoLock(SecurityAction action, PermissionSet pset)
        {
            if (pset == null)
            {
                throw new ArgumentNullException("pset");
            }
            if ((!Enum.IsDefined(typeof(SecurityAction), action) || (action == SecurityAction.RequestMinimum)) || ((action == SecurityAction.RequestOptional) || (action == SecurityAction.RequestRefuse)))
            {
                throw new ArgumentOutOfRangeException("action");
            }
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            byte[] blob = null;
            int cb = 0;
            if (!pset.IsEmpty())
            {
                blob = pset.EncodeXml();
                cb = blob.Length;
            }
            AddDeclarativeSecurity(this.m_module.GetNativeHandle(), this.m_tdType.Token, action, blob, cb);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddInterfaceImpl(RuntimeModule module, int tdTypeDef, int tkInterface);
        [SecuritySafeCritical, ComVisible(true)]
        public void AddInterfaceImplementation(Type interfaceType)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            this.ThrowIfGeneric();
            this.CheckContext(new Type[] { interfaceType });
            this.ThrowIfCreated();
            System.Reflection.Emit.TypeToken typeTokenInternal = this.m_module.GetTypeTokenInternal(interfaceType);
            AddInterfaceImpl(this.m_module.GetNativeHandle(), this.m_tdType.Token, typeTokenInternal.Token);
            this.m_typeInterfaces.Add(interfaceType);
        }

        internal void CheckContext(params Type[][] typess)
        {
            this.m_module.CheckContext(typess);
        }

        internal void CheckContext(params Type[] types)
        {
            this.m_module.CheckContext(types);
        }

        [SecuritySafeCritical]
        public Type CreateType()
        {
            lock (this.SyncRoot)
            {
                return this.CreateTypeNoLock();
            }
        }

        [SecurityCritical]
        private Type CreateTypeNoLock()
        {
            if (this.IsCreated())
            {
                return this.m_runtimeType;
            }
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            if (this.m_typeInterfaces == null)
            {
                this.m_typeInterfaces = new List<Type>();
            }
            int[] numArray = new int[this.m_typeInterfaces.Count];
            for (int i = 0; i < this.m_typeInterfaces.Count; i++)
            {
                numArray[i] = this.m_module.GetTypeTokenInternal(this.m_typeInterfaces[i]).Token;
            }
            int tkParent = 0;
            if (this.m_typeParent != null)
            {
                tkParent = this.m_module.GetTypeTokenInternal(this.m_typeParent).Token;
            }
            if (this.IsGenericParameter)
            {
                int[] numArray2;
                if (this.m_typeParent != null)
                {
                    numArray2 = new int[this.m_typeInterfaces.Count + 2];
                    numArray2[numArray2.Length - 2] = tkParent;
                }
                else
                {
                    numArray2 = new int[this.m_typeInterfaces.Count + 1];
                }
                for (int k = 0; k < this.m_typeInterfaces.Count; k++)
                {
                    numArray2[k] = this.m_module.GetTypeTokenInternal(this.m_typeInterfaces[k]).Token;
                }
                int num4 = (this.m_declMeth == null) ? this.m_DeclaringType.m_tdType.Token : this.m_declMeth.GetToken().Token;
                this.m_tdType = new System.Reflection.Emit.TypeToken(DefineGenericParam(this.m_module.GetNativeHandle(), this.m_strName, num4, this.m_genParamAttributes, this.m_genParamPos, numArray2));
                if (this.m_ca != null)
                {
                    foreach (CustAttr attr in this.m_ca)
                    {
                        attr.Bake(this.m_module, this.MetadataTokenInternal);
                    }
                }
                this.m_hasBeenCreated = true;
                return this;
            }
            if (((this.m_tdType.Token & 0xffffff) != 0) && ((tkParent & 0xffffff) != 0))
            {
                SetParentType(this.m_module.GetNativeHandle(), this.m_tdType.Token, tkParent);
            }
            if (this.m_inst != null)
            {
                GenericTypeParameterBuilder[] inst = this.m_inst;
                for (int m = 0; m < inst.Length; m++)
                {
                    Type type = inst[m];
                    if (type is GenericTypeParameterBuilder)
                    {
                        ((GenericTypeParameterBuilder) type).m_type.CreateType();
                    }
                }
            }
            if (((!this.m_isHiddenGlobalType && (this.m_constructorCount == 0)) && (((this.m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.AnsiClass) && !base.IsValueType)) && ((this.m_iAttr & (TypeAttributes.Sealed | TypeAttributes.Abstract)) != (TypeAttributes.Sealed | TypeAttributes.Abstract)))
            {
                this.DefineDefaultConstructor(MethodAttributes.Public);
            }
            int count = this.m_listMethods.Count;
            for (int j = 0; j < count; j++)
            {
                MethodBuilder builder = this.m_listMethods[j];
                if (builder.IsGenericMethodDefinition)
                {
                    builder.GetToken();
                }
                MethodAttributes attributes = builder.Attributes;
                if (((builder.GetMethodImplementationFlags() & (MethodImplAttributes.PreserveSig | MethodImplAttributes.ManagedMask | MethodImplAttributes.CodeTypeMask)) == MethodImplAttributes.IL) && ((attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PrivateScope))
                {
                    int num5;
                    int num8;
                    byte[] signature = builder.GetLocalsSignature().InternalGetSignature(out num8);
                    if (((attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope) && ((this.m_iAttr & TypeAttributes.Abstract) == TypeAttributes.AnsiClass))
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadTypeAttributesNotAbstract"));
                    }
                    byte[] body = builder.GetBody();
                    if ((attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope)
                    {
                        if (body != null)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadMethodBody"));
                        }
                    }
                    else if ((body == null) || (body.Length == 0))
                    {
                        if (builder.m_ilGenerator != null)
                        {
                            builder.CreateMethodBodyHelper(builder.GetILGenerator());
                        }
                        body = builder.GetBody();
                        if (((body == null) || (body.Length == 0)) && !builder.m_canBeRuntimeImpl)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadEmptyMethodBody", new object[] { builder.Name }));
                        }
                    }
                    if (builder.m_ilGenerator != null)
                    {
                        num5 = builder.m_ilGenerator.GetMaxStackSize() + builder.GetNumberOfExceptions();
                    }
                    else
                    {
                        num5 = 0x10;
                    }
                    __ExceptionInstance[] exceptionInstances = builder.GetExceptionInstances();
                    int[] tokenFixups = builder.GetTokenFixups();
                    int[] rVAFixups = builder.GetRVAFixups();
                    SetMethodIL(this.m_module.GetNativeHandle(), builder.GetToken().Token, builder.InitLocals, body, (body != null) ? body.Length : 0, signature, num8, num5, exceptionInstances, (exceptionInstances != null) ? exceptionInstances.Length : 0, tokenFixups, (tokenFixups != null) ? tokenFixups.Length : 0, rVAFixups, (rVAFixups != null) ? rVAFixups.Length : 0);
                    if (this.m_module.ContainingAssemblyBuilder.m_assemblyData.m_access == AssemblyBuilderAccess.Run)
                    {
                        builder.ReleaseBakedStructures();
                    }
                }
            }
            this.m_hasBeenCreated = true;
            RuntimeType o = null;
            TermCreateClass(this.m_module.GetNativeHandle(), this.m_tdType.Token, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            if (this.m_isHiddenGlobalType)
            {
                return null;
            }
            this.m_runtimeType = o;
            if ((this.m_DeclaringType != null) && (this.m_DeclaringType.m_runtimeType != null))
            {
                this.m_DeclaringType.m_runtimeType.InvalidateCachedNestedType();
            }
            return o;
        }

        [ComVisible(true)]
        public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
        {
            return this.DefineConstructor(attributes, callingConvention, parameterTypes, null, null);
        }

        [ComVisible(true), SecuritySafeCritical]
        public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
        {
            if (((this.m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask) && ((attributes & MethodAttributes.Static) != MethodAttributes.Static))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConstructorNotAllowedOnInterface"));
            }
            lock (this.SyncRoot)
            {
                return this.DefineConstructorNoLock(attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
            }
        }

        [SecurityCritical]
        private ConstructorBuilder DefineConstructorNoLock(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
        {
            string constructorName;
            this.CheckContext(parameterTypes);
            this.CheckContext(requiredCustomModifiers);
            this.CheckContext(optionalCustomModifiers);
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            if ((attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope)
            {
                constructorName = ConstructorInfo.ConstructorName;
            }
            else
            {
                constructorName = ConstructorInfo.TypeConstructorName;
            }
            attributes |= MethodAttributes.SpecialName;
            ConstructorBuilder builder = new ConstructorBuilder(constructorName, attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers, this.m_module, this);
            this.m_constructorCount++;
            return builder;
        }

        [SecurityCritical]
        internal static void DefineCustomAttribute(ModuleBuilder module, int tkAssociate, int tkConstructor, byte[] attr, bool toDisk, bool updateCompilerFlags)
        {
            byte[] destinationArray = null;
            if (attr != null)
            {
                destinationArray = new byte[attr.Length];
                Array.Copy(attr, destinationArray, attr.Length);
            }
            DefineCustomAttribute(module.GetNativeHandle(), tkAssociate, tkConstructor, destinationArray, (destinationArray != null) ? destinationArray.Length : 0, toDisk, updateCompilerFlags);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void DefineCustomAttribute(RuntimeModule module, int tkAssociate, int tkConstructor, byte[] attr, int attrLength, bool toDisk, bool updateCompilerFlags);
        [SecurityCritical]
        private FieldBuilder DefineDataHelper(string name, byte[] data, int size, FieldAttributes attributes)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if ((size <= 0) || (size >= 0x3f0000))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadSizeForData"));
            }
            this.ThrowIfCreated();
            string strTypeName = "$ArrayType$" + size;
            TypeBuilder type = this.m_module.FindTypeBuilderWithName(strTypeName, false) as TypeBuilder;
            if (type == null)
            {
                TypeAttributes attr = TypeAttributes.Sealed | TypeAttributes.ExplicitLayout | TypeAttributes.Public;
                type = this.m_module.DefineType(strTypeName, attr, typeof(ValueType), System.Reflection.Emit.PackingSize.Size1, size);
                type.m_isHiddenType = true;
                type.CreateType();
            }
            FieldBuilder builder2 = this.DefineField(name, type, attributes | FieldAttributes.Static);
            builder2.SetData(data, size);
            return builder2;
        }

        [SecuritySafeCritical, ComVisible(true)]
        public ConstructorBuilder DefineDefaultConstructor(MethodAttributes attributes)
        {
            if ((this.m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConstructorNotAllowedOnInterface"));
            }
            lock (this.SyncRoot)
            {
                return this.DefineDefaultConstructorNoLock(attributes);
            }
        }

        private ConstructorBuilder DefineDefaultConstructorNoLock(MethodAttributes attributes)
        {
            this.ThrowIfGeneric();
            ConstructorInfo con = null;
            if (this.m_typeParent is TypeBuilderInstantiation)
            {
                Type genericTypeDefinition = this.m_typeParent.GetGenericTypeDefinition();
                if (genericTypeDefinition is TypeBuilder)
                {
                    genericTypeDefinition = ((TypeBuilder) genericTypeDefinition).m_runtimeType;
                }
                if (genericTypeDefinition == null)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
                }
                Type type = genericTypeDefinition.MakeGenericType(this.m_typeParent.GetGenericArguments());
                if (type is TypeBuilderInstantiation)
                {
                    con = GetConstructor(type, genericTypeDefinition.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
                }
                else
                {
                    con = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                }
            }
            if (con == null)
            {
                con = this.m_typeParent.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            }
            if (con == null)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoParentDefaultConstructor"));
            }
            ConstructorBuilder builder = this.DefineConstructor(attributes, CallingConventions.Standard, null);
            this.m_constructorCount++;
            ILGenerator iLGenerator = builder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, con);
            iLGenerator.Emit(OpCodes.Ret);
            builder.m_ReturnILGen = false;
            return builder;
        }

        [SecuritySafeCritical]
        public EventBuilder DefineEvent(string name, EventAttributes attributes, Type eventtype)
        {
            lock (this.SyncRoot)
            {
                return this.DefineEventNoLock(name, attributes, eventtype);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int DefineEvent(RuntimeModule module, int tkParent, string name, EventAttributes attributes, int tkEventType);
        [SecurityCritical]
        private EventBuilder DefineEventNoLock(string name, EventAttributes attributes, Type eventtype)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (name[0] == '\0')
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "name");
            }
            this.CheckContext(new Type[] { eventtype });
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            int token = this.m_module.GetTypeTokenInternal(eventtype).Token;
            return new EventBuilder(this.m_module, name, attributes, this, new EventToken(DefineEvent(this.m_module.GetNativeHandle(), this.m_tdType.Token, name, attributes, token)));
        }

        public FieldBuilder DefineField(string fieldName, Type type, FieldAttributes attributes)
        {
            return this.DefineField(fieldName, type, null, null, attributes);
        }

        [SecuritySafeCritical]
        public FieldBuilder DefineField(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
        {
            lock (this.SyncRoot)
            {
                return this.DefineFieldNoLock(fieldName, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int DefineField(RuntimeModule module, int tkParent, string name, byte[] signature, int sigLength, FieldAttributes attributes);
        [SecurityCritical]
        private FieldBuilder DefineFieldNoLock(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
        {
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            this.CheckContext(new Type[] { type });
            this.CheckContext(requiredCustomModifiers);
            if (((this.m_underlyingSystemType == null) && this.IsEnum) && ((attributes & FieldAttributes.Static) == FieldAttributes.PrivateScope))
            {
                this.m_underlyingSystemType = type;
            }
            return new FieldBuilder(this, fieldName, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int DefineGenericParam(RuntimeModule module, string name, int tkParent, System.Reflection.GenericParameterAttributes attributes, int position, int[] constraints);
        public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            if (names.Length == 0)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == null)
                {
                    throw new ArgumentNullException("names");
                }
            }
            if (this.m_inst != null)
            {
                throw new InvalidOperationException();
            }
            this.m_bIsGenTypeDef = true;
            this.m_inst = new GenericTypeParameterBuilder[names.Length];
            for (int j = 0; j < names.Length; j++)
            {
                this.m_inst[j] = new GenericTypeParameterBuilder(new TypeBuilder(names[j], j, this));
            }
            return this.m_inst;
        }

        [SecuritySafeCritical]
        public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
        {
            lock (this.SyncRoot)
            {
                return this.DefineInitializedDataNoLock(name, data, attributes);
            }
        }

        [SecurityCritical]
        private FieldBuilder DefineInitializedDataNoLock(string name, byte[] data, FieldAttributes attributes)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this.ThrowIfGeneric();
            return this.DefineDataHelper(name, data, data.Length, attributes);
        }

        public MethodBuilder DefineMethod(string name, MethodAttributes attributes)
        {
            return this.DefineMethod(name, attributes, CallingConventions.Standard, null, null);
        }

        public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention)
        {
            return this.DefineMethod(name, attributes, callingConvention, null, null);
        }

        public MethodBuilder DefineMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
        {
            return this.DefineMethod(name, attributes, CallingConventions.Standard, returnType, parameterTypes);
        }

        public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            return this.DefineMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int DefineMethod(RuntimeModule module, int tkParent, string name, byte[] signature, int sigLength, MethodAttributes attributes);
        [SecuritySafeCritical]
        public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
        {
            lock (this.SyncRoot)
            {
                return this.DefineMethodNoLock(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void DefineMethodImpl(RuntimeModule module, int tkType, int tkBody, int tkDecl);
        private MethodBuilder DefineMethodNoLock(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            this.CheckContext(new Type[] { returnType });
            this.CheckContext(new Type[][] { returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes });
            this.CheckContext(parameterTypeRequiredCustomModifiers);
            this.CheckContext(parameterTypeOptionalCustomModifiers);
            if (parameterTypes != null)
            {
                if ((parameterTypeOptionalCustomModifiers != null) && (parameterTypeOptionalCustomModifiers.Length != parameterTypes.Length))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", new object[] { "parameterTypeOptionalCustomModifiers", "parameterTypes" }));
                }
                if ((parameterTypeRequiredCustomModifiers != null) && (parameterTypeRequiredCustomModifiers.Length != parameterTypes.Length))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", new object[] { "parameterTypeRequiredCustomModifiers", "parameterTypes" }));
                }
            }
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            if ((!this.m_isHiddenGlobalType && ((this.m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask)) && (((attributes & MethodAttributes.Abstract) == MethodAttributes.PrivateScope) && ((attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadAttributeOnInterfaceMethod"));
            }
            MethodBuilder item = new MethodBuilder(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, this.m_module, this, false);
            if ((!this.m_isHiddenGlobalType && ((item.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope)) && item.Name.Equals(ConstructorInfo.ConstructorName))
            {
                this.m_constructorCount++;
            }
            this.m_listMethods.Add(item);
            return item;
        }

        [SecuritySafeCritical]
        public void DefineMethodOverride(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
        {
            lock (this.SyncRoot)
            {
                this.DefineMethodOverrideNoLock(methodInfoBody, methodInfoDeclaration);
            }
        }

        [SecurityCritical]
        private void DefineMethodOverrideNoLock(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
        {
            if (methodInfoBody == null)
            {
                throw new ArgumentNullException("methodInfoBody");
            }
            if (methodInfoDeclaration == null)
            {
                throw new ArgumentNullException("methodInfoDeclaration");
            }
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            if (!object.ReferenceEquals(methodInfoBody.DeclaringType, this))
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentException_BadMethodImplBody"));
            }
            MethodToken methodTokenInternal = this.m_module.GetMethodTokenInternal(methodInfoBody);
            MethodToken token2 = this.m_module.GetMethodTokenInternal(methodInfoDeclaration);
            DefineMethodImpl(this.m_module.GetNativeHandle(), this.m_tdType.Token, methodTokenInternal.Token, token2.Token);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void DefineMethodSemantics(RuntimeModule module, int tkAssociation, MethodSemanticsAttributes semantics, int tkMethod);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int DefineMethodSpec(RuntimeModule module, int tkParent, byte[] signature, int sigLength);
        [SecuritySafeCritical]
        public TypeBuilder DefineNestedType(string name)
        {
            lock (this.SyncRoot)
            {
                return this.DefineNestedTypeNoLock(name);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineNestedType(string name, TypeAttributes attr)
        {
            lock (this.SyncRoot)
            {
                return this.DefineNestedTypeNoLock(name, attr);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent)
        {
            lock (this.SyncRoot)
            {
                return this.DefineNestedTypeNoLock(name, attr, parent);
            }
        }

        [SecuritySafeCritical, ComVisible(true)]
        public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            lock (this.SyncRoot)
            {
                return this.DefineNestedTypeNoLock(name, attr, parent, interfaces);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, int typeSize)
        {
            lock (this.SyncRoot)
            {
                return this.DefineNestedTypeNoLock(name, attr, parent, typeSize);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, System.Reflection.Emit.PackingSize packSize)
        {
            lock (this.SyncRoot)
            {
                return this.DefineNestedTypeNoLock(name, attr, parent, packSize);
            }
        }

        [SecurityCritical]
        private TypeBuilder DefineNestedTypeNoLock(string name)
        {
            this.ThrowIfGeneric();
            TypeBuilder type = new TypeBuilder(name, TypeAttributes.NestedPrivate, null, null, this.m_module, System.Reflection.Emit.PackingSize.Unspecified, this);
            this.m_module.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr)
        {
            this.ThrowIfGeneric();
            TypeBuilder type = new TypeBuilder(name, attr, null, null, this.m_module, System.Reflection.Emit.PackingSize.Unspecified, this);
            this.m_module.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent)
        {
            this.ThrowIfGeneric();
            TypeBuilder type = new TypeBuilder(name, attr, parent, null, this.m_module, System.Reflection.Emit.PackingSize.Unspecified, this);
            this.m_module.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            this.CheckContext(new Type[] { parent });
            this.CheckContext(interfaces);
            this.ThrowIfGeneric();
            TypeBuilder type = new TypeBuilder(name, attr, parent, interfaces, this.m_module, System.Reflection.Emit.PackingSize.Unspecified, this);
            this.m_module.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent, int typeSize)
        {
            TypeBuilder type = new TypeBuilder(name, attr, parent, this.m_module, System.Reflection.Emit.PackingSize.Unspecified, typeSize, this);
            this.m_module.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent, System.Reflection.Emit.PackingSize packSize)
        {
            this.ThrowIfGeneric();
            TypeBuilder type = new TypeBuilder(name, attr, parent, null, this.m_module, packSize, this);
            this.m_module.AddType(type);
            return type;
        }

        [SecuritySafeCritical]
        public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            this.ThrowIfGeneric();
            return this.DefinePInvokeMethodHelper(name, dllName, name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
        }

        [SecuritySafeCritical]
        public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            return this.DefinePInvokeMethodHelper(name, dllName, entryName, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
        }

        [SecuritySafeCritical]
        public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            this.ThrowIfGeneric();
            return this.DefinePInvokeMethodHelper(name, dllName, entryName, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, nativeCallConv, nativeCharSet);
        }

        [SecurityCritical]
        private MethodBuilder DefinePInvokeMethodHelper(string name, string dllName, string importName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            this.CheckContext(new Type[] { returnType });
            this.CheckContext(new Type[][] { returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes });
            this.CheckContext(parameterTypeRequiredCustomModifiers);
            this.CheckContext(parameterTypeOptionalCustomModifiers);
            lock (this.SyncRoot)
            {
                return this.DefinePInvokeMethodHelperNoLock(name, dllName, importName, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, nativeCallConv, nativeCharSet);
            }
        }

        [SecurityCritical]
        private MethodBuilder DefinePInvokeMethodHelperNoLock(string name, string dllName, string importName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            int num;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (dllName == null)
            {
                throw new ArgumentNullException("dllName");
            }
            if (dllName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "dllName");
            }
            if (importName == null)
            {
                throw new ArgumentNullException("importName");
            }
            if (importName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "importName");
            }
            if ((attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadPInvokeMethod"));
            }
            if ((this.m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadPInvokeOnInterface"));
            }
            this.ThrowIfCreated();
            attributes |= MethodAttributes.PinvokeImpl;
            MethodBuilder item = new MethodBuilder(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, this.m_module, this, false);
            item.GetMethodSignature().InternalGetSignature(out num);
            if (this.m_listMethods.Contains(item))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MethodRedefined"));
            }
            this.m_listMethods.Add(item);
            MethodToken token = item.GetToken();
            int linkFlags = 0;
            switch (nativeCallConv)
            {
                case CallingConvention.Winapi:
                    linkFlags = 0x100;
                    break;

                case CallingConvention.Cdecl:
                    linkFlags = 0x200;
                    break;

                case CallingConvention.StdCall:
                    linkFlags = 0x300;
                    break;

                case CallingConvention.ThisCall:
                    linkFlags = 0x400;
                    break;

                case CallingConvention.FastCall:
                    linkFlags = 0x500;
                    break;
            }
            switch (nativeCharSet)
            {
                case CharSet.None:
                    break;

                case CharSet.Ansi:
                    linkFlags |= 2;
                    break;

                case CharSet.Unicode:
                    linkFlags |= 4;
                    break;

                case CharSet.Auto:
                    linkFlags |= 6;
                    break;
            }
            SetPInvokeData(this.m_module.GetNativeHandle(), dllName, importName, token.Token, linkFlags);
            item.SetToken(token);
            return item;
        }

        public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
        {
            return this.DefineProperty(name, attributes, returnType, null, null, parameterTypes, null, null);
        }

        [SecuritySafeCritical]
        public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            return this.DefineProperty(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int DefineProperty(RuntimeModule module, int tkParent, string name, PropertyAttributes attributes, byte[] signature, int sigLength);
        [SecuritySafeCritical]
        public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
        {
            return this.DefineProperty(name, attributes, 0, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
        }

        [SecuritySafeCritical]
        public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
        {
            lock (this.SyncRoot)
            {
                return this.DefinePropertyNoLock(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
            }
        }

        [SecurityCritical]
        private PropertyBuilder DefinePropertyNoLock(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
        {
            int num;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            this.ThrowIfGeneric();
            this.CheckContext(new Type[] { returnType });
            this.CheckContext(new Type[][] { returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes });
            this.CheckContext(parameterTypeRequiredCustomModifiers);
            this.CheckContext(parameterTypeOptionalCustomModifiers);
            this.ThrowIfCreated();
            SignatureHelper sig = SignatureHelper.GetPropertySigHelper(this.m_module, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
            byte[] signature = sig.InternalGetSignature(out num);
            return new PropertyBuilder(this.m_module, name, sig, attributes, returnType, new PropertyToken(DefineProperty(this.m_module.GetNativeHandle(), this.m_tdType.Token, name, attributes, signature, num)), this);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int DefineType(RuntimeModule module, string fullname, int tkParent, TypeAttributes attributes, int tkEnclosingType, int[] interfaceTokens);
        [SecuritySafeCritical, ComVisible(true)]
        public ConstructorBuilder DefineTypeInitializer()
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeInitializerNoLock();
            }
        }

        [SecurityCritical]
        private ConstructorBuilder DefineTypeInitializerNoLock()
        {
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            return new ConstructorBuilder(ConstructorInfo.TypeConstructorName, MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.Private, CallingConventions.Standard, null, this.m_module, this);
        }

        [SecuritySafeCritical]
        public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
        {
            lock (this.SyncRoot)
            {
                return this.DefineUninitializedDataNoLock(name, size, attributes);
            }
        }

        [SecurityCritical]
        private FieldBuilder DefineUninitializedDataNoLock(string name, int size, FieldAttributes attributes)
        {
            this.ThrowIfGeneric();
            return this.DefineDataHelper(name, null, size, attributes);
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.m_iAttr;
        }

        public static ConstructorInfo GetConstructor(Type type, ConstructorInfo constructor)
        {
            if (!(type is TypeBuilder) && !(type is TypeBuilderInstantiation))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeTypeBuilder"));
            }
            if (!constructor.DeclaringType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ConstructorNeedGenericDeclaringType"), "constructor");
            }
            if (!(type is TypeBuilderInstantiation))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
            }
            if ((type is TypeBuilder) && type.IsGenericTypeDefinition)
            {
                type = type.MakeGenericType(type.GetGenericArguments());
            }
            if (type.GetGenericTypeDefinition() != constructor.DeclaringType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidConstructorDeclaringType"), "type");
            }
            return ConstructorOnTypeBuilderInstantiation.GetConstructor(constructor, type as TypeBuilderInstantiation);
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        [ComVisible(true)]
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetConstructors(bindingAttr);
        }

        [SecuritySafeCritical]
        public override object[] GetCustomAttributes(bool inherit)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return CustomAttribute.GetCustomAttributes(this.m_runtimeType, typeof(object) as RuntimeType, inherit);
        }

        [SecuritySafeCritical]
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
            }
            return CustomAttribute.GetCustomAttributes(this.m_runtimeType, underlyingSystemType, inherit);
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents()
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetEvents();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetEvents(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetField(name, bindingAttr);
        }

        public static FieldInfo GetField(Type type, FieldInfo field)
        {
            if (!(type is TypeBuilder) && !(type is TypeBuilderInstantiation))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeTypeBuilder"));
            }
            if (!field.DeclaringType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_FieldNeedGenericDeclaringType"), "field");
            }
            if (!(type is TypeBuilderInstantiation))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
            }
            if ((type is TypeBuilder) && type.IsGenericTypeDefinition)
            {
                type = type.MakeGenericType(type.GetGenericArguments());
            }
            if (type.GetGenericTypeDefinition() != field.DeclaringType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFieldDeclaringType"), "type");
            }
            return FieldOnTypeBuilderInstantiation.GetField(field, type as TypeBuilderInstantiation);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetFields(bindingAttr);
        }

        public override Type[] GetGenericArguments()
        {
            return this.m_inst;
        }

        public override Type GetGenericTypeDefinition()
        {
            if (this.IsGenericTypeDefinition)
            {
                return this;
            }
            if (this.m_genTypeDef == null)
            {
                throw new InvalidOperationException();
            }
            return this.m_genTypeDef;
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetInterface(name, ignoreCase);
        }

        [ComVisible(true)]
        public override InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetInterfaceMap(interfaceType);
        }

        public override Type[] GetInterfaces()
        {
            if (this.m_runtimeType != null)
            {
                return this.m_runtimeType.GetInterfaces();
            }
            if (this.m_typeInterfaces == null)
            {
                return new Type[0];
            }
            return this.m_typeInterfaces.ToArray();
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetMember(name, type, bindingAttr);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetMembers(bindingAttr);
        }

        public static MethodInfo GetMethod(Type type, MethodInfo method)
        {
            if (!(type is TypeBuilder) && !(type is TypeBuilderInstantiation))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeTypeBuilder"));
            }
            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedGenericMethodDefinition"), "method");
            }
            if ((method.DeclaringType == null) || !method.DeclaringType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MethodNeedGenericDeclaringType"), "method");
            }
            if (type.GetGenericTypeDefinition() != method.DeclaringType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidMethodDeclaringType"), "type");
            }
            if (type.IsGenericTypeDefinition)
            {
                type = type.MakeGenericType(type.GetGenericArguments());
            }
            if (!(type is TypeBuilderInstantiation))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
            }
            return MethodOnTypeBuilderInstantiation.GetMethod(method, type as TypeBuilderInstantiation);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            if (types == null)
            {
                return this.m_runtimeType.GetMethod(name, bindingAttr);
            }
            return this.m_runtimeType.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetMethods(bindingAttr);
        }

        internal ModuleBuilder GetModuleBuilder()
        {
            return this.m_module;
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetNestedType(name, bindingAttr);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetNestedTypes(bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.GetProperties(bindingAttr);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int GetTokenFromSig(RuntimeModule module, byte[] signature, int sigLength);
        protected override bool HasElementTypeImpl()
        {
            return false;
        }

        [SecurityCritical]
        private void Init(string fullname, TypeAttributes attr, Type parent, Type[] interfaces, System.Reflection.Module module, System.Reflection.Emit.PackingSize iPackingSize, int iTypeSize, TypeBuilder enclosingType)
        {
            if (fullname == null)
            {
                throw new ArgumentNullException("fullname");
            }
            if (fullname.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "fullname");
            }
            if (fullname[0] == '\0')
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "fullname");
            }
            if (fullname.Length > 0x3ff)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TypeNameTooLong"), "fullname");
            }
            this.m_bIsGenTypeDef = false;
            this.m_bIsGenParam = false;
            this.m_hasBeenCreated = false;
            this.m_runtimeType = null;
            this.m_isHiddenGlobalType = false;
            this.m_isHiddenType = false;
            this.m_module = (ModuleBuilder) module;
            this.m_DeclaringType = enclosingType;
            AssemblyBuilder containingAssemblyBuilder = this.m_module.ContainingAssemblyBuilder;
            this.m_underlyingSystemType = null;
            containingAssemblyBuilder.m_assemblyData.CheckTypeNameConflict(fullname, enclosingType);
            if ((enclosingType != null) && (((attr & TypeAttributes.NestedFamORAssem) == TypeAttributes.Public) || ((attr & TypeAttributes.NestedFamORAssem) == TypeAttributes.AnsiClass)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadNestedTypeFlags"), "attr");
            }
            int[] interfaceTokens = null;
            if (interfaces != null)
            {
                int num;
                for (num = 0; num < interfaces.Length; num++)
                {
                    if (interfaces[num] == null)
                    {
                        throw new ArgumentNullException("interfaces");
                    }
                }
                interfaceTokens = new int[interfaces.Length + 1];
                for (num = 0; num < interfaces.Length; num++)
                {
                    interfaceTokens[num] = this.m_module.GetTypeTokenInternal(interfaces[num]).Token;
                }
            }
            int length = fullname.LastIndexOf('.');
            switch (length)
            {
                case -1:
                case 0:
                    this.m_strNameSpace = string.Empty;
                    this.m_strName = fullname;
                    break;

                default:
                    this.m_strNameSpace = fullname.Substring(0, length);
                    this.m_strName = fullname.Substring(length + 1);
                    break;
            }
            this.VerifyTypeAttributes(attr);
            this.m_iAttr = attr;
            this.SetParent(parent);
            this.m_listMethods = new List<MethodBuilder>();
            this.m_lastTokenizedMethod = -1;
            this.SetInterfaces(interfaces);
            this.m_constructorCount = 0;
            int tkParent = 0;
            if (this.m_typeParent != null)
            {
                tkParent = this.m_module.GetTypeTokenInternal(this.m_typeParent).Token;
            }
            int tkEnclosingType = 0;
            if (enclosingType != null)
            {
                tkEnclosingType = enclosingType.m_tdType.Token;
            }
            this.m_tdType = new System.Reflection.Emit.TypeToken(DefineType(this.m_module.GetNativeHandle(), fullname, tkParent, this.m_iAttr, tkEnclosingType, interfaceTokens));
            this.m_iPackingSize = iPackingSize;
            this.m_iTypeSize = iTypeSize;
            if ((this.m_iPackingSize != System.Reflection.Emit.PackingSize.Unspecified) || (this.m_iTypeSize != 0))
            {
                SetClassLayout(this.GetModuleBuilder().GetNativeHandle(), this.m_tdType.Token, this.m_iPackingSize, this.m_iTypeSize);
            }
            if (IsPublicComType(this))
            {
                if (containingAssemblyBuilder.IsPersistable() && !this.m_module.IsTransient())
                {
                    containingAssemblyBuilder.m_assemblyData.AddPublicComType(this);
                }
                if (!this.m_module.Equals(containingAssemblyBuilder.ManifestModule))
                {
                    containingAssemblyBuilder.DefineExportedTypeInMemory(this, this.m_module.m_moduleData.FileToken, this.m_tdType.Token);
                }
            }
        }

        private void InitAsGenericParam(string szName, int genParamPos)
        {
            this.m_strName = szName;
            this.m_genParamPos = genParamPos;
            this.m_bIsGenParam = true;
            this.m_bIsGenTypeDef = false;
            this.m_typeInterfaces = new List<Type>();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            return this.m_runtimeType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override bool IsArrayImpl()
        {
            return false;
        }

        public override bool IsAssignableFrom(Type c)
        {
            if (IsTypeEqual(c, this))
            {
                return true;
            }
            Type runtimeType = null;
            TypeBuilder builder = c as TypeBuilder;
            if (builder != null)
            {
                runtimeType = builder.m_runtimeType;
            }
            else
            {
                runtimeType = c;
            }
            if ((runtimeType != null) && runtimeType.IsRuntimeType)
            {
                if (this.m_runtimeType == null)
                {
                    return false;
                }
                return this.m_runtimeType.IsAssignableFrom(runtimeType);
            }
            if (builder != null)
            {
                if (builder.IsSubclassOf(this))
                {
                    return true;
                }
                if (!base.IsInterface)
                {
                    return false;
                }
                Type[] interfaces = builder.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (IsTypeEqual(interfaces[i], this))
                    {
                        return true;
                    }
                    if (interfaces[i].IsSubclassOf(this))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override bool IsByRefImpl()
        {
            return false;
        }

        protected override bool IsCOMObjectImpl()
        {
            if ((this.GetAttributeFlagsImpl() & TypeAttributes.Import) == TypeAttributes.AnsiClass)
            {
                return false;
            }
            return true;
        }

        public bool IsCreated()
        {
            return this.m_hasBeenCreated;
        }

        [SecuritySafeCritical]
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (!this.IsCreated())
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "caType");
            }
            return CustomAttribute.IsDefined(this.m_runtimeType, underlyingSystemType, inherit);
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        private static bool IsPublicComType(Type type)
        {
            Type declaringType = type.DeclaringType;
            if (declaringType != null)
            {
                if (IsPublicComType(declaringType) && ((type.Attributes & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedPublic))
                {
                    return true;
                }
            }
            else if ((type.Attributes & TypeAttributes.NestedFamORAssem) == TypeAttributes.Public)
            {
                return true;
            }
            return false;
        }

        [ComVisible(true)]
        public override bool IsSubclassOf(Type c)
        {
            Type type = this;
            if (!IsTypeEqual(type, c))
            {
                for (type = type.BaseType; type != null; type = type.BaseType)
                {
                    if (IsTypeEqual(type, c))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsTypeEqual(Type t1, Type t2)
        {
            if (t1 == t2)
            {
                return true;
            }
            TypeBuilder objA = null;
            TypeBuilder objB = null;
            Type runtimeType = null;
            Type type2 = null;
            if (t1 is TypeBuilder)
            {
                objA = (TypeBuilder) t1;
                runtimeType = objA.m_runtimeType;
            }
            else
            {
                runtimeType = t1;
            }
            if (t2 is TypeBuilder)
            {
                objB = (TypeBuilder) t2;
                type2 = objB.m_runtimeType;
            }
            else
            {
                type2 = t2;
            }
            return ((((objA != null) && (objB != null)) && object.ReferenceEquals(objA, objB)) || (((runtimeType != null) && (type2 != null)) && (runtimeType == type2)));
        }

        public override Type MakeArrayType()
        {
            return SymbolType.FormCompoundType("[]".ToCharArray(), this, 0);
        }

        public override Type MakeArrayType(int rank)
        {
            if (rank <= 0)
            {
                throw new IndexOutOfRangeException();
            }
            string str = "";
            if (rank == 1)
            {
                str = "*";
            }
            else
            {
                for (int i = 1; i < rank; i++)
                {
                    str = str + ",";
                }
            }
            return SymbolType.FormCompoundType(string.Format(CultureInfo.InvariantCulture, "[{0}]", new object[] { str }).ToCharArray(), this, 0);
        }

        public override Type MakeByRefType()
        {
            return SymbolType.FormCompoundType("&".ToCharArray(), this, 0);
        }

        [SecuritySafeCritical]
        public override Type MakeGenericType(params Type[] typeArguments)
        {
            this.CheckContext(typeArguments);
            return TypeBuilderInstantiation.MakeGenericType(this, typeArguments);
        }

        public override Type MakePointerType()
        {
            return SymbolType.FormCompoundType("*".ToCharArray(), this, 0);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetClassLayout(RuntimeModule module, int tk, System.Reflection.Emit.PackingSize iPackingSize, int iTypeSize);
        [SecurityCritical]
        internal static unsafe void SetConstantValue(ModuleBuilder module, int tk, Type destType, object value)
        {
            if (value != null)
            {
                Type c = value.GetType();
                if (destType.IsByRef)
                {
                    destType = destType.GetElementType();
                }
                if (destType.IsEnum)
                {
                    Type underlyingSystemType;
                    EnumBuilder builder = destType as EnumBuilder;
                    if (builder != null)
                    {
                        underlyingSystemType = builder.UnderlyingSystemType;
                        if ((c != builder.RuntimeEnumType) && (c != underlyingSystemType))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
                        }
                    }
                    else
                    {
                        TypeBuilder builder2 = destType as TypeBuilder;
                        if (builder2 != null)
                        {
                            underlyingSystemType = builder2.m_underlyingSystemType;
                            if ((underlyingSystemType == null) || ((c != builder2.UnderlyingSystemType) && (c != underlyingSystemType)))
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
                            }
                        }
                        else
                        {
                            underlyingSystemType = Enum.GetUnderlyingType(destType);
                            if ((c != destType) && (c != underlyingSystemType))
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
                            }
                        }
                    }
                    c = underlyingSystemType;
                }
                else if (!destType.IsAssignableFrom(c))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
                }
                CorElementType corElementType = RuntimeTypeHandle.GetCorElementType(c.GetTypeHandleInternal().GetRuntimeType());
                switch (corElementType)
                {
                    case CorElementType.Boolean:
                    case CorElementType.Char:
                    case CorElementType.I1:
                    case CorElementType.U1:
                    case CorElementType.I2:
                    case CorElementType.U2:
                    case CorElementType.I4:
                    case CorElementType.U4:
                    case CorElementType.I8:
                    case CorElementType.U8:
                    case CorElementType.R4:
                    case CorElementType.R8:
                        fixed (byte* numRef = &JitHelpers.GetPinningHelper(value).m_data)
                        {
                            SetConstantValue(module.GetNativeHandle(), tk, (int) corElementType, (void*) numRef);
                        }
                        return;
                }
                if (c == typeof(string))
                {
                    fixed (char* str = ((char*) ((string) value)))
                    {
                        char* chPtr = str;
                        SetConstantValue(module.GetNativeHandle(), tk, 14, (void*) chPtr);
                    }
                }
                else
                {
                    if (c != typeof(DateTime))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ConstantNotSupported", new object[] { c.ToString() }));
                    }
                    DateTime time = (DateTime) value;
                    long ticks = time.Ticks;
                    SetConstantValue(module.GetNativeHandle(), tk, 10, (void*) &ticks);
                }
            }
            else
            {
                if (destType.IsValueType)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ConstantNull"));
                }
                SetConstantValue(module.GetNativeHandle(), tk, 0x12, null);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern unsafe void SetConstantValue(RuntimeModule module, int tk, int corType, void* pValue);
        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            this.ThrowIfGeneric();
            customBuilder.CreateCustomAttribute(this.m_module, this.m_tdType.Token);
        }

        [SecuritySafeCritical, ComVisible(true)]
        public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }
            if (binaryAttribute == null)
            {
                throw new ArgumentNullException("binaryAttribute");
            }
            this.ThrowIfGeneric();
            DefineCustomAttribute(this.m_module, this.m_tdType.Token, this.m_module.GetConstructorToken(con).Token, binaryAttribute, false, false);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetFieldLayoutOffset(RuntimeModule module, int fdToken, int iOffset);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetFieldMarshal(RuntimeModule module, int tk, byte[] ubMarshal, int ubSize);
        internal void SetInterfaces(params Type[] interfaces)
        {
            this.ThrowIfCreated();
            this.m_typeInterfaces = new List<Type>();
            if (interfaces != null)
            {
                this.m_typeInterfaces.AddRange(interfaces);
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetMethodIL(RuntimeModule module, int tk, bool isInitLocals, byte[] body, int bodyLength, byte[] LocalSig, int sigLength, int maxStackSize, __ExceptionInstance[] exceptions, int numExceptions, int[] tokenFixups, int numTokenFixups, int[] rvaFixups, int numRvaFixups);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetMethodImpl(RuntimeModule module, int tkMethod, MethodImplAttributes MethodImplAttributes);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int SetParamInfo(RuntimeModule module, int tkMethod, int iSequence, ParameterAttributes iParamAttributes, string strParamName);
        [SecuritySafeCritical]
        public void SetParent(Type parent)
        {
            this.ThrowIfGeneric();
            this.ThrowIfCreated();
            if (parent != null)
            {
                this.CheckContext(new Type[] { parent });
                if (parent.IsInterface)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_CannotSetParentToInterface"));
                }
                this.m_typeParent = parent;
            }
            else if ((this.m_iAttr & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
            {
                this.m_typeParent = typeof(object);
            }
            else
            {
                if ((this.m_iAttr & TypeAttributes.Abstract) == TypeAttributes.AnsiClass)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadInterfaceNotAbstract"));
                }
                this.m_typeParent = null;
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetParentType(RuntimeModule module, int tdTypeDef, int tkParent);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetPInvokeData(RuntimeModule module, string DllName, string name, int token, int linkFlags);
        void _TypeBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _TypeBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _TypeBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _TypeBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void TermCreateClass(RuntimeModule module, int tk, ObjectHandleOnStack type);
        internal void ThrowIfCreated()
        {
            if (this.IsCreated())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TypeHasBeenCreated"));
            }
        }

        internal void ThrowIfGeneric()
        {
            if (this.IsGenericType && !this.IsGenericTypeDefinition)
            {
                throw new InvalidOperationException();
            }
        }

        public override string ToString()
        {
            return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.ToString);
        }

        private void VerifyTypeAttributes(TypeAttributes attr)
        {
            if (this.DeclaringType == null)
            {
                if (((attr & TypeAttributes.NestedFamORAssem) != TypeAttributes.AnsiClass) && ((attr & TypeAttributes.NestedFamORAssem) != TypeAttributes.Public))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrNestedVisibilityOnNonNestedType"));
                }
            }
            else if (((attr & TypeAttributes.NestedFamORAssem) == TypeAttributes.AnsiClass) || ((attr & TypeAttributes.NestedFamORAssem) == TypeAttributes.Public))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrNonNestedVisibilityNestedType"));
            }
            if ((((attr & TypeAttributes.LayoutMask) != TypeAttributes.AnsiClass) && ((attr & TypeAttributes.LayoutMask) != TypeAttributes.SequentialLayout)) && ((attr & TypeAttributes.LayoutMask) != TypeAttributes.ExplicitLayout))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrInvalidLayout"));
            }
            if ((attr & TypeAttributes.ReservedMask) != TypeAttributes.AnsiClass)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrReservedBitsSet"));
            }
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.m_module.Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.AssemblyQualifiedName);
            }
        }

        public override Type BaseType
        {
            get
            {
                return this.m_typeParent;
            }
        }

        public override MethodBase DeclaringMethod
        {
            get
            {
                return this.m_declMeth;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_DeclaringType;
            }
        }

        public override string FullName
        {
            get
            {
                if (this.m_strFullQualName == null)
                {
                    this.m_strFullQualName = TypeNameBuilder.ToString(this, TypeNameBuilder.Format.FullName);
                }
                return this.m_strFullQualName;
            }
        }

        public override System.Reflection.GenericParameterAttributes GenericParameterAttributes
        {
            get
            {
                return this.m_genParamAttributes;
            }
        }

        public override int GenericParameterPosition
        {
            get
            {
                return this.m_genParamPos;
            }
        }

        public override Guid GUID
        {
            get
            {
                if (!this.IsCreated())
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
                }
                return this.m_runtimeType.GUID;
            }
        }

        public override bool IsGenericParameter
        {
            get
            {
                return this.m_bIsGenParam;
            }
        }

        public override bool IsGenericType
        {
            get
            {
                return (this.m_inst != null);
            }
        }

        public override bool IsGenericTypeDefinition
        {
            get
            {
                return this.m_bIsGenTypeDef;
            }
        }

        public override bool IsSecurityCritical
        {
            get
            {
                if (!this.IsCreated())
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
                }
                return this.m_runtimeType.IsSecurityCritical;
            }
        }

        public override bool IsSecuritySafeCritical
        {
            get
            {
                if (!this.IsCreated())
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
                }
                return this.m_runtimeType.IsSecuritySafeCritical;
            }
        }

        public override bool IsSecurityTransparent
        {
            get
            {
                if (!this.IsCreated())
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_TypeNotYetCreated"));
                }
                return this.m_runtimeType.IsSecurityTransparent;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                return this.m_tdType.Token;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.GetModuleBuilder();
            }
        }

        public override string Name
        {
            get
            {
                return this.m_strName;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.m_strNameSpace;
            }
        }

        public System.Reflection.Emit.PackingSize PackingSize
        {
            get
            {
                return this.m_iPackingSize;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_DeclaringType;
            }
        }

        public int Size
        {
            get
            {
                return this.m_iTypeSize;
            }
        }

        internal object SyncRoot
        {
            get
            {
                return this.m_module.SyncRoot;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
            }
        }

        public System.Reflection.Emit.TypeToken TypeToken
        {
            get
            {
                if (this.IsGenericParameter)
                {
                    this.ThrowIfCreated();
                }
                return this.m_tdType;
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                if (this.m_runtimeType != null)
                {
                    return this.m_runtimeType.UnderlyingSystemType;
                }
                if (!this.IsEnum)
                {
                    return this;
                }
                if (this.m_underlyingSystemType == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoUnderlyingTypeOnEnum"));
                }
                return this.m_underlyingSystemType;
            }
        }

        internal class CustAttr
        {
            private byte[] m_binaryAttribute;
            private ConstructorInfo m_con;
            private CustomAttributeBuilder m_customBuilder;

            public CustAttr(CustomAttributeBuilder customBuilder)
            {
                if (customBuilder == null)
                {
                    throw new ArgumentNullException("customBuilder");
                }
                this.m_customBuilder = customBuilder;
            }

            public CustAttr(ConstructorInfo con, byte[] binaryAttribute)
            {
                if (con == null)
                {
                    throw new ArgumentNullException("con");
                }
                if (binaryAttribute == null)
                {
                    throw new ArgumentNullException("binaryAttribute");
                }
                this.m_con = con;
                this.m_binaryAttribute = binaryAttribute;
            }

            [SecurityCritical]
            public void Bake(ModuleBuilder module, int token)
            {
                if (this.m_customBuilder == null)
                {
                    TypeBuilder.DefineCustomAttribute(module, token, module.GetConstructorToken(this.m_con).Token, this.m_binaryAttribute, false, false);
                }
                else
                {
                    this.m_customBuilder.CreateCustomAttribute(module, token);
                }
            }
        }
    }
}

