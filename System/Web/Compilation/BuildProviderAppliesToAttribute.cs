namespace System.Web.Compilation
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class BuildProviderAppliesToAttribute : Attribute
    {
        private BuildProviderAppliesTo _appliesTo;

        public BuildProviderAppliesToAttribute(BuildProviderAppliesTo appliesTo)
        {
            this._appliesTo = appliesTo;
        }

        public BuildProviderAppliesTo AppliesTo
        {
            get
            {
                return this._appliesTo;
            }
        }
    }
}

