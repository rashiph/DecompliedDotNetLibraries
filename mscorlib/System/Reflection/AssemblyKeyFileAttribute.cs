namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AssemblyKeyFileAttribute : Attribute
    {
        private string m_keyFile;

        public AssemblyKeyFileAttribute(string keyFile)
        {
            this.m_keyFile = keyFile;
        }

        public string KeyFile
        {
            get
            {
                return this.m_keyFile;
            }
        }
    }
}

