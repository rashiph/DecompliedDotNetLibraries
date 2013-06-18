namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal static class AssertUtility
    {
        [MethodImpl(MethodImplOptions.NoInlining), Conditional("DEBUG"), Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.DebugAssert instead"), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void DebugAssert(string message)
        {
            DebugAssertCore(message);
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.DebugAssert instead"), Conditional("DEBUG")]
        internal static void DebugAssert(bool condition, string message)
        {
            if (!condition)
            {
                DebugAssert(message);
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.DebugAssert instead")]
        internal static void DebugAssertCore(string message)
        {
            try
            {
            }
            finally
            {
                Debug.Assert(false, message);
            }
        }
    }
}

