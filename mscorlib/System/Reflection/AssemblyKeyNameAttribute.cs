namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class AssemblyKeyNameAttribute : Attribute
    {
        private string m_keyName;

        public AssemblyKeyNameAttribute(string keyName)
        {
            this.m_keyName = keyName;
        }

        public string KeyName
        {
            get
            {
                return this.m_keyName;
            }
        }
    }
}

