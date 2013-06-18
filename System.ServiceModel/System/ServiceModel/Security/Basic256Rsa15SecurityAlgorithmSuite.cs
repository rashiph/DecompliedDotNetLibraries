namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class Basic256Rsa15SecurityAlgorithmSuite : Basic256SecurityAlgorithmSuite
    {
        public override string ToString()
        {
            return "Basic256Rsa15";
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

