namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Field, Inherited=false, AllowMultiple=false)]
    public sealed class VBFixedStringAttribute : Attribute
    {
        private int m_Length;

        public VBFixedStringAttribute(int Length)
        {
            if ((Length < 1) || (Length > 0x7fff))
            {
                throw new ArgumentException(Utils.GetResourceString("Invalid_VBFixedString"));
            }
            this.m_Length = Length;
        }

        public int Length
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_Length;
            }
        }
    }
}

