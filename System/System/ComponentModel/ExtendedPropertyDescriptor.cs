namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal sealed class ExtendedPropertyDescriptor : PropertyDescriptor
    {
        private readonly ReflectPropertyDescriptor extenderInfo;
        private readonly IExtenderProvider provider;

        public ExtendedPropertyDescriptor(PropertyDescriptor extender, Attribute[] attributes) : base(extender, attributes)
        {
            ExtenderProvidedPropertyAttribute attribute = extender.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
            ReflectPropertyDescriptor extenderProperty = attribute.ExtenderProperty as ReflectPropertyDescriptor;
            this.extenderInfo = extenderProperty;
            this.provider = attribute.Provider;
        }

        public ExtendedPropertyDescriptor(ReflectPropertyDescriptor extenderInfo, Type receiverType, IExtenderProvider provider, Attribute[] attributes) : base(extenderInfo, attributes)
        {
            ArrayList list = new ArrayList(this.AttributeArray);
            list.Add(ExtenderProvidedPropertyAttribute.Create(extenderInfo, receiverType, provider));
            if (extenderInfo.IsReadOnly)
            {
                list.Add(ReadOnlyAttribute.Yes);
            }
            Attribute[] array = new Attribute[list.Count];
            list.CopyTo(array, 0);
            this.AttributeArray = array;
            this.extenderInfo = extenderInfo;
            this.provider = provider;
        }

        public override bool CanResetValue(object comp)
        {
            return this.extenderInfo.ExtenderCanResetValue(this.provider, comp);
        }

        public override object GetValue(object comp)
        {
            return this.extenderInfo.ExtenderGetValue(this.provider, comp);
        }

        public override void ResetValue(object comp)
        {
            this.extenderInfo.ExtenderResetValue(this.provider, comp, this);
        }

        public override void SetValue(object component, object value)
        {
            this.extenderInfo.ExtenderSetValue(this.provider, component, value, this);
        }

        public override bool ShouldSerializeValue(object comp)
        {
            return this.extenderInfo.ExtenderShouldSerializeValue(this.provider, comp);
        }

        public override Type ComponentType
        {
            get
            {
                return this.extenderInfo.ComponentType;
            }
        }

        public override string DisplayName
        {
            get
            {
                string displayName = base.DisplayName;
                DisplayNameAttribute attribute = this.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if ((attribute == null) || attribute.IsDefaultAttribute())
                {
                    ISite site = MemberDescriptor.GetSite(this.provider);
                    if (site != null)
                    {
                        string name = site.Name;
                        if ((name != null) && (name.Length > 0))
                        {
                            displayName = SR.GetString("MetaExtenderName", new object[] { displayName, name });
                        }
                    }
                }
                return displayName;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.extenderInfo.ExtenderGetType(this.provider);
            }
        }
    }
}

