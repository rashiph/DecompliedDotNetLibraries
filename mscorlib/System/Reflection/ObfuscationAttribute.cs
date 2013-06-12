namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Parameter | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class ObfuscationAttribute : Attribute
    {
        private bool m_applyToMembers = true;
        private bool m_exclude = true;
        private string m_feature = "all";
        private bool m_strip = true;

        public bool ApplyToMembers
        {
            get
            {
                return this.m_applyToMembers;
            }
            set
            {
                this.m_applyToMembers = value;
            }
        }

        public bool Exclude
        {
            get
            {
                return this.m_exclude;
            }
            set
            {
                this.m_exclude = value;
            }
        }

        public string Feature
        {
            get
            {
                return this.m_feature;
            }
            set
            {
                this.m_feature = value;
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

