namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime;
    using System.Security;
    using System.Threading;

    internal static class JitHelpers
    {
        internal const string QCall = "QCall";

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecurityCritical]
        internal static ObjectHandleOnStack GetObjectHandleOnStack<T>(ref T o) where T: class
        {
            TypedReference reference = __makeref(o);
            return new ObjectHandleOnStack(reference.GetPointerOnStack());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern PinningHelper GetPinningHelper(object o);
        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static StackCrawlMarkHandle GetStackCrawlMarkHandle(ref StackCrawlMark stackMark)
        {
            TypedReference reference = __makeref(stackMark);
            return new StackCrawlMarkHandle(reference.GetPointerOnStack());
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static StringHandleOnStack GetStringHandleOnStack(ref string s)
        {
            TypedReference reference = __makeref(s);
            return new StringHandleOnStack(reference.GetPointerOnStack());
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        internal static T UnsafeCast<T>(object o) where T: class
        {
            return (o as T);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static int UnsafeEnumCast<T>(T val) where T: struct
        {
            throw new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void UnsafeSetArrayElement(object[] target, int index, object element);
    }
}

