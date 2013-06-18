namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class TraceSRCategoryAttribute : CategoryAttribute
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TraceSRCategoryAttribute(string category) : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return TraceSR.GetString(value);
        }
    }
}

