namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class DefaultMemberAttribute : Attribute
    {
        private string m_memberName;

        public DefaultMemberAttribute(string memberName)
        {
            this.m_memberName = memberName;
        }

        public string MemberName
        {
            get
            {
                return this.m_memberName;
            }
        }
    }
}

