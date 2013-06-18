namespace System.Windows.Forms
{
    using System;
    using System.Reflection;

    public abstract class FeatureSupport : IFeatureSupport
    {
        protected FeatureSupport()
        {
        }

        public abstract Version GetVersionPresent(object feature);
        public static Version GetVersionPresent(string featureClassName, string featureConstName)
        {
            object feature = null;
            IFeatureSupport support = null;
            System.Type type = null;
            try
            {
                type = System.Type.GetType(featureClassName);
            }
            catch (ArgumentException)
            {
            }
            if (type != null)
            {
                FieldInfo field = type.GetField(featureConstName);
                if (field != null)
                {
                    feature = field.GetValue(null);
                }
            }
            if (feature != null)
            {
                support = (IFeatureSupport) System.Windows.Forms.SecurityUtils.SecureCreateInstance(type);
                if (support != null)
                {
                    return support.GetVersionPresent(feature);
                }
            }
            return null;
        }

        public virtual bool IsPresent(object feature)
        {
            return this.IsPresent(feature, new Version(0, 0, 0, 0));
        }

        public virtual bool IsPresent(object feature, Version minimumVersion)
        {
            Version versionPresent = this.GetVersionPresent(feature);
            return ((versionPresent != null) && (versionPresent.CompareTo(minimumVersion) >= 0));
        }

        public static bool IsPresent(string featureClassName, string featureConstName)
        {
            return IsPresent(featureClassName, featureConstName, new Version(0, 0, 0, 0));
        }

        public static bool IsPresent(string featureClassName, string featureConstName, Version minimumVersion)
        {
            object feature = null;
            IFeatureSupport support = null;
            System.Type c = null;
            try
            {
                c = System.Type.GetType(featureClassName);
            }
            catch (ArgumentException)
            {
            }
            if (c != null)
            {
                FieldInfo field = c.GetField(featureConstName);
                if (field != null)
                {
                    feature = field.GetValue(null);
                }
            }
            if ((feature != null) && typeof(IFeatureSupport).IsAssignableFrom(c))
            {
                support = (IFeatureSupport) System.Windows.Forms.SecurityUtils.SecureCreateInstance(c);
                if (support != null)
                {
                    return support.IsPresent(feature, minimumVersion);
                }
            }
            return false;
        }
    }
}

