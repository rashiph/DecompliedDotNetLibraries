namespace System.Resources
{
    using System;
    using System.Security;
    using System.Threading;

    internal static class MultitargetUtil
    {
        public static string GetAssemblyQualifiedName(Type type, Func<Type, string> typeNameConverter)
        {
            string assemblyQualifiedName = null;
            if (type != null)
            {
                if (typeNameConverter != null)
                {
                    try
                    {
                        assemblyQualifiedName = typeNameConverter(type);
                    }
                    catch (Exception exception)
                    {
                        if (IsSecurityOrCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    assemblyQualifiedName = type.AssemblyQualifiedName;
                }
            }
            return assemblyQualifiedName;
        }

        private static bool IsSecurityOrCriticalException(Exception ex)
        {
            return (((((ex is NullReferenceException) || (ex is StackOverflowException)) || ((ex is OutOfMemoryException) || (ex is ThreadAbortException))) || (((ex is ExecutionEngineException) || (ex is IndexOutOfRangeException)) || (ex is AccessViolationException))) || (ex is SecurityException));
        }
    }
}

