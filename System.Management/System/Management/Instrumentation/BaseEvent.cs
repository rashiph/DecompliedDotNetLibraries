namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime;

    [InstrumentationClass(InstrumentationType.Event)]
    public abstract class BaseEvent : IEvent
    {
        private ProvisionFunction fireFunction;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected BaseEvent()
        {
        }

        public void Fire()
        {
            this.FireFunction(this);
        }

        private ProvisionFunction FireFunction
        {
            get
            {
                if (this.fireFunction == null)
                {
                    this.fireFunction = System.Management.Instrumentation.Instrumentation.GetFireFunction(base.GetType());
                }
                return this.fireFunction;
            }
        }
    }
}

