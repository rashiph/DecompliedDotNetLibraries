namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal static class EnumHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
        public static Type GetUnderlyingType(Type type)
        {
            Type type2 = typeof(int);
            if (type.GetType().FullName.Equals("System.Workflow.ComponentModel.Compiler.DesignTimeType", StringComparison.Ordinal))
            {
                MethodInfo method = type.GetType().GetMethod("GetEnumType");
                if (method != null)
                {
                    Type type3 = method.Invoke(type, new object[0]) as Type;
                    type2 = (type3 != null) ? type3 : type2;
                }
                return type2;
            }
            return Enum.GetUnderlyingType(type);
        }
    }
}

