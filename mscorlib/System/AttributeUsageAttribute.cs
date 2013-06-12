namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Class, Inherited=true)]
    public sealed class AttributeUsageAttribute : Attribute
    {
        internal static AttributeUsageAttribute Default = new AttributeUsageAttribute(AttributeTargets.All);
        internal bool m_allowMultiple;
        internal AttributeTargets m_attributeTarget;
        internal bool m_inherited;

        public AttributeUsageAttribute(AttributeTargets validOn)
        {
            this.m_attributeTarget = AttributeTargets.All;
            this.m_inherited = true;
            this.m_attributeTarget = validOn;
        }

        internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
        {
            this.m_attributeTarget = AttributeTargets.All;
            this.m_inherited = true;
            this.m_attributeTarget = validOn;
            this.m_allowMultiple = allowMultiple;
            this.m_inherited = inherited;
        }

        public bool AllowMultiple
        {
            get
            {
                return this.m_allowMultiple;
            }
            set
            {
                this.m_allowMultiple = value;
            }
        }

        public bool Inherited
        {
            get
            {
                return this.m_inherited;
            }
            set
            {
                this.m_inherited = value;
            }
        }

        public AttributeTargets ValidOn
        {
            get
            {
                return this.m_attributeTarget;
            }
        }
    }
}

