namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum ControlFlags
    {
        DiscretionaryAclAutoInherited = 0x400,
        DiscretionaryAclAutoInheritRequired = 0x100,
        DiscretionaryAclDefaulted = 8,
        DiscretionaryAclPresent = 4,
        DiscretionaryAclProtected = 0x1000,
        DiscretionaryAclUntrusted = 0x40,
        GroupDefaulted = 2,
        None = 0,
        OwnerDefaulted = 1,
        RMControlValid = 0x4000,
        SelfRelative = 0x8000,
        ServerSecurity = 0x80,
        SystemAclAutoInherited = 0x800,
        SystemAclAutoInheritRequired = 0x200,
        SystemAclDefaulted = 0x20,
        SystemAclPresent = 0x10,
        SystemAclProtected = 0x2000
    }
}

