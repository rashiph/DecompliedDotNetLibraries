namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class Basic192Rsa15SecurityAlgorithmSuite : Basic192SecurityAlgorithmSuite
    {
        public override string ToString()
        {
            return "Basic192Rsa15";
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

