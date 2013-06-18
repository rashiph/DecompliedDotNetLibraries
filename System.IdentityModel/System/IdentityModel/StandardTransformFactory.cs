namespace System.IdentityModel
{
    using System;
    using System.Security.Cryptography;

    internal class StandardTransformFactory : TransformFactory
    {
        private static StandardTransformFactory instance = new StandardTransformFactory();

        protected StandardTransformFactory()
        {
        }

        public override Transform CreateTransform(string transformAlgorithmUri)
        {
            if (transformAlgorithmUri != "http://www.w3.org/2001/10/xml-exc-c14n#")
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedTransformAlgorithm")));
            }
            return new ExclusiveCanonicalizationTransform();
        }

        internal static StandardTransformFactory Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

