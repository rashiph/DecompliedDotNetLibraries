using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

[AttributeUsage(AttributeTargets.All)]
internal sealed class SRDescriptionAttribute : DescriptionAttribute
{
    public SRDescriptionAttribute(string description)
    {
        base.DescriptionValue = SR.GetString(description);
    }

    public SRDescriptionAttribute(string description, string resourceSet)
    {
        base.DescriptionValue = new ResourceManager(resourceSet, Assembly.GetExecutingAssembly()).GetString(description);
    }
}

