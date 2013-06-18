namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class Utility
    {
        private ExceptionUtility exceptionUtility;

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.Utility instead"), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Utility(ExceptionUtility exceptionUtility)
        {
            this.exceptionUtility = exceptionUtility;
        }

        internal byte[] AllocateByteArray(int size)
        {
            return Fx.AllocateByteArray(size);
        }

        internal char[] AllocateCharArray(int size)
        {
            return Fx.AllocateCharArray(size);
        }

        internal static void CloseInvalidOutCriticalHandle(CriticalHandle handle)
        {
            if (handle != null)
            {
                handle.SetHandleAsInvalid();
            }
        }

        internal static void CloseInvalidOutSafeHandle(SafeHandle handle)
        {
            if (handle != null)
            {
                handle.SetHandleAsInvalid();
            }
        }

        internal Guid CreateGuid(string guidString)
        {
            return Fx.CreateGuid(guidString);
        }

        internal bool TryCreateGuid(string guidString, out Guid result)
        {
            return Fx.TryCreateGuid(guidString, out result);
        }
    }
}

