namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false, Inherited=false), ComVisible(true)]
    public sealed class ObfuscateAssemblyAttribute : Attribute
    {
        private bool m_assemblyIsPrivate;
        private bool m_strip = true;

        public ObfuscateAssemblyAttribute(bool assemblyIsPrivate)
        {
            this.m_assemblyIsPrivate = assemblyIsPrivate;
        }

        public bool AssemblyIsPrivate
        {
            get
            {
                return this.m_assemblyIsPrivate;
            }
        }

        public bool StripAfterObfuscation
        {
            get
            {
                return this.m_strip;
            }
            set
            {
                this.m_strip = value;
            }
        }
    }
}

