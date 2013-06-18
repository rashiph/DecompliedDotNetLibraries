namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.IO.IsolatedStorage;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    public abstract class MethodInvoker
    {
        private static int count = 0;
        private static SimpleHashtable invokerFor = new SimpleHashtable(0x40);

        protected MethodInvoker()
        {
        }

        private static bool DoesCallerRequireFullTrust(MethodInfo method)
        {
            Assembly target = method.DeclaringType.Assembly;
            new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
            byte[] publicKey = target.GetName().GetPublicKey();
            if ((publicKey == null) || (publicKey.Length == 0))
            {
                return false;
            }
            if (Microsoft.JScript.CustomAttribute.GetCustomAttributes(target, typeof(AllowPartiallyTrustedCallersAttribute), true).Length != 0)
            {
                return false;
            }
            return true;
        }

        internal static MethodInvoker GetInvokerFor(MethodInfo method)
        {
            if ((method.DeclaringType == typeof(CodeAccessPermission)) && (((method.Name == "Deny") || (method.Name == "Assert")) || (method.Name == "PermitOnly")))
            {
                throw new JScriptException(JSError.CannotCallSecurityMethodLateBound);
            }
            MethodInvoker invoker = invokerFor[method] as MethodInvoker;
            if (invoker == null)
            {
                if (!SafeToCall(method))
                {
                    return null;
                }
                bool requiresDemand = DoesCallerRequireFullTrust(method);
                lock (invokerFor)
                {
                    invoker = invokerFor[method] as MethodInvoker;
                    if (invoker != null)
                    {
                        return invoker;
                    }
                    invoker = SpitAndInstantiateClassFor(method, requiresDemand);
                    invokerFor[method] = invoker;
                }
            }
            return invoker;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public abstract object Invoke(object thisob, object[] parameters);
        private static bool SafeToCall(MethodInfo meth)
        {
            Type declaringType = meth.DeclaringType;
            return ((((((declaringType != null) && (declaringType != typeof(Activator))) && ((declaringType != typeof(AppDomain)) && (declaringType != typeof(IsolatedStorageFile)))) && (((declaringType != typeof(MethodRental)) && (declaringType != typeof(TypeLibConverter))) && ((declaringType != typeof(SecurityManager)) && !typeof(Assembly).IsAssignableFrom(declaringType)))) && (((!typeof(MemberInfo).IsAssignableFrom(declaringType) && !typeof(ResourceManager).IsAssignableFrom(declaringType)) && (!typeof(Delegate).IsAssignableFrom(declaringType) && ((declaringType.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.AnsiClass))) && ((meth.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.PrivateScope))) && ((meth.Attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PrivateScope));
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        private static MethodInvoker SpitAndInstantiateClassFor(MethodInfo method, bool requiresDemand)
        {
            TypeBuilder builder = Runtime.ThunkModuleBuilder.DefineType("invoker" + count++, TypeAttributes.Public, typeof(MethodInvoker));
            MethodBuilder builder2 = builder.DefineMethod("Invoke", MethodAttributes.Virtual | MethodAttributes.Public, typeof(object), new Type[] { typeof(object), typeof(object[]) });
            if (requiresDemand)
            {
                builder2.AddDeclarativeSecurity(SecurityAction.Demand, new NamedPermissionSet("FullTrust"));
            }
            builder2.SetCustomAttribute(new CustomAttributeBuilder(Runtime.TypeRefs.debuggerStepThroughAttributeCtor, new object[0]));
            builder2.SetCustomAttribute(new CustomAttributeBuilder(Runtime.TypeRefs.debuggerHiddenAttributeCtor, new object[0]));
            ILGenerator iLGenerator = builder2.GetILGenerator();
            if (!method.DeclaringType.IsPublic)
            {
                method = method.GetBaseDefinition();
            }
            Type declaringType = method.DeclaringType;
            if (!method.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                if (declaringType.IsValueType)
                {
                    Microsoft.JScript.Convert.EmitUnbox(iLGenerator, declaringType, Type.GetTypeCode(declaringType));
                    Microsoft.JScript.Convert.EmitLdloca(iLGenerator, declaringType);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Castclass, declaringType);
                }
            }
            ParameterInfo[] parameters = method.GetParameters();
            LocalBuilder[] builderArray = null;
            int i = 0;
            int length = parameters.Length;
            while (i < length)
            {
                iLGenerator.Emit(OpCodes.Ldarg_2);
                ConstantWrapper.TranslateToILInt(iLGenerator, i);
                Type parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                    if (builderArray == null)
                    {
                        builderArray = new LocalBuilder[length];
                    }
                    builderArray[i] = iLGenerator.DeclareLocal(parameterType);
                    iLGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (parameterType.IsValueType)
                    {
                        Microsoft.JScript.Convert.EmitUnbox(iLGenerator, parameterType, Type.GetTypeCode(parameterType));
                    }
                    iLGenerator.Emit(OpCodes.Stloc, builderArray[i]);
                    iLGenerator.Emit(OpCodes.Ldloca, builderArray[i]);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (parameterType.IsValueType)
                    {
                        Microsoft.JScript.Convert.EmitUnbox(iLGenerator, parameterType, Type.GetTypeCode(parameterType));
                    }
                }
                i++;
            }
            if (((!method.IsStatic && method.IsVirtual) && !method.IsFinal) && (!declaringType.IsSealed || !declaringType.IsValueType))
            {
                iLGenerator.Emit(OpCodes.Callvirt, method);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Call, method);
            }
            Type returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else if (returnType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, returnType);
            }
            if (builderArray != null)
            {
                int index = 0;
                int num4 = parameters.Length;
                while (index < num4)
                {
                    LocalBuilder local = builderArray[index];
                    if (local != null)
                    {
                        iLGenerator.Emit(OpCodes.Ldarg_2);
                        ConstantWrapper.TranslateToILInt(iLGenerator, index);
                        iLGenerator.Emit(OpCodes.Ldloc, local);
                        Type elementType = parameters[index].ParameterType.GetElementType();
                        if (elementType.IsValueType)
                        {
                            iLGenerator.Emit(OpCodes.Box, elementType);
                        }
                        iLGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                    index++;
                }
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (MethodInvoker) Activator.CreateInstance(builder.CreateType());
        }
    }
}

