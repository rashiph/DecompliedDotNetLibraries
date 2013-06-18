namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Text;

    internal static class MsmqFormatName
    {
        private const string systemMessagingFormatNamePrefix = "FORMATNAME:";
        private const string systemMessagingLabelPrefix = "LABEL:";

        public static string FromQueuePath(string queuePath)
        {
            int capacity = 0x100;
            StringBuilder formatName = new StringBuilder(capacity);
            int error = UnsafeNativeMethods.MQPathNameToFormatName(queuePath, formatName, ref capacity);
            if (-1072824289 == error)
            {
                formatName = new StringBuilder(capacity);
                error = UnsafeNativeMethods.MQPathNameToFormatName(queuePath, formatName, ref capacity);
            }
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqPathLookupError", new object[] { queuePath, MsmqError.GetErrorString(error) }), error));
            }
            return formatName.ToString();
        }

        public static string ToSystemMessagingQueueName(string formatName)
        {
            return ("FORMATNAME:" + formatName);
        }
    }
}

