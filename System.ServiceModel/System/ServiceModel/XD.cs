namespace System.ServiceModel
{
    using System;

    internal static class XD
    {
        private static System.ServiceModel.ActivityIdFlowDictionary activityIdFlowDictionary;
        private static System.ServiceModel.Addressing10Dictionary addressing10Dictionary;
        private static System.ServiceModel.Addressing200408Dictionary addressing200408Dictionary;
        private static System.ServiceModel.AddressingDictionary addressingDictionary;
        private static System.ServiceModel.AddressingNoneDictionary addressingNoneDictionary;
        private static System.ServiceModel.AtomicTransactionExternal10Dictionary atomicTransactionExternal10Dictionary;
        private static System.ServiceModel.AtomicTransactionExternalDictionary atomicTransactionExternalDictionary;
        private static System.ServiceModel.CoordinationExternal10Dictionary coordinationExternal10Dictionary;
        private static System.ServiceModel.CoordinationExternalDictionary coordinationExternalDictionary;
        private static System.ServiceModel.DotNetAddressingDictionary dotNetAddressingDictionary;
        private static System.ServiceModel.DotNetAtomicTransactionExternalDictionary dotNetAtomicTransactionExternalDictionary;
        private static System.ServiceModel.DotNetOneWayDictionary dotNetOneWayDictionary;
        private static System.ServiceModel.DotNetSecurityDictionary dotNetSecurityDictionary;
        private static System.ServiceModel.ExclusiveC14NDictionary exclusiveC14NDictionary;
        private static System.ServiceModel.Message11Dictionary message11Dictionary;
        private static System.ServiceModel.Message12Dictionary message12Dictionary;
        private static System.ServiceModel.MessageDictionary messageDictionary;
        private static System.ServiceModel.OleTxTransactionExternalDictionary oleTxTransactionExternalDictionary;
        private static System.ServiceModel.PeerWireStringsDictionary peerWireStringsDictionary;
        private static System.ServiceModel.PolicyDictionary policyDictionary;
        private static System.ServiceModel.SamlDictionary samlDictionary;
        private static System.ServiceModel.SecureConversationApr2004Dictionary secureConversationApr2004Dictionary;
        private static System.ServiceModel.SecureConversationFeb2005Dictionary secureConversationFeb2005Dictionary;
        private static System.ServiceModel.SecurityAlgorithmDictionary securityAlgorithmDictionary;
        private static System.ServiceModel.SecurityJan2004Dictionary securityJan2004Dictionary;
        private static System.ServiceModel.SecurityXXX2005Dictionary securityXXX2005Dictionary;
        private static System.ServiceModel.SerializationDictionary serializationDictionary;
        private static System.ServiceModel.TrustApr2004Dictionary trustApr2004Dictionary;
        private static System.ServiceModel.TrustFeb2005Dictionary trustFeb2005Dictionary;
        private static System.ServiceModel.UtilityDictionary utilityDictionary;
        private static System.ServiceModel.WsrmFeb2005Dictionary wsrmFeb2005Dictionary;
        private static System.ServiceModel.XmlEncryptionDictionary xmlEncryptionDictionary;
        private static System.ServiceModel.XmlSignatureDictionary xmlSignatureDictionary;

        public static System.ServiceModel.ActivityIdFlowDictionary ActivityIdFlowDictionary
        {
            get
            {
                if (activityIdFlowDictionary == null)
                {
                    activityIdFlowDictionary = new System.ServiceModel.ActivityIdFlowDictionary(Dictionary);
                }
                return activityIdFlowDictionary;
            }
        }

        public static System.ServiceModel.Addressing10Dictionary Addressing10Dictionary
        {
            get
            {
                if (addressing10Dictionary == null)
                {
                    addressing10Dictionary = new System.ServiceModel.Addressing10Dictionary(Dictionary);
                }
                return addressing10Dictionary;
            }
        }

        public static System.ServiceModel.Addressing200408Dictionary Addressing200408Dictionary
        {
            get
            {
                if (addressing200408Dictionary == null)
                {
                    addressing200408Dictionary = new System.ServiceModel.Addressing200408Dictionary(Dictionary);
                }
                return addressing200408Dictionary;
            }
        }

        public static System.ServiceModel.AddressingDictionary AddressingDictionary
        {
            get
            {
                if (addressingDictionary == null)
                {
                    addressingDictionary = new System.ServiceModel.AddressingDictionary(Dictionary);
                }
                return addressingDictionary;
            }
        }

        public static System.ServiceModel.AddressingNoneDictionary AddressingNoneDictionary
        {
            get
            {
                if (addressingNoneDictionary == null)
                {
                    addressingNoneDictionary = new System.ServiceModel.AddressingNoneDictionary(Dictionary);
                }
                return addressingNoneDictionary;
            }
        }

        public static System.ServiceModel.AtomicTransactionExternal10Dictionary AtomicTransactionExternal10Dictionary
        {
            get
            {
                if (atomicTransactionExternal10Dictionary == null)
                {
                    atomicTransactionExternal10Dictionary = new System.ServiceModel.AtomicTransactionExternal10Dictionary(Dictionary);
                }
                return atomicTransactionExternal10Dictionary;
            }
        }

        public static System.ServiceModel.AtomicTransactionExternalDictionary AtomicTransactionExternalDictionary
        {
            get
            {
                if (atomicTransactionExternalDictionary == null)
                {
                    atomicTransactionExternalDictionary = new System.ServiceModel.AtomicTransactionExternalDictionary(Dictionary);
                }
                return atomicTransactionExternalDictionary;
            }
        }

        public static System.ServiceModel.CoordinationExternal10Dictionary CoordinationExternal10Dictionary
        {
            get
            {
                if (coordinationExternal10Dictionary == null)
                {
                    coordinationExternal10Dictionary = new System.ServiceModel.CoordinationExternal10Dictionary(Dictionary);
                }
                return coordinationExternal10Dictionary;
            }
        }

        public static System.ServiceModel.CoordinationExternalDictionary CoordinationExternalDictionary
        {
            get
            {
                if (coordinationExternalDictionary == null)
                {
                    coordinationExternalDictionary = new System.ServiceModel.CoordinationExternalDictionary(Dictionary);
                }
                return coordinationExternalDictionary;
            }
        }

        public static ServiceModelDictionary Dictionary
        {
            get
            {
                return ServiceModelDictionary.CurrentVersion;
            }
        }

        public static System.ServiceModel.DotNetAddressingDictionary DotNetAddressingDictionary
        {
            get
            {
                if (dotNetAddressingDictionary == null)
                {
                    dotNetAddressingDictionary = new System.ServiceModel.DotNetAddressingDictionary(Dictionary);
                }
                return dotNetAddressingDictionary;
            }
        }

        public static System.ServiceModel.DotNetAtomicTransactionExternalDictionary DotNetAtomicTransactionExternalDictionary
        {
            get
            {
                if (dotNetAtomicTransactionExternalDictionary == null)
                {
                    dotNetAtomicTransactionExternalDictionary = new System.ServiceModel.DotNetAtomicTransactionExternalDictionary(Dictionary);
                }
                return dotNetAtomicTransactionExternalDictionary;
            }
        }

        public static System.ServiceModel.DotNetOneWayDictionary DotNetOneWayDictionary
        {
            get
            {
                if (dotNetOneWayDictionary == null)
                {
                    dotNetOneWayDictionary = new System.ServiceModel.DotNetOneWayDictionary(Dictionary);
                }
                return dotNetOneWayDictionary;
            }
        }

        public static System.ServiceModel.DotNetSecurityDictionary DotNetSecurityDictionary
        {
            get
            {
                if (dotNetSecurityDictionary == null)
                {
                    dotNetSecurityDictionary = new System.ServiceModel.DotNetSecurityDictionary(Dictionary);
                }
                return dotNetSecurityDictionary;
            }
        }

        public static System.ServiceModel.ExclusiveC14NDictionary ExclusiveC14NDictionary
        {
            get
            {
                if (exclusiveC14NDictionary == null)
                {
                    exclusiveC14NDictionary = new System.ServiceModel.ExclusiveC14NDictionary(Dictionary);
                }
                return exclusiveC14NDictionary;
            }
        }

        public static System.ServiceModel.Message11Dictionary Message11Dictionary
        {
            get
            {
                if (message11Dictionary == null)
                {
                    message11Dictionary = new System.ServiceModel.Message11Dictionary(Dictionary);
                }
                return message11Dictionary;
            }
        }

        public static System.ServiceModel.Message12Dictionary Message12Dictionary
        {
            get
            {
                if (message12Dictionary == null)
                {
                    message12Dictionary = new System.ServiceModel.Message12Dictionary(Dictionary);
                }
                return message12Dictionary;
            }
        }

        public static System.ServiceModel.MessageDictionary MessageDictionary
        {
            get
            {
                if (messageDictionary == null)
                {
                    messageDictionary = new System.ServiceModel.MessageDictionary(Dictionary);
                }
                return messageDictionary;
            }
        }

        public static System.ServiceModel.OleTxTransactionExternalDictionary OleTxTransactionExternalDictionary
        {
            get
            {
                if (oleTxTransactionExternalDictionary == null)
                {
                    oleTxTransactionExternalDictionary = new System.ServiceModel.OleTxTransactionExternalDictionary(Dictionary);
                }
                return oleTxTransactionExternalDictionary;
            }
        }

        public static System.ServiceModel.PeerWireStringsDictionary PeerWireStringsDictionary
        {
            get
            {
                if (peerWireStringsDictionary == null)
                {
                    peerWireStringsDictionary = new System.ServiceModel.PeerWireStringsDictionary(Dictionary);
                }
                return peerWireStringsDictionary;
            }
        }

        public static System.ServiceModel.PolicyDictionary PolicyDictionary
        {
            get
            {
                if (policyDictionary == null)
                {
                    policyDictionary = new System.ServiceModel.PolicyDictionary(Dictionary);
                }
                return policyDictionary;
            }
        }

        public static System.ServiceModel.SamlDictionary SamlDictionary
        {
            get
            {
                if (samlDictionary == null)
                {
                    samlDictionary = new System.ServiceModel.SamlDictionary(Dictionary);
                }
                return samlDictionary;
            }
        }

        public static System.ServiceModel.SecureConversationApr2004Dictionary SecureConversationApr2004Dictionary
        {
            get
            {
                if (secureConversationApr2004Dictionary == null)
                {
                    secureConversationApr2004Dictionary = new System.ServiceModel.SecureConversationApr2004Dictionary(Dictionary);
                }
                return secureConversationApr2004Dictionary;
            }
        }

        public static System.ServiceModel.SecureConversationFeb2005Dictionary SecureConversationFeb2005Dictionary
        {
            get
            {
                if (secureConversationFeb2005Dictionary == null)
                {
                    secureConversationFeb2005Dictionary = new System.ServiceModel.SecureConversationFeb2005Dictionary(Dictionary);
                }
                return secureConversationFeb2005Dictionary;
            }
        }

        public static System.ServiceModel.SecurityAlgorithmDictionary SecurityAlgorithmDictionary
        {
            get
            {
                if (securityAlgorithmDictionary == null)
                {
                    securityAlgorithmDictionary = new System.ServiceModel.SecurityAlgorithmDictionary(Dictionary);
                }
                return securityAlgorithmDictionary;
            }
        }

        public static System.ServiceModel.SecurityJan2004Dictionary SecurityJan2004Dictionary
        {
            get
            {
                if (securityJan2004Dictionary == null)
                {
                    securityJan2004Dictionary = new System.ServiceModel.SecurityJan2004Dictionary(Dictionary);
                }
                return securityJan2004Dictionary;
            }
        }

        public static System.ServiceModel.SecurityXXX2005Dictionary SecurityXXX2005Dictionary
        {
            get
            {
                if (securityXXX2005Dictionary == null)
                {
                    securityXXX2005Dictionary = new System.ServiceModel.SecurityXXX2005Dictionary(Dictionary);
                }
                return securityXXX2005Dictionary;
            }
        }

        public static System.ServiceModel.SerializationDictionary SerializationDictionary
        {
            get
            {
                if (serializationDictionary == null)
                {
                    serializationDictionary = new System.ServiceModel.SerializationDictionary(Dictionary);
                }
                return serializationDictionary;
            }
        }

        public static System.ServiceModel.TrustApr2004Dictionary TrustApr2004Dictionary
        {
            get
            {
                if (trustApr2004Dictionary == null)
                {
                    trustApr2004Dictionary = new System.ServiceModel.TrustApr2004Dictionary(Dictionary);
                }
                return trustApr2004Dictionary;
            }
        }

        public static System.ServiceModel.TrustFeb2005Dictionary TrustFeb2005Dictionary
        {
            get
            {
                if (trustFeb2005Dictionary == null)
                {
                    trustFeb2005Dictionary = new System.ServiceModel.TrustFeb2005Dictionary(Dictionary);
                }
                return trustFeb2005Dictionary;
            }
        }

        public static System.ServiceModel.UtilityDictionary UtilityDictionary
        {
            get
            {
                if (utilityDictionary == null)
                {
                    utilityDictionary = new System.ServiceModel.UtilityDictionary(Dictionary);
                }
                return utilityDictionary;
            }
        }

        public static System.ServiceModel.WsrmFeb2005Dictionary WsrmFeb2005Dictionary
        {
            get
            {
                if (wsrmFeb2005Dictionary == null)
                {
                    wsrmFeb2005Dictionary = new System.ServiceModel.WsrmFeb2005Dictionary(Dictionary);
                }
                return wsrmFeb2005Dictionary;
            }
        }

        public static System.ServiceModel.XmlEncryptionDictionary XmlEncryptionDictionary
        {
            get
            {
                if (xmlEncryptionDictionary == null)
                {
                    xmlEncryptionDictionary = new System.ServiceModel.XmlEncryptionDictionary(Dictionary);
                }
                return xmlEncryptionDictionary;
            }
        }

        public static System.ServiceModel.XmlSignatureDictionary XmlSignatureDictionary
        {
            get
            {
                if (xmlSignatureDictionary == null)
                {
                    xmlSignatureDictionary = new System.ServiceModel.XmlSignatureDictionary(Dictionary);
                }
                return xmlSignatureDictionary;
            }
        }
    }
}

