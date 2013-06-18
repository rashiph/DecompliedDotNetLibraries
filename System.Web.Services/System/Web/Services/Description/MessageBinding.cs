namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;

    public abstract class MessageBinding : NamedItem
    {
        private System.Web.Services.Description.OperationBinding parent;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MessageBinding()
        {
        }

        internal void SetParent(System.Web.Services.Description.OperationBinding parent)
        {
            this.parent = parent;
        }

        public System.Web.Services.Description.OperationBinding OperationBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }
    }
}

