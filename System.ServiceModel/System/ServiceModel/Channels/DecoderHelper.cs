namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class DecoderHelper
    {
        public static void ValidateSize(int size)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, System.ServiceModel.SR.GetString("ValueMustBePositive")));
            }
        }
    }
}

