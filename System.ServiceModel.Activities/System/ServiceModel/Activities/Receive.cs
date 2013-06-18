namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.XamlIntegration;
    using System.Windows.Markup;
    using System.Xml.Linq;

    [ContentProperty("Content")]
    public sealed class Receive : Activity
    {
        private MessageQuerySet correlatesOn;
        private Collection<CorrelationInitializer> correlationInitializers;
        private IList<SendReply> followingFaults;
        private IList<SendReply> followingReplies;
        private InternalReceiveMessage internalReceive;
        private Collection<Type> knownTypes;
        private FromRequest requestFormatter;

        public Receive()
        {
            Func<Activity> func = null;
            if (func == null)
            {
                func = delegate {
                    if (this.internalReceive == null)
                    {
                        return null;
                    }
                    if (this.requestFormatter == null)
                    {
                        return this.internalReceive;
                    }
                    Variable<Message> variable = new Variable<Message> {
                        Name = "RequestMessage"
                    };
                    Variable<NoPersistHandle> variable2 = new Variable<NoPersistHandle> {
                        Name = "ReceiveNoPersistHandle"
                    };
                    this.internalReceive.Message = new OutArgument<Message>(variable);
                    this.requestFormatter.Message = new InOutArgument<Message>(variable);
                    this.internalReceive.NoPersistHandle = new InArgument<NoPersistHandle>(variable2);
                    this.requestFormatter.NoPersistHandle = new InArgument<NoPersistHandle>(variable2);
                    return new Sequence { Variables = { variable, variable2 }, Activities = { this.internalReceive, this.requestFormatter } };
                };
            }
            base.Implementation = func;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(this.OperationName))
            {
                metadata.AddValidationError(System.ServiceModel.Activities.SR.MissingOperationName(base.DisplayName));
            }
            MessagingActivityHelper.ValidateCorrelationInitializer(metadata, this.correlationInitializers, false, base.DisplayName, this.OperationName);
            MessagingActivityHelper.AddRuntimeArgument(this.CorrelatesWith, "CorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In, metadata);
            this.InternalContent.CacheMetadata(metadata, this, this.OperationName);
            if (this.correlationInitializers != null)
            {
                for (int i = 0; i < this.correlationInitializers.Count; i++)
                {
                    CorrelationInitializer initializer = this.correlationInitializers[i];
                    initializer.ArgumentName = "Parameter" + i;
                    RuntimeArgument argument = new RuntimeArgument(initializer.ArgumentName, Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(initializer.CorrelationHandle, argument);
                    metadata.AddArgument(argument);
                }
            }
            if (!metadata.HasViolations)
            {
                this.internalReceive = this.CreateInternalReceive();
                this.InternalContent.ConfigureInternalReceive(this.internalReceive, out this.requestFormatter);
            }
            else
            {
                this.internalReceive = null;
                this.requestFormatter = null;
            }
        }

        private InternalReceiveMessage CreateInternalReceive()
        {
            InternalReceiveMessage message2 = new InternalReceiveMessage {
                OperationName = this.OperationName,
                ServiceContractName = this.ServiceContractName
            };
            ArgumentValue<CorrelationHandle> expression = new ArgumentValue<CorrelationHandle> {
                ArgumentName = "CorrelatesWith"
            };
            message2.CorrelatesWith = new InArgument<CorrelationHandle>(expression);
            message2.IsOneWay = true;
            InternalReceiveMessage message = message2;
            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer initializer in this.correlationInitializers)
                {
                    message.CorrelationInitializers.Add(initializer.Clone());
                }
            }
            return message;
        }

        internal void SetFormatter(IDispatchMessageFormatter formatter, IDispatchFaultFormatter faultFormatter, bool includeExceptionDetailInFaults)
        {
            this.requestFormatter.Formatter = formatter;
            if (this.followingReplies != null)
            {
                for (int i = 0; i < this.followingReplies.Count; i++)
                {
                    this.followingReplies[i].SetFormatter(formatter);
                }
            }
            if (this.followingFaults != null)
            {
                for (int j = 0; j < this.followingFaults.Count; j++)
                {
                    this.followingFaults[j].SetFaultFormatter(faultFormatter, includeExceptionDetailInFaults);
                }
            }
        }

        internal void SetIsOneWay(bool flag)
        {
            this.internalReceive.IsOneWay = flag;
            if (!this.internalReceive.IsOneWay)
            {
                this.internalReceive.NoPersistHandle = null;
                if (this.requestFormatter != null)
                {
                    this.requestFormatter.NoPersistHandle = null;
                }
            }
            else if (this.requestFormatter != null)
            {
                this.requestFormatter.CloseMessage = true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeCorrelatesOn()
        {
            return ((this.correlatesOn != null) && ((this.correlatesOn.Name != null) || (this.correlatesOn.Count != 0)));
        }

        [DefaultValue((string) null)]
        public string Action { get; set; }

        [DefaultValue(false)]
        public bool CanCreateInstance { get; set; }

        [DefaultValue((string) null)]
        public ReceiveContent Content { get; set; }

        public MessageQuerySet CorrelatesOn
        {
            get
            {
                if (this.correlatesOn == null)
                {
                    this.correlatesOn = new MessageQuerySet();
                }
                return this.correlatesOn;
            }
            set
            {
                this.correlatesOn = value;
            }
        }

        [DefaultValue((string) null)]
        public InArgument<CorrelationHandle> CorrelatesWith { get; set; }

        public Collection<CorrelationInitializer> CorrelationInitializers
        {
            get
            {
                if (this.correlationInitializers == null)
                {
                    this.correlationInitializers = new Collection<CorrelationInitializer>();
                }
                return this.correlationInitializers;
            }
        }

        internal IList<SendReply> FollowingFaults
        {
            get
            {
                if (this.followingFaults == null)
                {
                    this.followingFaults = new List<SendReply>();
                }
                return this.followingFaults;
            }
        }

        internal IList<SendReply> FollowingReplies
        {
            get
            {
                if (this.followingReplies == null)
                {
                    this.followingReplies = new List<SendReply>();
                }
                return this.followingReplies;
            }
        }

        internal bool HasCorrelatesOn
        {
            get
            {
                return ((this.correlatesOn != null) && (this.correlatesOn.Count > 0));
            }
        }

        internal bool HasCorrelationInitializers
        {
            get
            {
                return ((this.correlationInitializers != null) && (this.correlationInitializers.Count > 0));
            }
        }

        internal bool HasFault
        {
            get
            {
                return ((this.followingFaults != null) && (this.followingFaults.Count > 0));
            }
        }

        internal bool HasReply
        {
            get
            {
                return ((this.followingReplies != null) && (this.followingReplies.Count > 0));
            }
        }

        internal ReceiveContent InternalContent
        {
            get
            {
                return (this.Content ?? ReceiveContent.DefaultReceiveContent);
            }
        }

        internal Collection<Type> InternalKnownTypes
        {
            get
            {
                return this.knownTypes;
            }
        }

        internal InternalReceiveMessage InternalReceive
        {
            get
            {
                return this.internalReceive;
            }
        }

        public Collection<Type> KnownTypes
        {
            get
            {
                if (this.knownTypes == null)
                {
                    this.knownTypes = new Collection<Type>();
                }
                return this.knownTypes;
            }
        }

        internal string OperationBookmarkName
        {
            get
            {
                return this.internalReceive.OperationBookmarkName;
            }
        }

        [DefaultValue((string) null)]
        public string OperationName { get; set; }

        [DefaultValue((string) null)]
        public System.Net.Security.ProtectionLevel? ProtectionLevel { get; set; }

        [DefaultValue(0)]
        public System.ServiceModel.Activities.SerializerOption SerializerOption { get; set; }

        [TypeConverter(typeof(ServiceXNameTypeConverter)), DefaultValue((string) null)]
        public XName ServiceContractName { get; set; }
    }
}

