namespace System.Activities.Hosting
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal abstract class WorkflowInstanceExtensionProvider
    {
        protected WorkflowInstanceExtensionProvider()
        {
        }

        public bool IsMatch<TTarget>(object value) where TTarget: class
        {
            return ((value is TTarget) && (this.GeneratedTypeMatchesDeclaredType || TypeHelper.AreReferenceTypesCompatible(this.Type, typeof(TTarget))));
        }

        public abstract object ProvideValue();

        protected bool GeneratedTypeMatchesDeclaredType { get; set; }

        public System.Type Type { get; protected set; }
    }
}

