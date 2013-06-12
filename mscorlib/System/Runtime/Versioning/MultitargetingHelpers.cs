namespace System.Runtime.Versioning
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal static class MultitargetingHelpers
    {
        private static Func<Type, string> defaultConverter = t => t.AssemblyQualifiedName;

        [CompilerGenerated]
        private static string <.cctor>b__0(Type t)
        {
            return t.AssemblyQualifiedName;
        }

        internal static string GetAssemblyQualifiedName(Type type, Func<Type, string> converter)
        {
            string str = null;
            if (type != null)
            {
                if (converter != null)
                {
                    try
                    {
                        str = converter(type);
                    }
                    catch (Exception exception)
                    {
                        if (IsSecurityOrCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
                if (str == null)
                {
                    str = defaultConverter(type);
                }
            }
            return str;
        }

        private static bool IsCriticalException(Exception ex)
        {
            return (((((ex is NullReferenceException) || (ex is StackOverflowException)) || ((ex is OutOfMemoryException) || (ex is ThreadAbortException))) || (ex is IndexOutOfRangeException)) || (ex is AccessViolationException));
        }

        private static bool IsSecurityOrCriticalException(Exception ex)
        {
            return ((ex is SecurityException) || IsCriticalException(ex));
        }
    }
}

