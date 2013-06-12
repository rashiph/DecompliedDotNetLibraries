namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class AssemblyInformationalVersionAttribute : Attribute
    {
        private string m_informationalVersion;

        public AssemblyInformationalVersionAttribute(string informationalVersion)
        {
            this.m_informationalVersion = informationalVersion;
        }

        public string InformationalVersion
        {
            get
            {
                return this.m_informationalVersion;
            }
        }
    }
}

