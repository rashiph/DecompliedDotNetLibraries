namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class AssemblyDelaySignAttribute : Attribute
    {
        private bool m_delaySign;

        public AssemblyDelaySignAttribute(bool delaySign)
        {
            this.m_delaySign = delaySign;
        }

        public bool DelaySign
        {
            get
            {
                return this.m_delaySign;
            }
        }
    }
}

