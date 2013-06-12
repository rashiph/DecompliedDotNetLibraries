namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class RefreshPropertiesAttribute : Attribute
    {
        public static readonly RefreshPropertiesAttribute All = new RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.All);
        public static readonly RefreshPropertiesAttribute Default = new RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.None);
        private System.ComponentModel.RefreshProperties refresh;
        public static readonly RefreshPropertiesAttribute Repaint = new RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.Repaint);

        public RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties refresh)
        {
            this.refresh = refresh;
        }

        public override bool Equals(object value)
        {
            return ((value is RefreshPropertiesAttribute) && (((RefreshPropertiesAttribute) value).RefreshProperties == this.refresh));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public System.ComponentModel.RefreshProperties RefreshProperties
        {
            get
            {
                return this.refresh;
            }
        }
    }
}

