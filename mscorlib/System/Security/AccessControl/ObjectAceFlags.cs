namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum ObjectAceFlags
    {
        None,
        ObjectAceTypePresent,
        InheritedObjectAceTypePresent
    }
}

