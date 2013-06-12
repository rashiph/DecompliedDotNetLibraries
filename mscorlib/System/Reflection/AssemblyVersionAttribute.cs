namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AssemblyVersionAttribute : Attribute
    {
        private string m_version;

        public AssemblyVersionAttribute(string version)
        {
            this.m_version = version;
        }

        public string Version
        {
            get
            {
                return this.m_version;
            }
        }
    }
}

