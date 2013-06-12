namespace System.Diagnostics
{
    using System;
    using System.Collections.Specialized;
    using System.Text;

    internal static class EnvironmentBlock
    {
        public static byte[] ToByteArray(StringDictionary sd, bool unicode)
        {
            string[] array = new string[sd.Count];
            byte[] bytes = null;
            sd.Keys.CopyTo(array, 0);
            string[] strArray2 = new string[sd.Count];
            sd.Values.CopyTo(strArray2, 0);
            Array.Sort(array, strArray2, OrdinalCaseInsensitiveComparer.Default);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < sd.Count; i++)
            {
                builder.Append(array[i]);
                builder.Append('=');
                builder.Append(strArray2[i]);
                builder.Append('\0');
            }
            builder.Append('\0');
            if (unicode)
            {
                bytes = Encoding.Unicode.GetBytes(builder.ToString());
            }
            else
            {
                bytes = Encoding.Default.GetBytes(builder.ToString());
            }
            if (bytes.Length > 0xffff)
            {
                throw new InvalidOperationException(SR.GetString("EnvironmentBlockTooLong", new object[] { bytes.Length }));
            }
            return bytes;
        }
    }
}

