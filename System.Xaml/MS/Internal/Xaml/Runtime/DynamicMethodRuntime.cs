namespace MS.Internal.Xaml.Runtime
{
    using MS.Internal.Xaml;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Xaml;
    using System.Xaml.Permissions;
    using System.Xaml.Schema;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal class DynamicMethodRuntime : ClrObjectRuntime
    {
        private Dictionary<Type, object> _converterInstances;
        private Dictionary<Type, DelegateCreator> _delegateCreators;
        private DelegateCreator _delegateCreatorWithoutHelper;
        private Dictionary<MethodBase, FactoryDelegate> _factoryDelegates;
        private Assembly _localAssembly;
        private Type _localType;
        private Dictionary<MethodInfo, PropertyGetDelegate> _propertyGetDelegates;
        private Dictionary<MethodInfo, PropertySetDelegate> _propertySetDelegates;
        private XamlSchemaContext _schemaContext;
        private XamlLoadPermission _xamlLoadPermission;
        private const BindingFlags BF_AllInstanceMembers = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private const BindingFlags BF_AllStaticMembers = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static PermissionSet s_FullTrustPermission;
        private static MethodInfo s_GetTypeFromHandleMethod;
        private static MethodInfo s_InvokeMemberMethod;

        [SecuritySafeCritical]
        internal DynamicMethodRuntime(XamlRuntimeSettings settings, XamlSchemaContext schemaContext, XamlAccessLevel accessLevel) : base(settings, true)
        {
            this._schemaContext = schemaContext;
            this._xamlLoadPermission = new XamlLoadPermission(accessLevel);
            this._localAssembly = Assembly.Load(accessLevel.AssemblyAccessToAssemblyName);
            if (accessLevel.PrivateAccessToTypeName != null)
            {
                this._localType = this._localAssembly.GetType(accessLevel.PrivateAccessToTypeName, true);
            }
        }

        [SecuritySafeCritical]
        protected override Delegate CreateDelegate(Type delegateType, object target, string methodName)
        {
            DelegateCreator creator;
            this.DemandXamlLoadPermission();
            Type key = target.GetType();
            if (!this.DelegateCreators.TryGetValue(key, out creator))
            {
                creator = this.CreateDelegateCreator(key);
                this.DelegateCreators.Add(key, creator);
            }
            return creator(delegateType, target, methodName);
        }

        private DelegateCreator CreateDelegateCreator(Type targetType)
        {
            if (targetType.GetMethod("_CreateDelegate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { typeof(Type), typeof(string) }, null) == null)
            {
                if (this._delegateCreatorWithoutHelper == null)
                {
                    this._delegateCreatorWithoutHelper = this.CreateDelegateCreatorWithoutHelper();
                }
                return this._delegateCreatorWithoutHelper;
            }
            DynamicMethod method = this.CreateDynamicMethod(targetType.Name + "DelegateHelper", typeof(Delegate), new Type[] { typeof(Type), typeof(object), typeof(string) });
            ILGenerator iLGenerator = method.GetILGenerator();
            short[] paramArgNums = new short[2];
            paramArgNums[1] = 2;
            this.Emit_LateBoundInvoke(iLGenerator, targetType, "_CreateDelegate", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, 1, paramArgNums);
            Emit_CastTo(iLGenerator, typeof(Delegate));
            iLGenerator.Emit(OpCodes.Ret);
            return (DelegateCreator) method.CreateDelegate(typeof(DelegateCreator));
        }

        private DelegateCreator CreateDelegateCreatorWithoutHelper()
        {
            DynamicMethod method = this.CreateDynamicMethod("CreateDelegateHelper", typeof(Delegate), new Type[] { typeof(Type), typeof(object), typeof(string) });
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            MethodInfo meth = typeof(Delegate).GetMethod("CreateDelegate", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type), typeof(object), typeof(string) }, null);
            iLGenerator.Emit(OpCodes.Call, meth);
            iLGenerator.Emit(OpCodes.Ret);
            return (DelegateCreator) method.CreateDelegate(typeof(DelegateCreator));
        }

        private DynamicMethod CreateDynamicMethod(string name, Type returnType, params Type[] argTypes)
        {
            DynamicMethod method;
            if (s_FullTrustPermission == null)
            {
                s_FullTrustPermission = new PermissionSet(PermissionState.Unrestricted);
            }
            s_FullTrustPermission.Assert();
            try
            {
                if (this._localType != null)
                {
                    return new DynamicMethod(name, returnType, argTypes, this._localType);
                }
                method = new DynamicMethod(name, returnType, argTypes, this._localAssembly.ManifestModule);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return method;
        }

        private FactoryDelegate CreateFactoryDelegate(ConstructorInfo ctor)
        {
            DynamicMethod method = this.CreateDynamicMethod(ctor.DeclaringType.Name + "Ctor", typeof(object), new Type[] { typeof(object[]) });
            ILGenerator iLGenerator = method.GetILGenerator();
            LocalBuilder[] locals = this.LoadArguments(iLGenerator, ctor);
            iLGenerator.Emit(OpCodes.Newobj, ctor);
            this.UnloadArguments(iLGenerator, locals);
            iLGenerator.Emit(OpCodes.Ret);
            return (FactoryDelegate) method.CreateDelegate(typeof(FactoryDelegate));
        }

        private FactoryDelegate CreateFactoryDelegate(MethodInfo factory)
        {
            DynamicMethod method = this.CreateDynamicMethod(factory.Name + "Factory", typeof(object), new Type[] { typeof(object[]) });
            ILGenerator iLGenerator = method.GetILGenerator();
            LocalBuilder[] locals = this.LoadArguments(iLGenerator, factory);
            iLGenerator.Emit(OpCodes.Call, factory);
            Emit_BoxIfValueType(iLGenerator, factory.ReturnType);
            this.UnloadArguments(iLGenerator, locals);
            iLGenerator.Emit(OpCodes.Ret);
            return (FactoryDelegate) method.CreateDelegate(typeof(FactoryDelegate));
        }

        [SecuritySafeCritical]
        public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
        {
            if (ts == BuiltInValueConverter.Event)
            {
                string methodName = value as string;
                if (methodName != null)
                {
                    object obj2;
                    Type type;
                    EventConverter.GetRootObjectAndDelegateType(serviceContext, out obj2, out type);
                    return this.CreateDelegate(type, obj2, methodName);
                }
            }
            return base.CreateFromValue(serviceContext, ts, value, property);
        }

        private PropertyGetDelegate CreateGetDelegate(MethodInfo getter)
        {
            DynamicMethod method = this.CreateDynamicMethod(getter.Name + "Getter", typeof(object), new Type[] { typeof(object) });
            ILGenerator iLGenerator = method.GetILGenerator();
            Type toType = getter.IsStatic ? getter.GetParameters()[0].ParameterType : this.GetTargetType(getter);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            Emit_CastTo(iLGenerator, toType);
            Emit_Call(iLGenerator, getter);
            Emit_BoxIfValueType(iLGenerator, getter.ReturnType);
            iLGenerator.Emit(OpCodes.Ret);
            return (PropertyGetDelegate) method.CreateDelegate(typeof(PropertyGetDelegate));
        }

        private object CreateInstanceWithCtor(Type type, object[] args)
        {
            ConstructorInfo key = null;
            FactoryDelegate delegate2;
            if ((args == null) || (args.Length == 0))
            {
                key = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            }
            if (key == null)
            {
                ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                key = (ConstructorInfo) base.BindToMethod(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, constructors, args);
            }
            if (!this.FactoryDelegates.TryGetValue(key, out delegate2))
            {
                delegate2 = this.CreateFactoryDelegate(key);
                this.FactoryDelegates.Add(key, delegate2);
            }
            return delegate2(args);
        }

        [SecuritySafeCritical]
        protected override object CreateInstanceWithCtor(XamlType xamlType, object[] args)
        {
            this.DemandXamlLoadPermission();
            return this.CreateInstanceWithCtor(xamlType.UnderlyingType, args);
        }

        private PropertySetDelegate CreateSetDelegate(MethodInfo setter)
        {
            DynamicMethod method = this.CreateDynamicMethod(setter.Name + "Setter", typeof(void), new Type[] { typeof(object), typeof(object) });
            ILGenerator iLGenerator = method.GetILGenerator();
            ParameterInfo[] parameters = setter.GetParameters();
            Type toType = setter.IsStatic ? parameters[0].ParameterType : this.GetTargetType(setter);
            Type type2 = setter.IsStatic ? parameters[1].ParameterType : parameters[0].ParameterType;
            iLGenerator.Emit(OpCodes.Ldarg_0);
            Emit_CastTo(iLGenerator, toType);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            Emit_CastTo(iLGenerator, type2);
            Emit_Call(iLGenerator, setter);
            iLGenerator.Emit(OpCodes.Ret);
            return (PropertySetDelegate) method.CreateDelegate(typeof(PropertySetDelegate));
        }

        private void DemandXamlLoadPermission()
        {
            this._xamlLoadPermission.Demand();
        }

        private static void Emit_BoxIfValueType(ILGenerator ilGenerator, Type type)
        {
            if (type.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, type);
            }
        }

        private static void Emit_Call(ILGenerator ilGenerator, MethodInfo method)
        {
            OpCode opcode = (method.IsStatic || method.DeclaringType.IsValueType) ? OpCodes.Call : OpCodes.Callvirt;
            ilGenerator.Emit(opcode, method);
        }

        private static void Emit_CastTo(ILGenerator ilGenerator, Type toType)
        {
            if (toType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox_Any, toType);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Castclass, toType);
            }
        }

        private static void Emit_ConstInt(ILGenerator ilGenerator, int value)
        {
            switch (value)
            {
                case -1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            ilGenerator.Emit(OpCodes.Ldc_I4, value);
        }

        private void Emit_LateBoundInvoke(ILGenerator ilGenerator, Type targetType, string methodName, BindingFlags bindingFlags, short targetArgNum, params short[] paramArgNums)
        {
            this.Emit_TypeOf(ilGenerator, targetType);
            ilGenerator.Emit(OpCodes.Ldstr, methodName);
            Emit_ConstInt(ilGenerator, (int) bindingFlags);
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Ldarg, targetArgNum);
            LocalBuilder local = ilGenerator.DeclareLocal(typeof(object[]));
            Emit_ConstInt(ilGenerator, paramArgNums.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            ilGenerator.Emit(OpCodes.Stloc, local);
            for (int i = 0; i < paramArgNums.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldloc, local);
                Emit_ConstInt(ilGenerator, i);
                ilGenerator.Emit(OpCodes.Ldarg, paramArgNums[i]);
                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }
            ilGenerator.Emit(OpCodes.Ldloc, local);
            if (s_InvokeMemberMethod == null)
            {
                s_InvokeMemberMethod = typeof(Type).GetMethod("InvokeMember", new Type[] { typeof(string), typeof(BindingFlags), typeof(Binder), typeof(object), typeof(object[]) });
            }
            ilGenerator.Emit(OpCodes.Callvirt, s_InvokeMemberMethod);
        }

        private void Emit_TypeOf(ILGenerator ilGenerator, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            if (s_GetTypeFromHandleMethod == null)
            {
                s_GetTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(RuntimeTypeHandle) }, null);
            }
            ilGenerator.Emit(OpCodes.Call, s_GetTypeFromHandleMethod);
        }

        [SecuritySafeCritical]
        public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> ts) where TConverterBase: class
        {
            object obj2;
            this.DemandXamlLoadPermission();
            Type converterType = ts.ConverterType;
            if (converterType == null)
            {
                return default(TConverterBase);
            }
            if (!this.ConverterInstances.TryGetValue(converterType, out obj2))
            {
                obj2 = this.CreateInstanceWithCtor(converterType, null);
                this.ConverterInstances.Add(converterType, obj2);
            }
            return (TConverterBase) obj2;
        }

        private Type GetTargetType(MethodInfo instanceMethod)
        {
            Type declaringType = instanceMethod.DeclaringType;
            if (((this._localType != null) && (this._localType != declaringType)) && declaringType.IsAssignableFrom(this._localType))
            {
                if (instanceMethod.IsFamily || instanceMethod.IsFamilyAndAssembly)
                {
                    return this._localType;
                }
                if (instanceMethod.IsFamilyOrAssembly && !this._schemaContext.AreInternalsVisibleTo(declaringType.Assembly, this._localType.Assembly))
                {
                    return this._localType;
                }
            }
            return declaringType;
        }

        [SecuritySafeCritical]
        protected override object GetValue(XamlMember member, object obj)
        {
            PropertyGetDelegate delegate2;
            this.DemandXamlLoadPermission();
            MethodInfo underlyingGetter = member.Invoker.UnderlyingGetter;
            if (underlyingGetter == null)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("CantGetWriteonlyProperty", new object[] { member }));
            }
            if (!this.PropertyGetDelegates.TryGetValue(underlyingGetter, out delegate2))
            {
                delegate2 = this.CreateGetDelegate(underlyingGetter);
                this.PropertyGetDelegates.Add(underlyingGetter, delegate2);
            }
            return delegate2(obj);
        }

        [SecuritySafeCritical]
        protected override object InvokeFactoryMethod(Type type, string methodName, object[] args)
        {
            FactoryDelegate delegate2;
            this.DemandXamlLoadPermission();
            MethodInfo key = base.GetFactoryMethod(type, methodName, args, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (!this.FactoryDelegates.TryGetValue(key, out delegate2))
            {
                delegate2 = this.CreateFactoryDelegate(key);
                this.FactoryDelegates.Add(key, delegate2);
            }
            return delegate2(args);
        }

        private LocalBuilder[] LoadArguments(ILGenerator ilGenerator, MethodBase method)
        {
            if (method.GetParameters().Length == 0)
            {
                return null;
            }
            ParameterInfo[] parameters = method.GetParameters();
            Type[] typeArray = new Type[parameters.Length];
            LocalBuilder[] builderArray = new LocalBuilder[typeArray.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                ilGenerator.Emit(OpCodes.Ldarg_0);
                Emit_ConstInt(ilGenerator, i);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                if (parameterType.IsByRef)
                {
                    Type elementType = parameterType.GetElementType();
                    Emit_CastTo(ilGenerator, elementType);
                    builderArray[i] = ilGenerator.DeclareLocal(elementType);
                    ilGenerator.Emit(OpCodes.Stloc, builderArray[i]);
                    ilGenerator.Emit(OpCodes.Ldloca_S, builderArray[i]);
                }
                else
                {
                    Emit_CastTo(ilGenerator, parameterType);
                }
            }
            return builderArray;
        }

        [SecuritySafeCritical]
        protected override void SetValue(XamlMember member, object obj, object value)
        {
            PropertySetDelegate delegate2;
            this.DemandXamlLoadPermission();
            MethodInfo underlyingSetter = member.Invoker.UnderlyingSetter;
            if (underlyingSetter == null)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("CantSetReadonlyProperty", new object[] { member }));
            }
            if (!this.PropertySetDelegates.TryGetValue(underlyingSetter, out delegate2))
            {
                delegate2 = this.CreateSetDelegate(underlyingSetter);
                this.PropertySetDelegates.Add(underlyingSetter, delegate2);
            }
            delegate2(obj, value);
        }

        private void UnloadArguments(ILGenerator ilGenerator, LocalBuilder[] locals)
        {
            if (locals != null)
            {
                for (int i = 0; i < locals.Length; i++)
                {
                    if (locals[i] != null)
                    {
                        ilGenerator.Emit(OpCodes.Ldarg_0);
                        Emit_ConstInt(ilGenerator, i);
                        ilGenerator.Emit(OpCodes.Ldloc, locals[i]);
                        Emit_BoxIfValueType(ilGenerator, locals[i].LocalType);
                        ilGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }
        }

        private Dictionary<Type, object> ConverterInstances
        {
            get
            {
                if (this._converterInstances == null)
                {
                    this._converterInstances = new Dictionary<Type, object>();
                }
                return this._converterInstances;
            }
        }

        private Dictionary<Type, DelegateCreator> DelegateCreators
        {
            get
            {
                if (this._delegateCreators == null)
                {
                    this._delegateCreators = new Dictionary<Type, DelegateCreator>();
                }
                return this._delegateCreators;
            }
        }

        private Dictionary<MethodBase, FactoryDelegate> FactoryDelegates
        {
            get
            {
                if (this._factoryDelegates == null)
                {
                    this._factoryDelegates = new Dictionary<MethodBase, FactoryDelegate>();
                }
                return this._factoryDelegates;
            }
        }

        private Dictionary<MethodInfo, PropertyGetDelegate> PropertyGetDelegates
        {
            get
            {
                if (this._propertyGetDelegates == null)
                {
                    this._propertyGetDelegates = new Dictionary<MethodInfo, PropertyGetDelegate>();
                }
                return this._propertyGetDelegates;
            }
        }

        private Dictionary<MethodInfo, PropertySetDelegate> PropertySetDelegates
        {
            get
            {
                if (this._propertySetDelegates == null)
                {
                    this._propertySetDelegates = new Dictionary<MethodInfo, PropertySetDelegate>();
                }
                return this._propertySetDelegates;
            }
        }

        private delegate Delegate DelegateCreator(Type delegateType, object target, string methodName);

        private delegate object FactoryDelegate(object[] args);

        private delegate object PropertyGetDelegate(object target);

        private delegate void PropertySetDelegate(object target, object value);
    }
}

