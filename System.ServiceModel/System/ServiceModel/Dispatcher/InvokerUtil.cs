namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal sealed class InvokerUtil
    {
        [SecurityCritical]
        private CriticalHelper helper = new CriticalHelper();

        [SecuritySafeCritical]
        internal CreateInstanceDelegate GenerateCreateInstanceDelegate(Type type, ConstructorInfo constructor)
        {
            return this.helper.GenerateCreateInstanceDelegate(type, constructor);
        }

        [SecuritySafeCritical]
        internal InvokeBeginDelegate GenerateInvokeBeginDelegate(MethodInfo method, out int inputParameterCount)
        {
            return this.helper.GenerateInvokeBeginDelegate(method, out inputParameterCount);
        }

        [SecuritySafeCritical]
        internal InvokeDelegate GenerateInvokeDelegate(MethodInfo method, out int inputParameterCount, out int outputParameterCount)
        {
            return this.helper.GenerateInvokeDelegate(method, out inputParameterCount, out outputParameterCount);
        }

        [SecuritySafeCritical]
        internal InvokeEndDelegate GenerateInvokeEndDelegate(MethodInfo method, out int outputParameterCount)
        {
            return this.helper.GenerateInvokeEndDelegate(method, out outputParameterCount);
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class CriticalHelper
        {
            private CodeGenerator ilg;
            private static Type TypeOfObject = typeof(object);

            private static bool ConstructorRequiresMemberAccess(ConstructorInfo ctor)
            {
                if ((ctor == null) || (ctor.IsPublic && IsTypeVisible(ctor.DeclaringType)))
                {
                    return false;
                }
                return (ctor.Module != typeof(InvokerUtil).Module);
            }

            private void DeclareParameterLocals(ParameterInfo[] parameters, LocalBuilder[] parameterLocals)
            {
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    parameterLocals[i] = this.ilg.DeclareLocal(TypeLoader.GetParameterType(parameters[i]), "param" + i.ToString(CultureInfo.InvariantCulture));
                }
            }

            internal CreateInstanceDelegate GenerateCreateInstanceDelegate(Type type, ConstructorInfo constructor)
            {
                bool allowPrivateMemberAccess = !IsTypeVisible(type) || ConstructorRequiresMemberAccess(constructor);
                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("Create" + type.FullName, typeof(CreateInstanceDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustServiceCtorNotVisible", new object[] { type.FullName })));
                }
                if (type.IsValueType)
                {
                    LocalBuilder local = this.ilg.DeclareLocal(type, type.Name + "Instance");
                    this.ilg.LoadZeroValueIntoLocal(type, local);
                    this.ilg.Load(local);
                }
                else
                {
                    this.ilg.New(constructor);
                }
                this.ilg.ConvertValue(type, this.ilg.CurrentMethod.ReturnType);
                return (CreateInstanceDelegate) this.ilg.EndMethod();
            }

            internal InvokeBeginDelegate GenerateInvokeBeginDelegate(MethodInfo method, out int inputParameterCount)
            {
                bool allowPrivateMemberAccess = MethodRequiresMemberAccess(method);
                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("AsyncInvokeBegin" + method.Name, typeof(InvokeBeginDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustServiceMethodNotVisible", new object[] { method.DeclaringType.FullName, method.Name })));
                }
                ArgBuilder arg = this.ilg.GetArg(0);
                ArgBuilder inputParametersArg = this.ilg.GetArg(1);
                ArgBuilder builder3 = this.ilg.GetArg(2);
                ArgBuilder builder4 = this.ilg.GetArg(3);
                ParameterInfo[] parameters = method.GetParameters();
                LocalBuilder returnLocal = this.ilg.DeclareLocal(this.ilg.CurrentMethod.ReturnType, "returnParam");
                LocalBuilder[] parameterLocals = new LocalBuilder[parameters.Length - 2];
                this.DeclareParameterLocals(parameters, parameterLocals);
                this.LoadInputParametersIntoLocals(parameters, parameterLocals, inputParametersArg, out inputParameterCount);
                this.LoadTarget(arg, method.ReflectedType);
                this.LoadParameters(parameters, parameterLocals);
                this.ilg.Load(builder3);
                this.ilg.Load(builder4);
                this.InvokeMethod(method, returnLocal);
                this.ilg.Load(returnLocal);
                return (InvokeBeginDelegate) this.ilg.EndMethod();
            }

            internal InvokeDelegate GenerateInvokeDelegate(MethodInfo method, out int inputParameterCount, out int outputParameterCount)
            {
                bool allowPrivateMemberAccess = MethodRequiresMemberAccess(method);
                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("SyncInvoke" + method.Name, typeof(InvokeDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustServiceMethodNotVisible", new object[] { method.DeclaringType.FullName, method.Name })));
                }
                ArgBuilder arg = this.ilg.GetArg(0);
                ArgBuilder inputParametersArg = this.ilg.GetArg(1);
                ArgBuilder outputParametersArg = this.ilg.GetArg(2);
                ParameterInfo[] parameters = method.GetParameters();
                LocalBuilder returnLocal = this.ilg.DeclareLocal(this.ilg.CurrentMethod.ReturnType, "returnParam");
                LocalBuilder[] parameterLocals = new LocalBuilder[parameters.Length];
                this.DeclareParameterLocals(parameters, parameterLocals);
                this.LoadInputParametersIntoLocals(parameters, parameterLocals, inputParametersArg, out inputParameterCount);
                this.LoadTarget(arg, method.ReflectedType);
                this.LoadParameters(parameters, parameterLocals);
                this.InvokeMethod(method, returnLocal);
                this.LoadOutputParametersIntoArray(parameters, parameterLocals, outputParametersArg, out outputParameterCount);
                this.ilg.Load(returnLocal);
                return (InvokeDelegate) this.ilg.EndMethod();
            }

            internal InvokeEndDelegate GenerateInvokeEndDelegate(MethodInfo method, out int outputParameterCount)
            {
                bool allowPrivateMemberAccess = MethodRequiresMemberAccess(method);
                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("AsyncInvokeEnd" + method.Name, typeof(InvokeEndDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustServiceMethodNotVisible", new object[] { method.DeclaringType.FullName, method.Name })));
                }
                ArgBuilder arg = this.ilg.GetArg(0);
                ArgBuilder outputParametersArg = this.ilg.GetArg(1);
                ArgBuilder builder3 = this.ilg.GetArg(2);
                ParameterInfo[] parameters = method.GetParameters();
                LocalBuilder returnLocal = this.ilg.DeclareLocal(this.ilg.CurrentMethod.ReturnType, "returnParam");
                LocalBuilder[] parameterLocals = new LocalBuilder[parameters.Length - 1];
                this.DeclareParameterLocals(parameters, parameterLocals);
                this.LoadZeroValueInputParametersIntoLocals(parameters, parameterLocals);
                this.LoadTarget(arg, method.ReflectedType);
                this.LoadParameters(parameters, parameterLocals);
                this.ilg.Load(builder3);
                this.InvokeMethod(method, returnLocal);
                this.LoadOutputParametersIntoArray(parameters, parameterLocals, outputParametersArg, out outputParameterCount);
                this.ilg.Load(returnLocal);
                return (InvokeEndDelegate) this.ilg.EndMethod();
            }

            private void InvokeMethod(MethodInfo method, LocalBuilder returnLocal)
            {
                this.ilg.Call(method);
                if (method.ReturnType == typeof(void))
                {
                    this.ilg.Load(null);
                }
                else
                {
                    this.ilg.ConvertValue(method.ReturnType, this.ilg.CurrentMethod.ReturnType);
                }
                this.ilg.Store(returnLocal);
            }

            private static bool IsTypeVisible(Type t)
            {
                if (t.Module != typeof(InvokerUtil).Module)
                {
                    if (!t.IsVisible)
                    {
                        return false;
                    }
                    foreach (Type type in t.GetGenericArguments())
                    {
                        if (!type.IsGenericParameter && !IsTypeVisible(type))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            private void LoadInputParametersIntoLocals(ParameterInfo[] parameters, LocalBuilder[] parameterLocals, ArgBuilder inputParametersArg, out int inputParameterCount)
            {
                inputParameterCount = 0;
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (ServiceReflector.FlowsIn(parameters[i]))
                    {
                        Type localType = parameterLocals[i].LocalType;
                        this.ilg.LoadArrayElement(inputParametersArg, (int) inputParameterCount);
                        if (!localType.IsValueType)
                        {
                            this.ilg.ConvertValue(TypeOfObject, localType);
                            this.ilg.Store(parameterLocals[i]);
                        }
                        else
                        {
                            this.ilg.Dup();
                            this.ilg.If();
                            this.ilg.ConvertValue(TypeOfObject, localType);
                            this.ilg.Store(parameterLocals[i]);
                            this.ilg.Else();
                            this.ilg.Pop();
                            this.ilg.LoadZeroValueIntoLocal(localType, parameterLocals[i]);
                            this.ilg.EndIf();
                        }
                        inputParameterCount++;
                    }
                }
            }

            private void LoadOutputParametersIntoArray(ParameterInfo[] parameters, LocalBuilder[] parameterLocals, ArgBuilder outputParametersArg, out int outputParameterCount)
            {
                outputParameterCount = 0;
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (ServiceReflector.FlowsOut(parameters[i]))
                    {
                        this.ilg.Load(outputParametersArg);
                        this.ilg.Load((int) outputParameterCount);
                        this.ilg.Load(parameterLocals[i]);
                        this.ilg.ConvertValue(parameterLocals[i].LocalType, TypeOfObject);
                        this.ilg.Stelem(TypeOfObject);
                        outputParameterCount++;
                    }
                }
            }

            private void LoadParameters(ParameterInfo[] parameters, LocalBuilder[] parameterLocals)
            {
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (parameters[i].ParameterType.IsByRef)
                    {
                        this.ilg.Ldloca(parameterLocals[i]);
                    }
                    else
                    {
                        this.ilg.Ldloc(parameterLocals[i]);
                    }
                }
            }

            private void LoadTarget(ArgBuilder targetArg, Type targetType)
            {
                this.ilg.Load(targetArg);
                this.ilg.ConvertValue(targetArg.ArgType, targetType);
                if (targetType.IsValueType)
                {
                    LocalBuilder var = this.ilg.DeclareLocal(targetType, "target");
                    this.ilg.Store(var);
                    this.ilg.LoadAddress(var);
                }
            }

            private void LoadZeroValueInputParametersIntoLocals(ParameterInfo[] parameters, LocalBuilder[] parameterLocals)
            {
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (ServiceReflector.FlowsIn(parameters[i]))
                    {
                        this.ilg.LoadZeroValueIntoLocal(parameterLocals[i].LocalType, parameterLocals[i]);
                    }
                }
            }

            private static bool MethodRequiresMemberAccess(MethodInfo method)
            {
                if ((method == null) || (method.IsPublic && IsTypeVisible(method.DeclaringType)))
                {
                    return false;
                }
                return (method.Module != typeof(InvokerUtil).Module);
            }
        }
    }
}

