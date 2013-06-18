namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Activities;
    using System.Xaml;

    public static class VisualBasic
    {
        private static AttachableMemberIdentifier settingsPropertyID = new AttachableMemberIdentifier(typeof(VisualBasic), "Settings");

        public static VisualBasicSettings GetSettings(object target)
        {
            VisualBasicSettings settings;
            if (!AttachablePropertyServices.TryGetProperty<VisualBasicSettings>(target, settingsPropertyID, out settings))
            {
                return null;
            }
            return settings;
        }

        public static void SetSettings(object target, VisualBasicSettings value)
        {
            AttachablePropertyServices.SetProperty(target, settingsPropertyID, value);
        }

        public static void SetSettingsForImplementation(object target, VisualBasicSettings value)
        {
            value.SuppressXamlSerialization = true;
            SetSettings(target, value);
        }

        public static bool ShouldSerializeSettings(object target)
        {
            VisualBasicSettings settings = GetSettings(target);
            if (((settings != null) && settings.SuppressXamlSerialization) && (target is Activity))
            {
                return false;
            }
            return true;
        }
    }
}

