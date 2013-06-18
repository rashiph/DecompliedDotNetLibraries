namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Runtime.InteropServices;

    internal class BlobPackage
    {
        private byte[] _bits;
        internal _BLOB Blob;

        internal BlobPackage(_BLOB b)
        {
            this.Blob = b;
            this._bits = null;
        }

        internal byte[] GetBits()
        {
            if (this._bits != null)
            {
                return this._bits;
            }
            byte[] destination = new byte[this.Blob.cbSize];
            Marshal.Copy(this.Blob.pBlobData, destination, 0, this.Blob.cbSize);
            return destination;
        }
    }
}

