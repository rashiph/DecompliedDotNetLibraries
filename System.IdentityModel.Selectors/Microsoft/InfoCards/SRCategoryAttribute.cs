namespace Microsoft.InfoCards
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRCategoryAttribute : CategoryAttribute
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SRCategoryAttribute(string category) : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return Microsoft.InfoCards.SR.GetString(value);
        }
    }
}

