namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class AssemblyDescriptionAttribute : Attribute
    {
        private string m_description;

        public AssemblyDescriptionAttribute(string description)
        {
            this.m_description = description;
        }

        public string Description
        {
            get
            {
                return this.m_description;
            }
        }
    }
}

