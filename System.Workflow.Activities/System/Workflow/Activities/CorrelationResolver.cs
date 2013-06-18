namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;

    internal static class CorrelationResolver
    {
        private static Dictionary<Type, CorrelationMethodResolver> cachedTypeResolver = new Dictionary<Type, CorrelationMethodResolver>();
        private static object mutex = new object();

        internal static ICorrelationProvider GetCorrelationProvider(Type interfaceType)
        {
            return GetResolver(interfaceType).CorrelationProvider;
        }

        private static CorrelationMethodResolver GetResolver(Type interfaceType)
        {
            CorrelationMethodResolver resolver = null;
            cachedTypeResolver.TryGetValue(interfaceType, out resolver);
            if (resolver == null)
            {
                lock (mutex)
                {
                    cachedTypeResolver.TryGetValue(interfaceType, out resolver);
                    if (resolver == null)
                    {
                        resolver = new CorrelationMethodResolver(interfaceType);
                        cachedTypeResolver.Add(interfaceType, resolver);
                    }
                }
            }
            return resolver;
        }

        internal static bool IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (memberName == null)
            {
                throw new ArgumentNullException("memberName");
            }
            if (memberName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EventNameMissing"));
            }
            return GetCorrelationProvider(interfaceType).IsInitializingMember(interfaceType, memberName, methodArgs);
        }

        internal static ICollection<CorrelationProperty> ResolveCorrelationValues(Type interfaceType, string eventName, object[] eventArgs, bool provideInitializerTokens)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (eventName == null)
            {
                throw new ArgumentNullException("eventName");
            }
            if (eventName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EventNameMissing"));
            }
            return GetCorrelationProvider(interfaceType).ResolveCorrelationPropertyValues(interfaceType, eventName, eventArgs, provideInitializerTokens);
        }
    }
}

