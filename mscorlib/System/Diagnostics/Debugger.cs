namespace System.Diagnostics
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true)]
    public sealed class Debugger
    {
        public static readonly string DefaultCategory;

        [SecuritySafeCritical]
        public static void Break()
        {
            if (!IsDebuggerAttached())
            {
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                }
                catch (SecurityException)
                {
                    return;
                }
            }
            BreakInternal();
        }

        [SecuritySafeCritical]
        private static void BreakCanThrow()
        {
            if (!IsDebuggerAttached())
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            BreakInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void BreakInternal();
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern void CustomNotification(ICustomDebuggerNotification data);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern bool IsDebuggerAttached();
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool IsLogging();
        [SecuritySafeCritical]
        public static bool Launch()
        {
            if (IsDebuggerAttached())
            {
                return true;
            }
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            catch (SecurityException)
            {
                return false;
            }
            return LaunchInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool LaunchInternal();
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern void Log(int level, string category, string message);
        [ComVisible(false), SecuritySafeCritical]
        public static void NotifyOfCrossThreadDependency()
        {
            CrossThreadDependencyNotification data = new CrossThreadDependencyNotification();
            CustomNotification(data);
        }

        public static bool IsAttached
        {
            get
            {
                return IsDebuggerAttached();
            }
        }

        private class CrossThreadDependencyNotification : ICustomDebuggerNotification
        {
        }
    }
}

