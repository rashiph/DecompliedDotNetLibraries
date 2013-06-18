namespace System.DirectoryServices
{
    using System;

    public enum ActiveDirectorySecurityInheritance
    {
        None,
        All,
        Descendents,
        SelfAndChildren,
        Children
    }
}

