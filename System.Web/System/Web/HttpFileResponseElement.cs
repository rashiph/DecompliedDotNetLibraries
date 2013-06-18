namespace System.Web
{
    using System;
    using System.IO;

    internal sealed class HttpFileResponseElement : IHttpResponseElement
    {
        private string _filename;
        private bool _isImpersonating;
        private long _offset;
        private long _size;
        private bool _useTransmitFile;

        internal HttpFileResponseElement(string filename, long offset, long size) : this(filename, offset, size, false, false, false)
        {
        }

        internal HttpFileResponseElement(string filename, long offset, long size, bool isImpersonating, bool supportsLongTransmitFile) : this(filename, offset, size, isImpersonating, true, supportsLongTransmitFile)
        {
        }

        private HttpFileResponseElement(string filename, long offset, long size, bool isImpersonating, bool useTransmitFile, bool supportsLongTransmitFile)
        {
            if ((!supportsLongTransmitFile && (size > 0x7fffffffL)) || (size < 0L))
            {
                throw new ArgumentOutOfRangeException("size", size, System.Web.SR.GetString("Invalid_size"));
            }
            if ((!supportsLongTransmitFile && (offset > 0x7fffffffL)) || (offset < 0L))
            {
                throw new ArgumentOutOfRangeException("offset", offset, System.Web.SR.GetString("Invalid_size"));
            }
            this._filename = filename;
            this._offset = offset;
            this._size = size;
            this._isImpersonating = isImpersonating;
            this._useTransmitFile = useTransmitFile;
        }

        byte[] IHttpResponseElement.GetBytes()
        {
            if (this._size == 0L)
            {
                return null;
            }
            byte[] buffer = null;
            FileStream stream = null;
            try
            {
                stream = new FileStream(this._filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                long length = stream.Length;
                if ((this._offset < 0L) || (this._size > (length - this._offset)))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_range"));
                }
                if (this._offset > 0L)
                {
                    stream.Seek(this._offset, SeekOrigin.Begin);
                }
                int count = (int) this._size;
                buffer = new byte[count];
                int offset = 0;
                do
                {
                    int num4 = stream.Read(buffer, offset, count);
                    if (num4 == 0)
                    {
                        return buffer;
                    }
                    offset += num4;
                    count -= num4;
                }
                while (count > 0);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return buffer;
        }

        long IHttpResponseElement.GetSize()
        {
            return this._size;
        }

        void IHttpResponseElement.Send(HttpWorkerRequest wr)
        {
            if (this._size > 0L)
            {
                if (this._useTransmitFile)
                {
                    wr.TransmitFile(this._filename, this._offset, this._size, this._isImpersonating);
                }
                else
                {
                    wr.SendResponseFromFile(this._filename, this._offset, this._size);
                }
            }
        }

        internal string FileName
        {
            get
            {
                return this._filename;
            }
        }

        internal long Offset
        {
            get
            {
                return this._offset;
            }
        }
    }
}

