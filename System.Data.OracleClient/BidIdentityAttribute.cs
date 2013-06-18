using System;

[AttributeUsage(AttributeTargets.Module, AllowMultiple=false)]
internal sealed class BidIdentityAttribute : Attribute
{
    private string _identity;

    internal BidIdentityAttribute(string idStr)
    {
        this._identity = idStr;
    }

    internal string IdentityString
    {
        get
        {
            return this._identity;
        }
    }
}

