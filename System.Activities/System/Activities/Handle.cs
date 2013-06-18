namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class Handle
    {
        [DataMember(EmitDefaultValue=false)]
        private bool isUninitialized = true;
        [DataMember(EmitDefaultValue=false)]
        private System.Activities.ActivityInstance owner;

        protected Handle()
        {
        }

        internal static string GetPropertyName(Type handleType)
        {
            return handleType.FullName;
        }

        internal void Initialize(HandleInitializationContext context)
        {
            this.owner = context.OwningActivityInstance;
            this.isUninitialized = false;
            this.OnInitialize(context);
        }

        protected virtual void OnInitialize(HandleInitializationContext context)
        {
        }

        protected virtual void OnUninitialize(HandleInitializationContext context)
        {
        }

        internal void Reinitialize(System.Activities.ActivityInstance owner)
        {
            this.owner = owner;
        }

        protected void ThrowIfUninitialized()
        {
            if (this.isUninitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.HandleNotInitialized));
            }
        }

        internal void Uninitialize(HandleInitializationContext context)
        {
            this.OnUninitialize(context);
            this.isUninitialized = true;
        }

        [DataMember(EmitDefaultValue=false)]
        internal bool CanBeRemovedWithExecutingChildren { get; set; }

        public string ExecutionPropertyName
        {
            get
            {
                return base.GetType().FullName;
            }
        }

        internal bool IsInitialized
        {
            get
            {
                return !this.isUninitialized;
            }
        }

        public System.Activities.ActivityInstance Owner
        {
            get
            {
                return this.owner;
            }
        }
    }
}

