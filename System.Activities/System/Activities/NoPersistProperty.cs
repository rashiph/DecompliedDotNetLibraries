namespace System.Activities
{
    using System;
    using System.Activities.Runtime;

    internal class NoPersistProperty : IPropertyRegistrationCallback
    {
        private ActivityExecutor executor;
        public const string Name = "System.Activities.NoPersistProperty";
        private int refCount;

        public NoPersistProperty(ActivityExecutor executor)
        {
            this.executor = executor;
        }

        public void Enter()
        {
            this.refCount++;
            this.executor.EnterNoPersist();
        }

        public bool Exit()
        {
            this.refCount--;
            this.executor.ExitNoPersist();
            return (this.refCount == 0);
        }

        public void Register(RegistrationContext context)
        {
        }

        public void Unregister(RegistrationContext context)
        {
            if (this.refCount > 0)
            {
                for (int i = 0; i < this.refCount; i++)
                {
                    this.executor.ExitNoPersist();
                }
                this.refCount = 0;
            }
        }
    }
}

