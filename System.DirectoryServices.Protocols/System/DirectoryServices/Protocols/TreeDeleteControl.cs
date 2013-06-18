namespace System.DirectoryServices.Protocols
{
    using System;

    public class TreeDeleteControl : DirectoryControl
    {
        public TreeDeleteControl() : base("1.2.840.113556.1.4.805", null, true, true)
        {
        }
    }
}

