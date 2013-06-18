namespace Microsoft.Build.Shared
{
    using System;
    using System.Text;

    internal static class EncodingUtilities
    {
        private static Encoding currentOemEncoding;

        internal static Encoding CurrentSystemOemEncoding
        {
            get
            {
                if (currentOemEncoding == null)
                {
                    currentOemEncoding = Encoding.Default;
                    try
                    {
                        currentOemEncoding = Encoding.GetEncoding(Microsoft.Build.Shared.NativeMethodsShared.GetOEMCP());
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
                return currentOemEncoding;
            }
        }
    }
}

