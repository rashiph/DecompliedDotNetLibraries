namespace System.Web.Compilation
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Web;

    internal static class CompilationLock
    {
        private static CompilationMutex _mutex;

        static CompilationLock()
        {
            int hashCode = ("CompilationLock" + HttpRuntime.AppDomainAppIdInternal.ToLower(CultureInfo.InvariantCulture)).GetHashCode();
            _mutex = new CompilationMutex("CL" + hashCode.ToString("x", CultureInfo.InvariantCulture), "CompilationLock for " + HttpRuntime.AppDomainAppVirtualPath);
        }

        internal static void GetLock(ref bool gotLock)
        {
            try
            {
            }
            finally
            {
                Monitor.Enter(BuildManager.TheBuildManager);
                _mutex.WaitOne();
                gotLock = true;
            }
        }

        internal static void ReleaseLock()
        {
            _mutex.ReleaseMutex();
            Monitor.Exit(BuildManager.TheBuildManager);
        }
    }
}

