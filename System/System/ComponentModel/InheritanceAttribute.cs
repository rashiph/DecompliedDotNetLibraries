namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InheritanceAttribute : Attribute
    {
        public static readonly InheritanceAttribute Default = NotInherited;
        private readonly System.ComponentModel.InheritanceLevel inheritanceLevel;
        public static readonly InheritanceAttribute Inherited = new InheritanceAttribute(System.ComponentModel.InheritanceLevel.Inherited);
        public static readonly InheritanceAttribute InheritedReadOnly = new InheritanceAttribute(System.ComponentModel.InheritanceLevel.InheritedReadOnly);
        public static readonly InheritanceAttribute NotInherited = new InheritanceAttribute(System.ComponentModel.InheritanceLevel.NotInherited);

        public InheritanceAttribute()
        {
            this.inheritanceLevel = Default.inheritanceLevel;
        }

        public InheritanceAttribute(System.ComponentModel.InheritanceLevel inheritanceLevel)
        {
            this.inheritanceLevel = inheritanceLevel;
        }

        public override bool Equals(object value)
        {
            return ((value == this) || ((value is InheritanceAttribute) && (((InheritanceAttribute) value).InheritanceLevel == this.inheritanceLevel)));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public override string ToString()
        {
            return TypeDescriptor.GetConverter(typeof(System.ComponentModel.InheritanceLevel)).ConvertToString(this.InheritanceLevel);
        }

        public System.ComponentModel.InheritanceLevel InheritanceLevel
        {
            get
            {
                return this.inheritanceLevel;
            }
        }
    }
}

