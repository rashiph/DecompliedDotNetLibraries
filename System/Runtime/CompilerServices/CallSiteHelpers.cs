namespace System.Runtime.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Reflection;

    public static class CallSiteHelpers
    {
        private static Type _knownNonDynamicMethodType = typeof(object).GetMethod("ToString").GetType();

        public static bool IsInternalFrame(MethodBase mb)
        {
            return (((mb.Name == "CallSite.Target") && (mb.GetType() != _knownNonDynamicMethodType)) || (mb.DeclaringType == typeof(UpdateDelegates)));
        }
    }
}

