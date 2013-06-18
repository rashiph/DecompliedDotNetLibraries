namespace System.IdentityModel
{
    using System;

    internal class ExtendedTransformFactory : StandardTransformFactory
    {
        private static ExtendedTransformFactory instance = new ExtendedTransformFactory();

        private ExtendedTransformFactory()
        {
        }

        public override Transform CreateTransform(string transformAlgorithmUri)
        {
            if (transformAlgorithmUri == XD.XmlSignatureDictionary.EnvelopedSignature.Value)
            {
                return new EnvelopedSignatureTransform();
            }
            return base.CreateTransform(transformAlgorithmUri);
        }

        internal static ExtendedTransformFactory Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

