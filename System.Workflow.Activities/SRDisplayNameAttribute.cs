using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

[AttributeUsage(AttributeTargets.All)]
internal sealed class SRDisplayNameAttribute : DisplayNameAttribute
{
    public SRDisplayNameAttribute(string name)
    {
        base.DisplayNameValue = SR.GetString(name);
    }

    public SRDisplayNameAttribute(string name, string resourceSet)
    {
        base.DisplayNameValue = new ResourceManager(resourceSet, Assembly.GetExecutingAssembly()).GetString(name);
    }
}

