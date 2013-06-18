namespace Microsoft.VisualC
{
    using System;

    [Obsolete("Microsoft.VisualC.dll is an obsolete assembly and exists only for backwards compatibility."), AttributeUsage(AttributeTargets.All)]
    public sealed class MiscellaneousBitsAttribute : Attribute
    {
        public int m_dwAttrs;

        public MiscellaneousBitsAttribute(int miscellaneousBits)
        {
            this.m_dwAttrs = miscellaneousBits;
        }
    }
}

