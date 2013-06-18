namespace System.ServiceModel
{
    using System;

    internal static class OSEnvironmentHelper
    {
        private const int VistaMajorVersion = 6;

        internal static bool IsVistaOrGreater
        {
            get
            {
                return (Environment.OSVersion.Version.Major >= 6);
            }
        }

        internal static int ProcessorCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }
    }
}

