using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

[AttributeUsage(AttributeTargets.All)]
internal sealed class SRCategoryAttribute : CategoryAttribute
{
    private string resourceSet;

    public SRCategoryAttribute(string category) : base(category)
    {
        this.resourceSet = string.Empty;
    }

    public SRCategoryAttribute(string category, string resourceSet) : base(category)
    {
        this.resourceSet = string.Empty;
        this.resourceSet = resourceSet;
    }

    protected override string GetLocalizedString(string value)
    {
        if (this.resourceSet.Length > 0)
        {
            ResourceManager manager = new ResourceManager(this.resourceSet, Assembly.GetExecutingAssembly());
            return manager.GetString(value);
        }
        return SR.GetString(value);
    }
}

