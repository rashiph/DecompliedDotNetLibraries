namespace System.Runtime.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SRDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = System.Runtime.Serialization.SR.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}

