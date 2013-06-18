namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class Basic128Rsa15SecurityAlgorithmSuite : Basic128SecurityAlgorithmSuite
    {
        public override string ToString()
        {
            return "Basic128Rsa15";
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

