namespace System.Web.Management
{
    using System;

    [Flags]
    public enum SqlFeatures
    {
        All = 0x4000001f,
        Membership = 1,
        None = 0,
        Personalization = 8,
        Profile = 2,
        RoleManager = 4,
        SqlWebEventProvider = 0x10
    }
}

