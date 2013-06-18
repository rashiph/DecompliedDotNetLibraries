namespace Microsoft.Transactions.Wsat.Messaging
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal class CoordinationXmlDictionaryStrings10 : CoordinationXmlDictionaryStrings
    {
        private static CoordinationXmlDictionaryStrings instance = new CoordinationXmlDictionaryStrings10();

        public override XmlDictionaryString CreateCoordinationContextAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.CreateCoordinationContextAction;
            }
        }

        public override XmlDictionaryString CreateCoordinationContextResponseAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.CreateCoordinationContextResponseAction;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.FaultAction;
            }
        }

        public static CoordinationXmlDictionaryStrings Instance
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
                return XD.CoordinationExternal10Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString RegisterAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.RegisterAction;
            }
        }

        public override XmlDictionaryString RegisterResponseAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.RegisterResponseAction;
            }
        }
    }
}

