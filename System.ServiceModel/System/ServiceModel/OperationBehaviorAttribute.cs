namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OperationBehaviorAttribute : Attribute, IOperationBehavior
    {
        private bool autoCompleteTransaction = true;
        private bool autoDisposeParameters = true;
        private bool autoEnlistTransaction;
        internal const ImpersonationOption DefaultImpersonationOption = ImpersonationOption.NotAllowed;
        private ImpersonationOption impersonation;
        private bool preferAsyncInvocation;
        private System.ServiceModel.ReleaseInstanceMode releaseInstance;

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (description.IsServerInitiated() && (this.releaseInstance != System.ServiceModel.ReleaseInstanceMode.None))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxOperationBehaviorAttributeReleaseInstanceModeDoesNotApplyToCallback", new object[] { description.Name })));
            }
            dispatch.TransactionRequired = this.autoEnlistTransaction;
            dispatch.TransactionAutoComplete = this.autoCompleteTransaction;
            dispatch.AutoDisposeParameters = this.autoDisposeParameters;
            dispatch.ReleaseInstanceBeforeCall = (this.releaseInstance & System.ServiceModel.ReleaseInstanceMode.BeforeCall) != System.ServiceModel.ReleaseInstanceMode.None;
            dispatch.ReleaseInstanceAfterCall = (this.releaseInstance & System.ServiceModel.ReleaseInstanceMode.AfterCall) != System.ServiceModel.ReleaseInstanceMode.None;
            dispatch.Impersonation = this.Impersonation;
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        public bool AutoDisposeParameters
        {
            get
            {
                return this.autoDisposeParameters;
            }
            set
            {
                this.autoDisposeParameters = value;
            }
        }

        public ImpersonationOption Impersonation
        {
            get
            {
                return this.impersonation;
            }
            set
            {
                if (!ImpersonationOptionHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.impersonation = value;
            }
        }

        internal bool PreferAsyncInvocation
        {
            get
            {
                return this.preferAsyncInvocation;
            }
            set
            {
                this.preferAsyncInvocation = value;
            }
        }

        public System.ServiceModel.ReleaseInstanceMode ReleaseInstanceMode
        {
            get
            {
                return this.releaseInstance;
            }
            set
            {
                if (!ReleaseInstanceModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.releaseInstance = value;
            }
        }

        public bool TransactionAutoComplete
        {
            get
            {
                return this.autoCompleteTransaction;
            }
            set
            {
                this.autoCompleteTransaction = value;
            }
        }

        public bool TransactionScopeRequired
        {
            get
            {
                return this.autoEnlistTransaction;
            }
            set
            {
                this.autoEnlistTransaction = value;
            }
        }
    }
}

