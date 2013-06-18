namespace System.ServiceModel
{
    using System;

    internal static class OperationFormatStyleHelper
    {
        public static bool IsDefined(OperationFormatStyle x)
        {
            if (x != OperationFormatStyle.Document)
            {
                return (x == OperationFormatStyle.Rpc);
            }
            return true;
        }
    }
}

