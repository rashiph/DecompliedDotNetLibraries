namespace System.Web
{
    using System;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class HttpSubstBlockResponseElement : IHttpResponseElement
    {
        private HttpResponseSubstitutionCallback _callback;
        private IntPtr _firstSubstData;
        private int _firstSubstDataSize;
        private IHttpResponseElement _firstSubstitution;
        private bool _isIIS7WorkerRequest;

        internal HttpSubstBlockResponseElement(HttpResponseSubstitutionCallback callback)
        {
            this._callback = callback;
        }

        internal HttpSubstBlockResponseElement(HttpResponseSubstitutionCallback callback, Encoding encoding, System.Text.Encoder encoder, IIS7WorkerRequest iis7WorkerRequest)
        {
            this._callback = callback;
            if (iis7WorkerRequest != null)
            {
                this._isIIS7WorkerRequest = true;
                string s = this._callback(HttpContext.Current);
                if (s == null)
                {
                    throw new ArgumentNullException("substitutionString");
                }
                this.CreateFirstSubstData(s, iis7WorkerRequest, encoder);
            }
            else
            {
                this._firstSubstitution = this.Substitute(encoding);
            }
        }

        private unsafe void CreateFirstSubstData(string s, IIS7WorkerRequest iis7WorkerRequest, System.Text.Encoder encoder)
        {
            IntPtr ptr;
            int num = 0;
            int length = s.Length;
            if (length > 0)
            {
                fixed (char* str = ((char*) s))
                {
                    char* chars = str;
                    int size = encoder.GetByteCount(chars, length, true);
                    ptr = iis7WorkerRequest.AllocateRequestMemory(size);
                    if (ptr != IntPtr.Zero)
                    {
                        num = encoder.GetBytes(chars, length, (byte*) ptr, size, true);
                    }
                }
            }
            else
            {
                ptr = iis7WorkerRequest.AllocateRequestMemory(1);
            }
            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            this._firstSubstData = ptr;
            this._firstSubstDataSize = num;
        }

        internal bool PointerEquals(IntPtr ptr)
        {
            return (this._firstSubstData == ptr);
        }

        internal IHttpResponseElement Substitute(Encoding e)
        {
            string s = this._callback(HttpContext.Current);
            byte[] bytes = e.GetBytes(s);
            return new HttpResponseBufferElement(bytes, bytes.Length);
        }

        byte[] IHttpResponseElement.GetBytes()
        {
            if (!this._isIIS7WorkerRequest)
            {
                return this._firstSubstitution.GetBytes();
            }
            if (this._firstSubstDataSize > 0)
            {
                byte[] dest = new byte[this._firstSubstDataSize];
                Misc.CopyMemory(this._firstSubstData, 0, dest, 0, this._firstSubstDataSize);
                return dest;
            }
            if (!(this._firstSubstData == IntPtr.Zero))
            {
                return new byte[0];
            }
            return null;
        }

        long IHttpResponseElement.GetSize()
        {
            if (this._isIIS7WorkerRequest)
            {
                return (long) this._firstSubstDataSize;
            }
            return this._firstSubstitution.GetSize();
        }

        void IHttpResponseElement.Send(HttpWorkerRequest wr)
        {
            if (this._isIIS7WorkerRequest)
            {
                IIS7WorkerRequest request = wr as IIS7WorkerRequest;
                if (request != null)
                {
                    request.SendResponseFromIISAllocatedRequestMemory(this._firstSubstData, this._firstSubstDataSize);
                }
            }
            else
            {
                this._firstSubstitution.Send(wr);
            }
        }

        internal HttpResponseSubstitutionCallback Callback
        {
            get
            {
                return this._callback;
            }
        }
    }
}

