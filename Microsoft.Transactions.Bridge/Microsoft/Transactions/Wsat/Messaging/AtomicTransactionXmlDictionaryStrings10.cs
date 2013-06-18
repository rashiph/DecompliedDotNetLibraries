namespace Microsoft.Transactions.Wsat.Messaging
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal class AtomicTransactionXmlDictionaryStrings10 : AtomicTransactionXmlDictionaryStrings
    {
        private static AtomicTransactionXmlDictionaryStrings instance = new AtomicTransactionXmlDictionaryStrings10();

        public override XmlDictionaryString AbortedAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.AbortedAction;
            }
        }

        public override XmlDictionaryString CommitAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.CommitAction;
            }
        }

        public override XmlDictionaryString CommittedAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.CommittedAction;
            }
        }

        public override XmlDictionaryString CompletionUri
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.CompletionUri;
            }
        }

        public override XmlDictionaryString Durable2PCUri
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.Durable2PCUri;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.FaultAction;
            }
        }

        public static AtomicTransactionXmlDictionaryStrings Instance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return instance;
            }
        }

        public override XmlDictionaryString Namespace
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString PrepareAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.PrepareAction;
            }
        }

        public override XmlDictionaryString PreparedAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.PreparedAction;
            }
        }

        public override XmlDictionaryString ReadOnlyAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.ReadOnlyAction;
            }
        }

        public override XmlDictionaryString ReplayAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.ReplayAction;
            }
        }

        public override XmlDictionaryString RollbackAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.RollbackAction;
            }
        }

        public override XmlDictionaryString Volatile2PCUri
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.Volatile2PCUri;
            }
        }
    }
}

