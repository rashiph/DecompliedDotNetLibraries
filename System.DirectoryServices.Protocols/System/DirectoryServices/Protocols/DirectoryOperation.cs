namespace System.DirectoryServices.Protocols
{
    using System;

    public abstract class DirectoryOperation
    {
        internal string directoryRequestID;

        protected DirectoryOperation()
        {
        }
    }
}

