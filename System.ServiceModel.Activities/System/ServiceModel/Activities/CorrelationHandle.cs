namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class CorrelationHandle : Handle
    {
        [DataMember]
        private BookmarkScopeHandle bookmarkScopeHandle;
        private CorrelationCallbackContext callbackContext;
        private static readonly Type callbackCorrelationInitializerType = typeof(CallbackCorrelationInitializer);
        private CorrelationContext context;
        private static readonly Type contextCorrelationInitializerType = typeof(ContextCorrelationInitializer);
        [DataMember]
        private NoPersistHandle noPersistHandle;
        private static readonly Type requestReplyCorrelationInitializerType = typeof(RequestReplyCorrelationInitializer);
        internal static readonly string StaticExecutionPropertyName = typeof(CorrelationHandle).FullName;

        internal BookmarkScope EnsureBookmarkScope(NativeActivityContext executionContext)
        {
            if (this.Scope == null)
            {
                this.Scope = executionContext.DefaultBookmarkScope;
            }
            return this.Scope;
        }

        internal static CorrelationHandle GetExplicitCallbackCorrelation(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers)
        {
            return GetTypedCorrelationHandle(context, correlationInitializers, callbackCorrelationInitializerType);
        }

        internal static CorrelationHandle GetExplicitChannelCorrelation(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers)
        {
            return GetTypedCorrelationHandle(context, correlationInitializers, requestReplyCorrelationInitializerType);
        }

        internal static CorrelationHandle GetExplicitContextCorrelation(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers)
        {
            return GetTypedCorrelationHandle(context, correlationInitializers, contextCorrelationInitializerType);
        }

        internal static CorrelationHandle GetTypedCorrelationHandle(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers, Type correlationInitializerType)
        {
            if ((correlationInitializers != null) && (correlationInitializers.Count > 0))
            {
                foreach (CorrelationInitializer initializer in correlationInitializers)
                {
                    if (correlationInitializerType == initializer.GetType())
                    {
                        return initializer.CorrelationHandle.Get(context);
                    }
                }
            }
            return null;
        }

        internal void InitializeBookmarkScope(NativeActivityContext context, InstanceKey instanceKey)
        {
            if (this.Scope == null)
            {
                this.bookmarkScopeHandle.CreateBookmarkScope(context, instanceKey.Value);
                this.Scope = this.bookmarkScopeHandle.BookmarkScope;
            }
            else if (this.Scope.IsInitialized)
            {
                if (this.Scope.Id != instanceKey.Value)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CorrelationHandleInUse(this.Scope.Id, instanceKey.Value)));
                }
            }
            else
            {
                this.Scope.Initialize(context, instanceKey.Value);
            }
        }

        internal bool IsInitalized()
        {
            if (((this.Scope == null) && (this.CallbackContext == null)) && (((this.Context == null) && (this.ResponseContext == null)) && (this.RequestContext == null)))
            {
                return false;
            }
            return true;
        }

        protected override void OnInitialize(HandleInitializationContext context)
        {
            this.noPersistHandle = context.CreateAndInitializeHandle<NoPersistHandle>();
            this.bookmarkScopeHandle = context.CreateAndInitializeHandle<BookmarkScopeHandle>();
        }

        protected override void OnUninitialize(HandleInitializationContext context)
        {
            context.UninitializeHandle(this.noPersistHandle);
            context.UninitializeHandle(this.bookmarkScopeHandle);
        }

        internal bool TryAcquireRequestContext(NativeActivityContext executionContext, out CorrelationRequestContext requestContext)
        {
            if (this.RequestContext != null)
            {
                this.noPersistHandle.Exit(executionContext);
                requestContext = this.RequestContext;
                this.RequestContext = null;
                return true;
            }
            requestContext = null;
            return false;
        }

        internal bool TryAcquireResponseContext(NativeActivityContext executionContext, out CorrelationResponseContext responseContext)
        {
            if (this.ResponseContext != null)
            {
                this.noPersistHandle.Exit(executionContext);
                responseContext = this.ResponseContext;
                this.ResponseContext = null;
                return true;
            }
            responseContext = null;
            return false;
        }

        internal bool TryRegisterRequestContext(NativeActivityContext executionContext, CorrelationRequestContext requestContext)
        {
            if (this.noPersistHandle == null)
            {
                return false;
            }
            if (this.RequestContext == null)
            {
                this.noPersistHandle.Enter(executionContext);
                this.RequestContext = requestContext;
                return true;
            }
            return object.ReferenceEquals(this.RequestContext, requestContext);
        }

        internal bool TryRegisterResponseContext(NativeActivityContext executionContext, CorrelationResponseContext responseContext)
        {
            if (this.noPersistHandle == null)
            {
                return false;
            }
            if (this.ResponseContext == null)
            {
                this.noPersistHandle.Enter(executionContext);
                this.ResponseContext = responseContext;
                return true;
            }
            return object.ReferenceEquals(this.ResponseContext, responseContext);
        }

        [DataMember(EmitDefaultValue=false)]
        internal CorrelationCallbackContext CallbackContext
        {
            get
            {
                return this.callbackContext;
            }
            set
            {
                this.callbackContext = value;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        internal CorrelationContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        internal CorrelationRequestContext RequestContext { get; private set; }

        internal CorrelationResponseContext ResponseContext { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        internal BookmarkScope Scope { get; set; }
    }
}

