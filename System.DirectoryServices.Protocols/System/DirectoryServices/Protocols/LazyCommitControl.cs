namespace System.DirectoryServices.Protocols
{
    using System;

    public class LazyCommitControl : DirectoryControl
    {
        public LazyCommitControl() : base("1.2.840.113556.1.4.619", null, true, true)
        {
        }
    }
}

