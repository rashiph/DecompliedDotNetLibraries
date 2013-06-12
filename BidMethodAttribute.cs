using System;
using System.Diagnostics;

[Conditional("CODE_ANALYSIS"), AttributeUsage(AttributeTargets.Method)]
internal sealed class BidMethodAttribute : Attribute
{
    private bool m_enabled = true;

    internal BidMethodAttribute()
    {
    }

    public bool Enabled
    {
        get
        {
            return this.m_enabled;
        }
        set
        {
            this.m_enabled = value;
        }
    }
}

