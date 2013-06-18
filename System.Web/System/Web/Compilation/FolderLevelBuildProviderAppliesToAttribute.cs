namespace System.Web.Compilation
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class FolderLevelBuildProviderAppliesToAttribute : Attribute
    {
        private FolderLevelBuildProviderAppliesTo _appliesTo;

        public FolderLevelBuildProviderAppliesToAttribute(FolderLevelBuildProviderAppliesTo appliesTo)
        {
            this._appliesTo = appliesTo;
        }

        public FolderLevelBuildProviderAppliesTo AppliesTo
        {
            get
            {
                return this._appliesTo;
            }
        }
    }
}

