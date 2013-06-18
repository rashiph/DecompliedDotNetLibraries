namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Threading;

    internal sealed class OdbcEnvironment
    {
        private static object _globalEnvironmentHandle;
        private static object _globalEnvironmentHandleLock = new object();

        private OdbcEnvironment()
        {
        }

        internal static OdbcEnvironmentHandle GetGlobalEnvironmentHandle()
        {
            OdbcEnvironmentHandle handle = _globalEnvironmentHandle as OdbcEnvironmentHandle;
            if (handle == null)
            {
                ADP.CheckVersionMDAC(true);
                lock (_globalEnvironmentHandleLock)
                {
                    handle = _globalEnvironmentHandle as OdbcEnvironmentHandle;
                    if (handle == null)
                    {
                        handle = new OdbcEnvironmentHandle();
                        _globalEnvironmentHandle = handle;
                    }
                }
            }
            return handle;
        }

        internal static void ReleaseObjectPool()
        {
            object obj2 = Interlocked.Exchange(ref _globalEnvironmentHandle, null);
            if (obj2 != null)
            {
                (obj2 as OdbcEnvironmentHandle).Dispose();
            }
        }
    }
}

