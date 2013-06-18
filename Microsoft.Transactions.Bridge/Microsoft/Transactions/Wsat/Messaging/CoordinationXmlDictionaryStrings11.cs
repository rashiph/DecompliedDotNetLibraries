namespace Microsoft.Transactions.Wsat.Messaging
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal class CoordinationXmlDictionaryStrings11 : CoordinationXmlDictionaryStrings
    {
        private static CoordinationXmlDictionaryStrings instance = new CoordinationXmlDictionaryStrings11();

        public override XmlDictionaryString CreateCoordinationContextAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CreateCoordinationContextAction;
            }
        }

        public override XmlDictionaryString CreateCoordinationContextResponseAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CreateCoordinationContextResponseAction;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.FaultAction;
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
                return DXD.CoordinationExternal11Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString RegisterAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.RegisterAction;
            }
        }

        public override XmlDictionaryString RegisterResponseAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.RegisterResponseAction;
            }
        }
    }
}

