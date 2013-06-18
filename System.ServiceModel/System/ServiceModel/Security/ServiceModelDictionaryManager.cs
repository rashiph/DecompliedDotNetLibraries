namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.ServiceModel.Channels;

    internal class ServiceModelDictionaryManager
    {
        private static DictionaryManager dictionaryManager;

        public static DictionaryManager Instance
        {
            get
            {
                if (dictionaryManager == null)
                {
                    dictionaryManager = new DictionaryManager(BinaryMessageEncoderFactory.XmlDictionary);
                }
                return dictionaryManager;
            }
        }
    }
}

