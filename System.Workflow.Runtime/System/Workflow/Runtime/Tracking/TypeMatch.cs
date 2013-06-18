namespace System.Workflow.Runtime.Tracking
{
    using System;

    internal sealed class TypeMatch
    {
        private TypeMatch()
        {
        }

        internal static bool IsMatch(object obj, string name, bool matchDerived)
        {
            Type type = obj.GetType();
            if (string.Compare(type.Name, name, StringComparison.Ordinal) == 0)
            {
                return true;
            }
            if (matchDerived)
            {
                if (null != type.GetInterface(name))
                {
                    return true;
                }
                for (Type type2 = type.BaseType; type2 != null; type2 = type2.BaseType)
                {
                    if (string.Compare(type2.Name, name, StringComparison.Ordinal) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsMatch(object obj, Type matchType, bool matchDerived)
        {
            return ((obj.GetType() == matchType) || (matchDerived && matchType.IsInstanceOfType(obj)));
        }
    }
}

