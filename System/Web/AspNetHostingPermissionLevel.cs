namespace System.Web
{
    using System;

    [Serializable]
    public enum AspNetHostingPermissionLevel
    {
        High = 500,
        Low = 300,
        Medium = 400,
        Minimal = 200,
        None = 100,
        Unrestricted = 600
    }
}

