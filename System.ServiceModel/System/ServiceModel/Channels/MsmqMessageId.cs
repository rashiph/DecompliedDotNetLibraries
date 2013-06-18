namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Text;

    internal static class MsmqMessageId
    {
        private const int guidSize = 0x10;

        public static byte[] FromString(string messageId)
        {
            Guid guid;
            int num;
            string[] strArray = messageId.Split(new char[] { '\\' });
            if (strArray.Length != 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidMessageId", new object[] { messageId }), "messageId"));
            }
            if (!DiagnosticUtility.Utility.TryCreateGuid(strArray[0], out guid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidMessageId", new object[] { messageId }), "messageId"));
            }
            try
            {
                num = Convert.ToInt32(strArray[1], CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidMessageId", new object[] { messageId }), "messageId"));
            }
            byte[] destinationArray = new byte[20];
            Array.Copy(guid.ToByteArray(), destinationArray, 0x10);
            Array.Copy(BitConverter.GetBytes(num), 0, destinationArray, 0x10, 4);
            return destinationArray;
        }

        public static string ToString(byte[] messageId)
        {
            StringBuilder builder = new StringBuilder();
            byte[] destinationArray = new byte[0x10];
            Array.Copy(messageId, destinationArray, 0x10);
            int num = BitConverter.ToInt32(messageId, 0x10);
            builder.Append(new Guid(destinationArray).ToString());
            builder.Append(@"\");
            builder.Append(num);
            return builder.ToString();
        }
    }
}

