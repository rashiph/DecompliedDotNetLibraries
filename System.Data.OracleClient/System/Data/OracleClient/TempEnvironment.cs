namespace System.Data.OracleClient
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class TempEnvironment
    {
        private static OciErrorHandle availableErrorHandle;
        private static OciEnvironmentHandle environmentHandle;
        private static volatile bool isInitialized;
        private static object locked = new object();

        private TempEnvironment()
        {
        }

        internal static OciErrorHandle GetErrorHandle()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            return availableErrorHandle;
        }

        private static void Initialize()
        {
            lock (locked)
            {
                if (!isInitialized)
                {
                    bool unicode = false;
                    OCI.MODE environmentMode = OCI.MODE.OCI_DATA_AT_EXEC | OCI.MODE.OCI_BATCH_MODE;
                    OCI.DetermineClientVersion();
                    environmentHandle = new OciEnvironmentHandle(environmentMode, unicode);
                    availableErrorHandle = new OciErrorHandle(environmentHandle);
                    isInitialized = true;
                }
            }
        }
    }
}

