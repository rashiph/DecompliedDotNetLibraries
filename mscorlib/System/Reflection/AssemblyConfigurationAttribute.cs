namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AssemblyConfigurationAttribute : Attribute
    {
        private string m_configuration;

        public AssemblyConfigurationAttribute(string configuration)
        {
            this.m_configuration = configuration;
        }

        public string Configuration
        {
            get
            {
                return this.m_configuration;
            }
        }
    }
}

