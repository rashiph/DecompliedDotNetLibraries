namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.All, Inherited=true, AllowMultiple=false), ComVisible(true)]
    public sealed class CLSCompliantAttribute : Attribute
    {
        private bool m_compliant;

        public CLSCompliantAttribute(bool isCompliant)
        {
            this.m_compliant = isCompliant;
        }

        public bool IsCompliant
        {
            get
            {
                return this.m_compliant;
            }
        }
    }
}

