namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class DictionaryManager
    {
        private System.IdentityModel.ExclusiveC14NDictionary exclusiveC14NDictionary;
        private IXmlDictionary parentDictionary;
        private System.IdentityModel.SamlDictionary samlDictionary;
        private System.IdentityModel.SecurityAlgorithmDictionary securityAlgorithmDictionary;
        private System.IdentityModel.XmlSignatureDictionary sigantureDictionary;
        private System.IdentityModel.UtilityDictionary utilityDictionary;

        public DictionaryManager()
        {
            this.samlDictionary = XD.SamlDictionary;
            this.sigantureDictionary = XD.XmlSignatureDictionary;
            this.utilityDictionary = XD.UtilityDictionary;
            this.exclusiveC14NDictionary = XD.ExclusiveC14NDictionary;
            this.securityAlgorithmDictionary = XD.SecurityAlgorithmDictionary;
            this.parentDictionary = XD.Dictionary;
        }

        public DictionaryManager(IXmlDictionary parentDictionary)
        {
            this.samlDictionary = new System.IdentityModel.SamlDictionary(parentDictionary);
            this.sigantureDictionary = new System.IdentityModel.XmlSignatureDictionary(parentDictionary);
            this.utilityDictionary = new System.IdentityModel.UtilityDictionary(parentDictionary);
            this.exclusiveC14NDictionary = new System.IdentityModel.ExclusiveC14NDictionary(parentDictionary);
            this.securityAlgorithmDictionary = new System.IdentityModel.SecurityAlgorithmDictionary(parentDictionary);
            this.parentDictionary = parentDictionary;
        }

        public System.IdentityModel.ExclusiveC14NDictionary ExclusiveC14NDictionary
        {
            get
            {
                return this.exclusiveC14NDictionary;
            }
        }

        public IXmlDictionary ParentDictionary
        {
            get
            {
                return this.parentDictionary;
            }
        }

        public System.IdentityModel.SamlDictionary SamlDictionary
        {
            get
            {
                return this.samlDictionary;
            }
        }

        public System.IdentityModel.SecurityAlgorithmDictionary SecurityAlgorithmDictionary
        {
            get
            {
                return this.securityAlgorithmDictionary;
            }
        }

        public System.IdentityModel.UtilityDictionary UtilityDictionary
        {
            get
            {
                return this.utilityDictionary;
            }
        }

        public System.IdentityModel.XmlSignatureDictionary XmlSignatureDictionary
        {
            get
            {
                return this.sigantureDictionary;
            }
        }
    }
}

