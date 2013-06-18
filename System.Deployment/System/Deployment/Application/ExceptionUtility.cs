namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal sealed class ExceptionUtility
    {
        public static bool IsHardException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            Win32Exception exception2 = exception as Win32Exception;
            if (exception2 != null)
            {
                exception = Marshal.GetExceptionForHR(exception2.ErrorCode);
            }
            return (((exception is DivideByZeroException) || (exception is OutOfMemoryException)) || ((exception is StackOverflowException) || (exception is AccessViolationException)));
        }
    }
}

