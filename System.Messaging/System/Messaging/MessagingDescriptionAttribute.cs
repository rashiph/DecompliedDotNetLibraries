namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.All)]
    public class MessagingDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MessagingDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
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

