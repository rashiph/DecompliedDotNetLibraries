namespace System.Runtime
{
    using System;
    using System.Xml.Linq;

    internal static class PersistenceMetadataNamespace
    {
        private static XName activationType;
        private const string baseNamespace = "urn:schemas-microsoft-com:System.Runtime.DurableInstancing/4.0/metadata";
        private static XName instanceType;
        private static readonly XNamespace persistenceMetadataNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Runtime.DurableInstancing/4.0/metadata");

        public static XName ActivationType
        {
            get
            {
                if (activationType == null)
                {
                    activationType = persistenceMetadataNamespace.GetName("ActivationType");
                }
                return activationType;
            }
        }

        public static XName InstanceType
        {
            get
            {
                if (instanceType == null)
                {
                    instanceType = persistenceMetadataNamespace.GetName("InstanceType");
                }
                return instanceType;
            }
        }

        public static class ActivationTypes
        {
            private static readonly XNamespace activationNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activation");
            private const string baseNamespace = "urn:schemas-microsoft-com:System.ServiceModel.Activation";
            private static XName was;

            public static XName WAS
            {
                get
                {
                    if (was == null)
                    {
                        was = activationNamespace.GetName("WindowsProcessActivationService");
                    }
                    return was;
                }
            }
        }
    }
}

