namespace System.DirectoryServices.Protocols
{
    using System;

    internal class UtilityHandle
    {
        private static ConnectionHandle handle = new ConnectionHandle();

        public static ConnectionHandle GetHandle()
        {
            return handle;
        }
    }
}

