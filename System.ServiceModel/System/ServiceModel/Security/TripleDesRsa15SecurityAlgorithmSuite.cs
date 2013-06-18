namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class TripleDesRsa15SecurityAlgorithmSuite : TripleDesSecurityAlgorithmSuite
    {
        public override string ToString()
        {
            return "TripleDesRsa15";
        }

        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.RsaV15KeyWrap;
            }
        }
    }
}

