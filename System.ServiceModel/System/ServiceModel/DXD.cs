namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal static class DXD
    {
        private static System.ServiceModel.AtomicTransactionExternal11Dictionary atomicTransactionExternal11Dictionary;
        private static System.ServiceModel.CoordinationExternal11Dictionary coordinationExternal11Dictionary;
        private static System.ServiceModel.SecureConversationDec2005Dictionary secureConversationDec2005Dictionary;
        private static System.ServiceModel.SecurityAlgorithmDec2005Dictionary securityAlgorithmDec2005Dictionary;
        private static System.ServiceModel.TrustDec2005Dictionary trustDec2005Dictionary;
        private static System.ServiceModel.Wsrm11Dictionary wsrm11Dictionary;

        static DXD()
        {
            XmlDictionary dictionary = new XmlDictionary(0x89);
            atomicTransactionExternal11Dictionary = new System.ServiceModel.AtomicTransactionExternal11Dictionary(dictionary);
            coordinationExternal11Dictionary = new System.ServiceModel.CoordinationExternal11Dictionary(dictionary);
            secureConversationDec2005Dictionary = new System.ServiceModel.SecureConversationDec2005Dictionary(dictionary);
            securityAlgorithmDec2005Dictionary = new System.ServiceModel.SecurityAlgorithmDec2005Dictionary(dictionary);
            trustDec2005Dictionary = new System.ServiceModel.TrustDec2005Dictionary(dictionary);
            wsrm11Dictionary = new System.ServiceModel.Wsrm11Dictionary(dictionary);
        }

        public static System.ServiceModel.AtomicTransactionExternal11Dictionary AtomicTransactionExternal11Dictionary
        {
            get
            {
                return atomicTransactionExternal11Dictionary;
            }
        }

        public static System.ServiceModel.CoordinationExternal11Dictionary CoordinationExternal11Dictionary
        {
            get
            {
                return coordinationExternal11Dictionary;
            }
        }

        public static System.ServiceModel.SecureConversationDec2005Dictionary SecureConversationDec2005Dictionary
        {
            get
            {
                return secureConversationDec2005Dictionary;
            }
        }

        public static System.ServiceModel.SecurityAlgorithmDec2005Dictionary SecurityAlgorithmDec2005Dictionary
        {
            get
            {
                return securityAlgorithmDec2005Dictionary;
            }
        }

        public static System.ServiceModel.TrustDec2005Dictionary TrustDec2005Dictionary
        {
            get
            {
                return trustDec2005Dictionary;
            }
        }

        public static System.ServiceModel.Wsrm11Dictionary Wsrm11Dictionary
        {
            get
            {
                return wsrm11Dictionary;
            }
        }
    }
}

