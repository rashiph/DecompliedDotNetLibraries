namespace System.Web.Compilation
{
    using System;

    [Flags]
    public enum FolderLevelBuildProviderAppliesTo
    {
        Code = 1,
        GlobalResources = 8,
        LocalResources = 4,
        None = 0,
        WebReferences = 2
    }
}

