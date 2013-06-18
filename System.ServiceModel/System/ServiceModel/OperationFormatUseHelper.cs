namespace System.ServiceModel
{
    using System;

    internal static class OperationFormatUseHelper
    {
        public static bool IsDefined(OperationFormatUse x)
        {
            if (x != OperationFormatUse.Literal)
            {
                return (x == OperationFormatUse.Encoded);
            }
            return true;
        }
    }
}

