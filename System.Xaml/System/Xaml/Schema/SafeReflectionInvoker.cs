namespace System.Xaml.Schema
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    internal static class SafeReflectionInvoker
    {
        private static object lockObject = new object();
        private static CreateDelegate1Delegate s_CreateDelegate1;
        private static CreateDelegate2Delegate s_CreateDelegate2;
        private static CreateInstanceDelegate s_CreateInstance;
        private static InvokeMethodDelegate s_InvokeMethod;
        [SecurityCritical]
        private static ReflectionPermission s_reflectionMemberAccess;
        private static bool s_UseDynamicAssembly = false;
        private static readonly Assembly SystemXaml = typeof(SafeReflectionInvoker).Assembly;

        [SecuritySafeCritical]
        internal static Delegate CreateDelegate(Type delegateType, object target, string methodName)
        {
            if (!UseDynamicAssembly())
            {
                return CreateDelegateCritical(delegateType, target, methodName);
            }
            return s_CreateDelegate2(delegateType, target, methodName);
        }

        [SecuritySafeCritical]
        internal static Delegate CreateDelegate(Type delegateType, Type targetType, string methodName)
        {
            if (!UseDynamicAssembly())
            {
                return CreateDelegateCritical(delegateType, targetType, methodName);
            }
            return s_CreateDelegate1(delegateType, targetType, methodName);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static Delegate CreateDelegateCritical(Type delegateType, object target, string methodName)
        {
            return Delegate.CreateDelegate(delegateType, target, methodName);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static Delegate CreateDelegateCritical(Type delegateType, Type targetType, string methodName)
        {
            return Delegate.CreateDelegate(delegateType, targetType, methodName);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining), SecurityCritical]
        private static void CreateDynamicAssembly()
        {
            new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Assert();
            Type[] types = new Type[] { typeof(Type), typeof(Type), typeof(string) };
            MethodInfo methodInfo = typeof(Delegate).GetMethod("CreateDelegate", types);
            DynamicMethod method = new DynamicMethod("CreateDelegate", typeof(Delegate), types);
            method.DefineParameter(1, ParameterAttributes.In, "delegateType");
            method.DefineParameter(2, ParameterAttributes.In, "targetType");
            method.DefineParameter(3, ParameterAttributes.In, "methodName");
            ILGenerator iLGenerator = method.GetILGenerator(5);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
            iLGenerator.Emit(OpCodes.Ret);
            s_CreateDelegate1 = (CreateDelegate1Delegate) method.CreateDelegate(typeof(CreateDelegate1Delegate));
            types = new Type[] { typeof(Type), typeof(object), typeof(string) };
            methodInfo = typeof(Delegate).GetMethod("CreateDelegate", types);
            method = new DynamicMethod("CreateDelegate", typeof(Delegate), types);
            method.DefineParameter(1, ParameterAttributes.In, "delegateType");
            method.DefineParameter(2, ParameterAttributes.In, "target");
            method.DefineParameter(3, ParameterAttributes.In, "methodName");
            iLGenerator = method.GetILGenerator(5);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
            iLGenerator.Emit(OpCodes.Ret);
            s_CreateDelegate2 = (CreateDelegate2Delegate) method.CreateDelegate(typeof(CreateDelegate2Delegate));
            types = new Type[] { typeof(Type), typeof(object[]) };
            methodInfo = typeof(Activator).GetMethod("CreateInstance", types);
            method = new DynamicMethod("CreateInstance", typeof(object), types);
            method.DefineParameter(1, ParameterAttributes.In, "type");
            method.DefineParameter(2, ParameterAttributes.In, "arguments");
            iLGenerator = method.GetILGenerator(4);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
            iLGenerator.Emit(OpCodes.Ret);
            s_CreateInstance = (CreateInstanceDelegate) method.CreateDelegate(typeof(CreateInstanceDelegate));
            types = new Type[] { typeof(object), typeof(object[]) };
            Type[] parameterTypes = new Type[] { typeof(MethodInfo), typeof(object), typeof(object[]) };
            methodInfo = typeof(MethodInfo).GetMethod("Invoke", types);
            method = new DynamicMethod("InvokeMethod", typeof(object), parameterTypes);
            method.DefineParameter(1, ParameterAttributes.In, "method");
            method.DefineParameter(2, ParameterAttributes.In, "instance");
            method.DefineParameter(3, ParameterAttributes.In, "args");
            iLGenerator = method.GetILGenerator(5);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
            iLGenerator.Emit(OpCodes.Ret);
            s_InvokeMethod = (InvokeMethodDelegate) method.CreateDelegate(typeof(InvokeMethodDelegate));
        }

        [SecuritySafeCritical]
        internal static object CreateInstance(Type type, object[] arguments)
        {
            if (!UseDynamicAssembly())
            {
                return CreateInstanceCritical(type, arguments);
            }
            return s_CreateInstance(type, arguments);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static object CreateInstanceCritical(Type type, object[] arguments)
        {
            return Activator.CreateInstance(type, arguments);
        }

        [SecuritySafeCritical]
        internal static void DemandMemberAccessPermission()
        {
            if (s_reflectionMemberAccess == null)
            {
                s_reflectionMemberAccess = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
            }
            s_reflectionMemberAccess.Demand();
        }

        [SecuritySafeCritical]
        internal static object InvokeMethod(MethodInfo method, object instance, object[] args)
        {
            if (!UseDynamicAssembly())
            {
                return InvokeMethodCritical(method, instance, args);
            }
            return s_InvokeMethod(method, instance, args);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static object InvokeMethodCritical(MethodInfo method, object instance, object[] args)
        {
            return method.Invoke(instance, args);
        }

        [SecuritySafeCritical]
        public static bool IsInSystemXaml(Type type)
        {
            if (type.Assembly == SystemXaml)
            {
                return true;
            }
            if (type.IsGenericType)
            {
                foreach (Type type2 in type.GetGenericArguments())
                {
                    if (IsInSystemXaml(type2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        internal static bool IsSystemXamlNonPublic(MethodInfo method)
        {
            Type declaringType = method.DeclaringType;
            if (IsInSystemXaml(declaringType) && (!method.IsPublic || !declaringType.IsVisible))
            {
                return true;
            }
            if (method.IsGenericMethod)
            {
                foreach (Type type2 in method.GetGenericArguments())
                {
                    if (IsInSystemXaml(type2) && !type2.IsVisible)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecurityCritical]
        private static bool UseDynamicAssembly()
        {
            if (!s_UseDynamicAssembly)
            {
                bool flag = false;
                try
                {
                    new PermissionSet(PermissionState.Unrestricted).Demand();
                }
                catch (SecurityException)
                {
                    flag = true;
                }
                if (flag)
                {
                    lock (lockObject)
                    {
                        if (!s_UseDynamicAssembly)
                        {
                            CreateDynamicAssembly();
                            s_UseDynamicAssembly = true;
                        }
                    }
                }
            }
            return s_UseDynamicAssembly;
        }

        private delegate Delegate CreateDelegate1Delegate(Type delegateType, Type targetType, string methodName);

        private delegate Delegate CreateDelegate2Delegate(Type delegateType, object target, string methodName);

        private delegate object CreateInstanceDelegate(Type type, object[] arguments);

        private delegate object InvokeMethodDelegate(MethodInfo method, object instance, object[] args);
    }
}

