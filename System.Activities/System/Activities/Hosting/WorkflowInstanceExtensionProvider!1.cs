namespace System.Activities.Hosting
{
    using System;

    internal class WorkflowInstanceExtensionProvider<T> : WorkflowInstanceExtensionProvider where T: class
    {
        private bool hasGeneratedValue;
        private Func<T> providerFunction;

        public WorkflowInstanceExtensionProvider(Func<T> providerFunction)
        {
            this.providerFunction = providerFunction;
            base.Type = typeof(T);
        }

        public override object ProvideValue()
        {
            T local = this.providerFunction();
            if (!this.hasGeneratedValue)
            {
                base.GeneratedTypeMatchesDeclaredType = object.ReferenceEquals(local.GetType(), base.Type);
                this.hasGeneratedValue = true;
            }
            return local;
        }
    }
}

