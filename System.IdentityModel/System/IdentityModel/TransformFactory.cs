namespace System.IdentityModel
{
    using System;

    internal abstract class TransformFactory
    {
        protected TransformFactory()
        {
        }

        public abstract Transform CreateTransform(string transformAlgorithmUri);
    }
}

