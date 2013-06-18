namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class TraceSRDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TraceSRDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = TraceSR.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}

