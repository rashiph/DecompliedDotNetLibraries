namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;

    internal static class TransferModeHelper
    {
        public static bool IsDefined(TransferMode v)
        {
            if (((v != TransferMode.Buffered) && (v != TransferMode.Streamed)) && (v != TransferMode.StreamedRequest))
            {
                return (v == TransferMode.StreamedResponse);
            }
            return true;
        }

        public static bool IsRequestStreamed(TransferMode v)
        {
            if (v != TransferMode.StreamedRequest)
            {
                return (v == TransferMode.Streamed);
            }
            return true;
        }

        public static bool IsResponseStreamed(TransferMode v)
        {
            if (v != TransferMode.StreamedResponse)
            {
                return (v == TransferMode.Streamed);
            }
            return true;
        }

        public static void Validate(TransferMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(TransferMode)));
            }
        }
    }
}

