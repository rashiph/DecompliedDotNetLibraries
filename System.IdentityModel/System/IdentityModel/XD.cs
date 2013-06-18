namespace System.IdentityModel
{
    using System;

    internal static class XD
    {
        private static System.IdentityModel.ExclusiveC14NDictionary exclusiveC14NDictionary;
        private static System.IdentityModel.SamlDictionary samlDictionary;
        private static System.IdentityModel.SecurityAlgorithmDictionary securityAlgorithmDictionary;
        private static System.IdentityModel.UtilityDictionary utilityDictionary;
        private static System.IdentityModel.XmlSignatureDictionary xmlSignatureDictionary;

        public static IdentityModelDictionary Dictionary
        {
            get
            {
                return IdentityModelDictionary.CurrentVersion;
            }
        }

        public static System.IdentityModel.ExclusiveC14NDictionary ExclusiveC14NDictionary
        {
            get
            {
                if (exclusiveC14NDictionary == null)
                {
                    exclusiveC14NDictionary = new System.IdentityModel.ExclusiveC14NDictionary(Dictionary);
                }
                return exclusiveC14NDictionary;
            }
        }

        public static System.IdentityModel.SamlDictionary SamlDictionary
        {
            get
            {
                if (samlDictionary == null)
                {
                    samlDictionary = new System.IdentityModel.SamlDictionary(Dictionary);
                }
                return samlDictionary;
            }
        }

        public static System.IdentityModel.SecurityAlgorithmDictionary SecurityAlgorithmDictionary
        {
            get
            {
                if (securityAlgorithmDictionary == null)
                {
                    securityAlgorithmDictionary = new System.IdentityModel.SecurityAlgorithmDictionary(Dictionary);
                }
                return securityAlgorithmDictionary;
            }
        }

        public static System.IdentityModel.UtilityDictionary UtilityDictionary
        {
            get
            {
                if (utilityDictionary == null)
                {
                    utilityDictionary = new System.IdentityModel.UtilityDictionary(Dictionary);
                }
                return utilityDictionary;
            }
        }

        public static System.IdentityModel.XmlSignatureDictionary XmlSignatureDictionary
        {
            get
            {
                if (xmlSignatureDictionary == null)
                {
                    xmlSignatureDictionary = new System.IdentityModel.XmlSignatureDictionary(Dictionary);
                }
                return xmlSignatureDictionary;
            }
        }
    }
}

