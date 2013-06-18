namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    [DebuggerDisplay("Name={name}, IsInitiating={isInitiating}, IsTerminating={isTerminating}")]
    public class OperationDescription
    {
        private MethodInfo beginMethod;
        private KeyedByTypeCollection<IOperationBehavior> behaviors;
        private ContractDescription declaringContract;
        private MethodInfo endMethod;
        private FaultDescriptionCollection faults;
        private bool hasNoDisposableParameters;
        private bool hasProtectionLevel;
        private bool isInitiating;
        private bool isTerminating;
        private Collection<Type> knownTypes;
        private MessageDescriptionCollection messages;
        private System.ServiceModel.Description.XmlName name;
        private System.Net.Security.ProtectionLevel protectionLevel;
        private MethodInfo syncMethod;
        private bool validateRpcWrapperName;

        public OperationDescription(string name, ContractDescription declaringContract)
        {
            this.validateRpcWrapperName = true;
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (name.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("name", System.ServiceModel.SR.GetString("SFxOperationDescriptionNameCannotBeEmpty")));
            }
            this.name = new System.ServiceModel.Description.XmlName(name, true);
            if (declaringContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("declaringContract");
            }
            this.declaringContract = declaringContract;
            this.isInitiating = true;
            this.isTerminating = false;
            this.faults = new FaultDescriptionCollection();
            this.messages = new MessageDescriptionCollection();
            this.behaviors = new KeyedByTypeCollection<IOperationBehavior>();
            this.knownTypes = new Collection<Type>();
        }

        internal OperationDescription(string name, ContractDescription declaringContract, bool validateRpcWrapperName) : this(name, declaringContract)
        {
            this.validateRpcWrapperName = validateRpcWrapperName;
        }

        internal void EnsureInvariants()
        {
            if ((this.Messages.Count != 1) && (this.Messages.Count != 2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxOperationMustHaveOneOrTwoMessages", new object[] { this.Name })));
            }
        }

        internal bool IsServerInitiated()
        {
            this.EnsureInvariants();
            return (this.Messages[0].Direction == MessageDirection.Output);
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = System.Net.Security.ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }

        public bool ShouldSerializeProtectionLevel()
        {
            return this.HasProtectionLevel;
        }

        public MethodInfo BeginMethod
        {
            get
            {
                return this.beginMethod;
            }
            set
            {
                this.beginMethod = value;
            }
        }

        public KeyedByTypeCollection<IOperationBehavior> Behaviors
        {
            get
            {
                return this.behaviors;
            }
        }

        internal string CodeName
        {
            get
            {
                return this.name.DecodedName;
            }
        }

        public ContractDescription DeclaringContract
        {
            get
            {
                return this.declaringContract;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("DeclaringContract");
                }
                this.declaringContract = value;
            }
        }

        public MethodInfo EndMethod
        {
            get
            {
                return this.endMethod;
            }
            set
            {
                this.endMethod = value;
            }
        }

        public FaultDescriptionCollection Faults
        {
            get
            {
                return this.faults;
            }
        }

        internal bool HasNoDisposableParameters
        {
            get
            {
                return this.hasNoDisposableParameters;
            }
            set
            {
                this.hasNoDisposableParameters = value;
            }
        }

        public bool HasProtectionLevel
        {
            get
            {
                return this.hasProtectionLevel;
            }
        }

        internal bool IsFirstReceiveOfTransactedReceiveScopeTree { get; set; }

        [DefaultValue(false)]
        public bool IsInitiating
        {
            get
            {
                return this.isInitiating;
            }
            set
            {
                this.isInitiating = value;
            }
        }

        internal bool IsInsideTransactedReceiveScope { get; set; }

        public bool IsOneWay
        {
            get
            {
                return (this.Messages.Count == 1);
            }
        }

        [DefaultValue(false)]
        public bool IsTerminating
        {
            get
            {
                return this.isTerminating;
            }
            set
            {
                this.isTerminating = value;
            }
        }

        internal bool IsValidateRpcWrapperName
        {
            get
            {
                return this.validateRpcWrapperName;
            }
        }

        public Collection<Type> KnownTypes
        {
            get
            {
                return this.knownTypes;
            }
        }

        public MessageDescriptionCollection Messages
        {
            get
            {
                return this.messages;
            }
        }

        public string Name
        {
            get
            {
                return this.name.EncodedName;
            }
        }

        internal MethodInfo OperationMethod
        {
            get
            {
                return (this.SyncMethod ?? this.BeginMethod);
            }
        }

        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public MethodInfo SyncMethod
        {
            get
            {
                return this.syncMethod;
            }
            set
            {
                this.syncMethod = value;
            }
        }

        internal System.ServiceModel.Description.XmlName XmlName
        {
            get
            {
                return this.name;
            }
        }
    }
}

