namespace System.Timers
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    public class TimersDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public TimersDescriptionAttribute(string description) : base(description)
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

