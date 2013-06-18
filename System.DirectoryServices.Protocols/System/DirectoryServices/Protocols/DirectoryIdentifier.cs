namespace System.DirectoryServices.Protocols
{
    using System;

    public abstract class DirectoryIdentifier
    {
        protected DirectoryIdentifier()
        {
            Utility.CheckOSVersion();
        }
    }
}

