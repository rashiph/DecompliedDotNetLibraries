namespace System.Net
{
    using System;
    using System.IO;
    using System.IO.Compression;

    internal class GZipWrapperStream : GZipStream, ICloseEx, IRequestLifetimeTracker
    {
        public GZipWrapperStream(Stream stream, CompressionMode mode) : base(stream, mode, false)
        {
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            try
            {
                result = base.BeginRead(buffer, offset, size, callback, state);
            }
            catch (Exception exception)
            {
                try
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if (((exception is InvalidDataException) || (exception is InvalidOperationException)) || (exception is IndexOutOfRangeException))
                    {
                        this.Close();
                    }
                }
                catch
                {
                }
                throw exception;
            }
            return result;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int num;
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            try
            {
                num = base.EndRead(asyncResult);
            }
            catch (Exception exception)
            {
                try
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if (((exception is InvalidDataException) || (exception is InvalidOperationException)) || (exception is IndexOutOfRangeException))
                    {
                        this.Close();
                    }
                }
                catch
                {
                }
                throw exception;
            }
            return num;
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            int num;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            try
            {
                num = base.Read(buffer, offset, size);
            }
            catch (Exception exception)
            {
                try
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if (((exception is InvalidDataException) || (exception is InvalidOperationException)) || (exception is IndexOutOfRangeException))
                    {
                        this.Close();
                    }
                }
                catch
                {
                }
                throw exception;
            }
            return num;
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            ICloseEx baseStream = base.BaseStream as ICloseEx;
            if (baseStream != null)
            {
                baseStream.CloseEx(closeState);
            }
            else
            {
                this.Close();
            }
        }

        void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
        {
            (base.BaseStream as IRequestLifetimeTracker).TrackRequestLifetime(requestStartTimestamp);
        }
    }
}

