namespace System.IO
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    public class IODescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public IODescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = SR.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}

