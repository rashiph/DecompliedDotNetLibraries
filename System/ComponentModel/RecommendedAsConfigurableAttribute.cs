namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property), Obsolete("Use System.ComponentModel.SettingsBindableAttribute instead to work with the new settings model.")]
    public class RecommendedAsConfigurableAttribute : Attribute
    {
        public static readonly RecommendedAsConfigurableAttribute Default = No;
        public static readonly RecommendedAsConfigurableAttribute No = new RecommendedAsConfigurableAttribute(false);
        private bool recommendedAsConfigurable;
        public static readonly RecommendedAsConfigurableAttribute Yes = new RecommendedAsConfigurableAttribute(true);

        public RecommendedAsConfigurableAttribute(bool recommendedAsConfigurable)
        {
            this.recommendedAsConfigurable = recommendedAsConfigurable;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            RecommendedAsConfigurableAttribute attribute = obj as RecommendedAsConfigurableAttribute;
            return ((attribute != null) && (attribute.RecommendedAsConfigurable == this.recommendedAsConfigurable));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return !this.recommendedAsConfigurable;
        }

        public bool RecommendedAsConfigurable
        {
            get
            {
                return this.recommendedAsConfigurable;
            }
        }
    }
}

