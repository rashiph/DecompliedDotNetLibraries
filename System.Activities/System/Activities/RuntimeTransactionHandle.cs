namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Transactions;

    [DataContract]
    public sealed class RuntimeTransactionHandle : Handle, IExecutionProperty, IPropertyRegistrationCallback
    {
        [DataMember(EmitDefaultValue=false)]
        private bool doNotAbort;
        [DataMember]
        private ActivityExecutor executor;
        [DataMember(EmitDefaultValue=false)]
        private bool isHandleInitialized;
        [DataMember(EmitDefaultValue=false)]
        private bool isPropertyRegistered;
        [DataMember(EmitDefaultValue=false)]
        private bool isSuppressed;
        private Transaction rootTransaction;
        private TransactionScope scope;

        public RuntimeTransactionHandle()
        {
        }

        public RuntimeTransactionHandle(Transaction rootTransaction)
        {
            if (rootTransaction == null)
            {
                throw FxTrace.Exception.ArgumentNull("rootTransaction");
            }
            this.rootTransaction = rootTransaction;
            this.AbortInstanceOnTransactionFailure = false;
        }

        public void CompleteTransaction(NativeActivityContext context)
        {
            this.CompleteTransactionCore(context, null);
        }

        public void CompleteTransaction(NativeActivityContext context, BookmarkCallback callback)
        {
            if (callback == null)
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }
            this.CompleteTransactionCore(context, callback);
        }

        private void CompleteTransactionCore(NativeActivityContext context, BookmarkCallback callback)
        {
            context.ThrowIfDisposed();
            if (this.rootTransaction != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotCompleteRuntimeOwnedTransaction));
            }
            if (!context.HasRuntimeTransaction)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.NoRuntimeTransactionExists));
            }
            if (!this.isHandleInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.UnInitializedRuntimeTransactionHandle));
            }
            if (this.SuppressTransaction)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RuntimeTransactionIsSuppressed));
            }
            context.CompleteTransaction(this, callback);
        }

        public Transaction GetCurrentTransaction(AsyncCodeActivityContext context)
        {
            return this.GetCurrentTransactionCore(context);
        }

        public Transaction GetCurrentTransaction(CodeActivityContext context)
        {
            return this.GetCurrentTransactionCore(context);
        }

        public Transaction GetCurrentTransaction(NativeActivityContext context)
        {
            return this.GetCurrentTransactionCore(context);
        }

        private Transaction GetCurrentTransactionCore(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.ThrowIfDisposed();
            if (this.rootTransaction == null)
            {
                this.ThrowIfNotRegistered(System.Activities.SR.RuntimeTransactionHandleNotRegisteredAsExecutionProperty("GetCurrentTransaction"));
            }
            if (!this.isHandleInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.UnInitializedRuntimeTransactionHandle));
            }
            if (this.SuppressTransaction)
            {
                return null;
            }
            return this.executor.CurrentTransaction;
        }

        protected override void OnInitialize(HandleInitializationContext context)
        {
            this.executor = context.Executor;
            this.isHandleInitialized = true;
            if (this.rootTransaction != null)
            {
                this.executor.SetTransaction(this, this.rootTransaction, null, null);
            }
            base.OnInitialize(context);
        }

        protected override void OnUninitialize(HandleInitializationContext context)
        {
            if (this.rootTransaction != null)
            {
                this.executor.ExitNoPersist();
            }
            this.isHandleInitialized = false;
            base.OnUninitialize(context);
        }

        private void RequestOrRequireTransactionContextCore(NativeActivityContext context, Action<NativeActivityTransactionContext, object> callback, object state, bool isRequires)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.ThrowIfDisposed();
            if (context.HasRuntimeTransaction)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RuntimeTransactionAlreadyExists));
            }
            if (context.IsInNoPersistScope)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotSetRuntimeTransactionInNoPersist));
            }
            if (!this.isHandleInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.UnInitializedRuntimeTransactionHandle));
            }
            if (this.SuppressTransaction)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RuntimeTransactionIsSuppressed));
            }
            if (isRequires)
            {
                if (context.RequiresTransactionContextWaiterExists)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.OnlyOneRequireTransactionContextAllowed));
                }
                this.ThrowIfNotRegistered(System.Activities.SR.RuntimeTransactionHandleNotRegisteredAsExecutionProperty("RequireTransactionContext"));
            }
            else
            {
                this.ThrowIfNotRegistered(System.Activities.SR.RuntimeTransactionHandleNotRegisteredAsExecutionProperty("RequestTransactionContext"));
            }
            context.RequestTransactionContext(isRequires, this, callback, state);
        }

        public void RequestTransactionContext(NativeActivityContext context, Action<NativeActivityTransactionContext, object> callback, object state)
        {
            this.RequestOrRequireTransactionContextCore(context, callback, state, false);
        }

        public void RequireTransactionContext(NativeActivityContext context, Action<NativeActivityTransactionContext, object> callback, object state)
        {
            this.RequestOrRequireTransactionContextCore(context, callback, state, true);
        }

        void IExecutionProperty.CleanupWorkflowThread()
        {
            Fx.CompleteTransactionScope(ref this.scope);
        }

        void IExecutionProperty.SetupWorkflowThread()
        {
            if (this.SuppressTransaction)
            {
                this.scope = new TransactionScope(TransactionScopeOption.Suppress);
            }
            else if ((this.executor != null) && this.executor.HasRuntimeTransaction)
            {
                this.scope = Fx.CreateTransactionScope(this.executor.CurrentTransaction);
            }
        }

        void IPropertyRegistrationCallback.Register(RegistrationContext context)
        {
            if (!this.isHandleInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.UnInitializedRuntimeTransactionHandle));
            }
            RuntimeTransactionHandle handle = (RuntimeTransactionHandle) context.FindProperty(typeof(RuntimeTransactionHandle).FullName);
            if ((handle != null) && handle.SuppressTransaction)
            {
                this.isSuppressed = true;
            }
            this.isPropertyRegistered = true;
        }

        void IPropertyRegistrationCallback.Unregister(RegistrationContext context)
        {
            this.isPropertyRegistered = false;
        }

        private void ThrowIfNotRegistered(string message)
        {
            if (!this.isPropertyRegistered)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(message));
            }
        }

        private void ThrowIfRegistered(string message)
        {
            if (this.isPropertyRegistered)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(message));
            }
        }

        public bool AbortInstanceOnTransactionFailure
        {
            get
            {
                return !this.doNotAbort;
            }
            set
            {
                this.ThrowIfRegistered(System.Activities.SR.CannotChangeAbortInstanceFlagAfterPropertyRegistration);
                this.doNotAbort = !value;
            }
        }

        internal bool IsRuntimeOwnedTransaction
        {
            get
            {
                return (this.rootTransaction != null);
            }
        }

        public bool SuppressTransaction
        {
            get
            {
                return this.isSuppressed;
            }
            set
            {
                this.ThrowIfRegistered(System.Activities.SR.CannotSuppressAlreadyRegisteredHandle);
                this.isSuppressed = value;
            }
        }
    }
}

