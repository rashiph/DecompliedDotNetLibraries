namespace System.Xaml
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal static class CriticalExceptions
    {
        internal static bool IsCriticalException(Exception ex)
        {
            ex = Unwrap(ex);
            return (((((ex is NullReferenceException) || (ex is StackOverflowException)) || ((ex is OutOfMemoryException) || (ex is ThreadAbortException))) || (ex is SEHException)) || (ex is SecurityException));
        }

        internal static Exception Unwrap(Exception ex)
        {
            while ((ex.InnerException != null) && (ex is TargetInvocationException))
            {
                ex = ex.InnerException;
            }
            return ex;
        }
    }
}

