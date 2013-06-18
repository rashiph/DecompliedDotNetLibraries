namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class AtomicTransactionXmlDictionaryStrings
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected AtomicTransactionXmlDictionaryStrings()
        {
        }

        public static AtomicTransactionXmlDictionaryStrings Version(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(AtomicTransactionXmlDictionaryStrings), "V");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return AtomicTransactionXmlDictionaryStrings10.Instance;

                case ProtocolVersion.Version11:
                    return AtomicTransactionXmlDictionaryStrings11.Instance;
            }
            return null;
        }

        public XmlDictionaryString Aborted
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Aborted;
            }
        }

        public abstract XmlDictionaryString AbortedAction { get; }

        public XmlDictionaryString Commit
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Commit;
            }
        }

        public abstract XmlDictionaryString CommitAction { get; }

        public XmlDictionaryString Committed
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Committed;
            }
        }

        public abstract XmlDictionaryString CommittedAction { get; }

        public XmlDictionaryString CompletionCoordinatorPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.CompletionCoordinatorPortType;
            }
        }

        public XmlDictionaryString CompletionParticipantPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.CompletionParticipantPortType;
            }
        }

        public abstract XmlDictionaryString CompletionUri { get; }

        public XmlDictionaryString CoordinatorPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.CoordinatorPortType;
            }
        }

        public abstract XmlDictionaryString Durable2PCUri { get; }

        public abstract XmlDictionaryString FaultAction { get; }

        public XmlDictionaryString InconsistentInternalState
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.InconsistentInternalState;
            }
        }

        public abstract XmlDictionaryString Namespace { get; }

        public XmlDictionaryString ParticipantPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.ParticipantPortType;
            }
        }

        public XmlDictionaryString Prefix
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Prefix;
            }
        }

        public XmlDictionaryString Prepare
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Prepare;
            }
        }

        public abstract XmlDictionaryString PrepareAction { get; }

        public XmlDictionaryString Prepared
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Prepared;
            }
        }

        public abstract XmlDictionaryString PreparedAction { get; }

        public XmlDictionaryString ReadOnly
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.ReadOnly;
            }
        }

        public abstract XmlDictionaryString ReadOnlyAction { get; }

        public XmlDictionaryString Replay
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Replay;
            }
        }

        public abstract XmlDictionaryString ReplayAction { get; }

        public XmlDictionaryString Rollback
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Rollback;
            }
        }

        public abstract XmlDictionaryString RollbackAction { get; }

        public XmlDictionaryString UnknownTransaction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.UnknownTransaction;
            }
        }

        public abstract XmlDictionaryString Volatile2PCUri { get; }
    }
}

