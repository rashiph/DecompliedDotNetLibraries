namespace System.Dynamic.Utils
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    internal static class TypeExtensions
    {
        private static readonly CacheDict<MethodBase, ParameterInfo[]> _ParamInfoCache = new CacheDict<MethodBase, ParameterInfo[]>(0x4b);

        internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
            DynamicMethod method = methodInfo as DynamicMethod;
            if (method != null)
            {
                return method.CreateDelegate(delegateType);
            }
            return Delegate.CreateDelegate(delegateType, methodInfo);
        }

        internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target)
        {
            DynamicMethod method = methodInfo as DynamicMethod;
            if (method != null)
            {
                return method.CreateDelegate(delegateType, target);
            }
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
        }

        internal static MethodInfo GetMethodValidated(this Type type, string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            MethodInfo mi = type.GetMethod(name, bindingAttr, binder, types, modifiers);
            if (!mi.MatchesArgumentTypes(types))
            {
                return null;
            }
            return mi;
        }

        internal static ParameterInfo[] GetParametersCached(this MethodBase method)
        {
            ParameterInfo[] parameters;
            lock (_ParamInfoCache)
            {
                if (!_ParamInfoCache.TryGetValue(method, out parameters))
                {
                    parameters = method.GetParameters();
                    Type declaringType = method.DeclaringType;
                    if ((declaringType != null) && declaringType.CanCache())
                    {
                        _ParamInfoCache[method] = parameters;
                    }
                }
            }
            return parameters;
        }

        internal static Type GetReturnType(this MethodBase mi)
        {
            if (!mi.IsConstructor)
            {
                return ((MethodInfo) mi).ReturnType;
            }
            return mi.DeclaringType;
        }

        internal static bool IsByRefParameter(this ParameterInfo pi)
        {
            return (pi.ParameterType.IsByRef || ((pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out));
        }

        private static bool MatchesArgumentTypes(this MethodInfo mi, Type[] argTypes)
        {
            if ((mi == null) || (argTypes == null))
            {
                return false;
            }
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length != argTypes.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!TypeUtils.AreReferenceAssignable(parameters[i].ParameterType, argTypes[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

