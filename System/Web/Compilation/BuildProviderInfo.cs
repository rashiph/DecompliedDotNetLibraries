namespace System.Web.Compilation
{
    using System;

    internal abstract class BuildProviderInfo
    {
        private BuildProviderAppliesTo _appliesTo;

        protected BuildProviderInfo()
        {
        }

        internal BuildProviderAppliesTo AppliesTo
        {
            get
            {
                if (this._appliesTo == 0)
                {
                    object[] customAttributes = this.Type.GetCustomAttributes(typeof(BuildProviderAppliesToAttribute), true);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        this._appliesTo = ((BuildProviderAppliesToAttribute) customAttributes[0]).AppliesTo;
                    }
                    else
                    {
                        this._appliesTo = BuildProviderAppliesTo.All;
                    }
                }
                return this._appliesTo;
            }
        }

        internal abstract System.Type Type { get; }
    }
}

