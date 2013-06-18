namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime;

    [InstrumentationClass(InstrumentationType.Instance)]
    public abstract class Instance : IInstance
    {
        private bool published;
        private ProvisionFunction publishFunction;
        private ProvisionFunction revokeFunction;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected Instance()
        {
        }

        [IgnoreMember]
        public bool Published
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.published;
            }
            set
            {
                if (this.published && !value)
                {
                    this.RevokeFunction(this);
                    this.published = false;
                }
                else if (!this.published && value)
                {
                    this.PublishFunction(this);
                    this.published = true;
                }
            }
        }

        private ProvisionFunction PublishFunction
        {
            get
            {
                if (this.publishFunction == null)
                {
                    this.publishFunction = System.Management.Instrumentation.Instrumentation.GetPublishFunction(base.GetType());
                }
                return this.publishFunction;
            }
        }

        private ProvisionFunction RevokeFunction
        {
            get
            {
                if (this.revokeFunction == null)
                {
                    this.revokeFunction = System.Management.Instrumentation.Instrumentation.GetRevokeFunction(base.GetType());
                }
                return this.revokeFunction;
            }
        }
    }
}

