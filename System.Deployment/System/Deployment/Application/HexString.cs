namespace System.Deployment.Application
{
    using System;
    using System.Globalization;
    using System.Text;

    internal static class HexString
    {
        public static string FromBytes(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", new object[] { bytes[i] });
            }
            return builder.ToString();
        }
    }
}

