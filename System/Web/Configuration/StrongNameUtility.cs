namespace System.Web.Configuration
{
    using Microsoft.Runtime.Hosting;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Web;

    internal class StrongNameUtility
    {
        private StrongNameUtility()
        {
        }

        internal static bool GenerateStrongNameFile(string filename)
        {
            IntPtr zero = IntPtr.Zero;
            int pcbKeyBlob = 0;
            if (!Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameKeyGen(null, 0, out zero, out pcbKeyBlob) || (zero == IntPtr.Zero))
            {
                throw Marshal.GetExceptionForHR(Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameErrorInfo());
            }
            try
            {
                if ((pcbKeyBlob <= 0) || (pcbKeyBlob > 0x7fffffff))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Browser_InvalidStrongNameKey"));
                }
                byte[] destination = new byte[pcbKeyBlob];
                Marshal.Copy(zero, destination, 0, pcbKeyBlob);
                using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(destination);
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameFreeBuffer(zero);
                }
            }
            return true;
        }
    }
}

