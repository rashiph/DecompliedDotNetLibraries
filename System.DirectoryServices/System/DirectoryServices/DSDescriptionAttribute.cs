namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    public class DSDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public DSDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = Res.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}

