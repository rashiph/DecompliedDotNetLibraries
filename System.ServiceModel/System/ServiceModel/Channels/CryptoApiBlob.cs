namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal sealed class CryptoApiBlob : IDisposable
    {
        private int cbData;
        private CriticalAllocHandle data;

        public CryptoApiBlob()
        {
        }

        public CryptoApiBlob(byte[] bytes)
        {
            this.AllocateBlob(bytes.Length);
            Marshal.Copy(bytes, 0, (IntPtr) this.data, bytes.Length);
            this.cbData = bytes.Length;
        }

        public void AllocateBlob(int size)
        {
            this.data = CriticalAllocHandle.FromSize(size);
            this.cbData = size;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public byte[] GetBytes()
        {
            if (this.cbData == 0)
            {
                return null;
            }
            byte[] destination = DiagnosticUtility.Utility.AllocateByteArray(this.cbData);
            Marshal.Copy((IntPtr) this.data, destination, 0, this.cbData);
            return destination;
        }

        public InteropHelper GetMemoryForPinning()
        {
            return new InteropHelper(this.cbData, (IntPtr) this.data);
        }

        public int DataSize
        {
            get
            {
                return this.cbData;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class InteropHelper
        {
            public int size;
            public IntPtr data;
            public InteropHelper(int size, IntPtr data)
            {
                this.size = size;
                this.data = data;
            }
        }
    }
}

