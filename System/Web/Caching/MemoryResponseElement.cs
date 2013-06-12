namespace System.Web.Caching
{
    using System;
    using System.Security.Permissions;
    using System.Web;

    [Serializable, AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Unrestricted)]
    public class MemoryResponseElement : ResponseElement
    {
        private byte[] _buffer;
        private long _length;

        private MemoryResponseElement()
        {
        }

        public MemoryResponseElement(byte[] buffer, long length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((length < 0L) || (length > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("length");
            }
            this._buffer = buffer;
            this._length = length;
        }

        public byte[] Buffer
        {
            get
            {
                return this._buffer;
            }
        }

        public long Length
        {
            get
            {
                return this._length;
            }
        }
    }
}

