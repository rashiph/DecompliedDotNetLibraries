namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Util;

    public sealed class HttpWriter : TextWriter
    {
        private ArrayList _buffers;
        private char[] _charBuffer;
        private int _charBufferFree;
        private int _charBufferLength;
        private HttpResponseStreamFilterSink _filterSink;
        private bool _hasBeenClearedRecently;
        private bool _ignoringFurtherWrites;
        private Stream _installedFilter;
        private HttpBaseMemoryResponseBufferElement _lastBuffer;
        private HttpResponse _response;
        private bool _responseBufferingOn;
        private int _responseCodePage;
        private bool _responseCodePageIsAsciiCompat;
        private System.Text.Encoder _responseEncoder;
        private System.Text.Encoding _responseEncoding;
        private bool _responseEncodingUpdated;
        private bool _responseEncodingUsed;
        private HttpResponseStream _stream;
        private ArrayList _substElements;
        private static CharBufferAllocator s_Allocator = new CharBufferAllocator(0x400, 0x40);

        internal HttpWriter(HttpResponse response) : base(null)
        {
            this._response = response;
            this._stream = new HttpResponseStream(this);
            this._buffers = new ArrayList();
            this._lastBuffer = null;
            this._charBuffer = (char[]) s_Allocator.GetBuffer();
            this._charBufferLength = this._charBuffer.Length;
            this._charBufferFree = this._charBufferLength;
            this.UpdateResponseBuffering();
        }

        private void BufferData(byte[] data, int offset, int size, bool needToCopyData)
        {
            int num;
            if (this._lastBuffer != null)
            {
                num = this._lastBuffer.Append(data, offset, size);
                size -= num;
                offset += num;
            }
            else if ((!needToCopyData && (offset == 0)) && !this._responseBufferingOn)
            {
                this._buffers.Add(new HttpResponseBufferElement(data, size));
                return;
            }
            while (size > 0)
            {
                this._lastBuffer = this.CreateNewMemoryBufferElement();
                this._buffers.Add(this._lastBuffer);
                num = this._lastBuffer.Append(data, offset, size);
                offset += num;
                size -= num;
            }
        }

        private void BufferResource(IntPtr data, int offset, int size)
        {
            if ((size > 0x1000) || !this._responseBufferingOn)
            {
                this._lastBuffer = null;
                this._buffers.Add(new HttpResourceResponseElement(data, offset, size));
            }
            else
            {
                int num;
                if (this._lastBuffer != null)
                {
                    num = this._lastBuffer.Append(data, offset, size);
                    size -= num;
                    offset += num;
                }
                while (size > 0)
                {
                    this._lastBuffer = this.CreateNewMemoryBufferElement();
                    this._buffers.Add(this._lastBuffer);
                    num = this._lastBuffer.Append(data, offset, size);
                    offset += num;
                    size -= num;
                }
            }
        }

        internal void ClearBuffers()
        {
            this.ClearCharBuffer();
            if (this._substElements != null)
            {
                this._response.Context.Request.SetDynamicCompression(true);
            }
            this.RecycleBufferElements();
            this._buffers = new ArrayList();
            this._lastBuffer = null;
            this._hasBeenClearedRecently = true;
        }

        private void ClearCharBuffer()
        {
            this._charBufferFree = this._charBufferLength;
        }

        internal void ClearSubstitutionBlocks()
        {
            this._substElements = null;
        }

        public override void Close()
        {
        }

        private HttpBaseMemoryResponseBufferElement CreateNewMemoryBufferElement()
        {
            return new HttpResponseUnmanagedBufferElement();
        }

        internal void DisposeIntegratedBuffers()
        {
            if (this._buffers != null)
            {
                int count = this._buffers.Count;
                for (int i = 0; i < count; i++)
                {
                    HttpBaseMemoryResponseBufferElement element = this._buffers[i] as HttpBaseMemoryResponseBufferElement;
                    if (element != null)
                    {
                        element.Recycle();
                    }
                }
                this._buffers = null;
            }
            this.ClearBuffers();
        }

        internal void Filter(bool finalFiltering)
        {
            if (this._installedFilter != null)
            {
                if (this._charBufferLength != this._charBufferFree)
                {
                    this.FlushCharBuffer(true);
                }
                this._lastBuffer = null;
                if ((this._buffers.Count != 0) || finalFiltering)
                {
                    ArrayList list = this._buffers;
                    this._buffers = new ArrayList();
                    this._filterSink.Filtering = true;
                    try
                    {
                        int count = list.Count;
                        for (int i = 0; i < count; i++)
                        {
                            IHttpResponseElement element = (IHttpResponseElement) list[i];
                            long size = element.GetSize();
                            if (size > 0L)
                            {
                                this._installedFilter.Write(element.GetBytes(), 0, Convert.ToInt32(size));
                            }
                        }
                        this._installedFilter.Flush();
                    }
                    finally
                    {
                        try
                        {
                            if (finalFiltering)
                            {
                                this._installedFilter.Close();
                            }
                        }
                        finally
                        {
                            this._filterSink.Filtering = false;
                        }
                    }
                }
            }
        }

        internal void FilterIntegrated(bool finalFiltering, IIS7WorkerRequest wr)
        {
            if (this._installedFilter != null)
            {
                if (this._charBufferLength != this._charBufferFree)
                {
                    this.FlushCharBuffer(true);
                }
                this._lastBuffer = null;
                ArrayList list = this._buffers;
                this._buffers = new ArrayList();
                ArrayList list2 = null;
                bool hasSubstBlocks = false;
                list2 = wr.GetBufferedResponseChunks(false, null, ref hasSubstBlocks);
                this._filterSink.Filtering = true;
                try
                {
                    if (list2 != null)
                    {
                        for (int i = 0; i < list2.Count; i++)
                        {
                            IHttpResponseElement element = (IHttpResponseElement) list2[i];
                            long size = element.GetSize();
                            if (size > 0L)
                            {
                                this._installedFilter.Write(element.GetBytes(), 0, Convert.ToInt32(size));
                            }
                        }
                        wr.ClearResponse(true, false);
                    }
                    if (list != null)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            IHttpResponseElement element2 = (IHttpResponseElement) list[j];
                            long num4 = element2.GetSize();
                            if (num4 > 0L)
                            {
                                this._installedFilter.Write(element2.GetBytes(), 0, Convert.ToInt32(num4));
                            }
                        }
                    }
                    this._installedFilter.Flush();
                }
                finally
                {
                    try
                    {
                        if (finalFiltering)
                        {
                            this._installedFilter.Close();
                        }
                    }
                    finally
                    {
                        this._filterSink.Filtering = false;
                    }
                }
            }
        }

        public override void Flush()
        {
        }

        private void FlushCharBuffer(bool flushEncoder)
        {
            int charCount = this._charBufferLength - this._charBufferFree;
            if (!this._responseEncodingUpdated)
            {
                this.UpdateResponseEncoding();
            }
            this._responseEncodingUsed = true;
            int maxByteCount = this._responseEncoding.GetMaxByteCount(charCount);
            if ((maxByteCount <= 0x80) || !this._responseBufferingOn)
            {
                byte[] bytes = new byte[maxByteCount];
                int size = this._responseEncoder.GetBytes(this._charBuffer, 0, charCount, bytes, 0, flushEncoder);
                this.BufferData(bytes, 0, size, false);
            }
            else
            {
                int freeBytes = (this._lastBuffer != null) ? this._lastBuffer.FreeBytes : 0;
                if (freeBytes < maxByteCount)
                {
                    this._lastBuffer = this.CreateNewMemoryBufferElement();
                    this._buffers.Add(this._lastBuffer);
                    freeBytes = this._lastBuffer.FreeBytes;
                }
                this._lastBuffer.AppendEncodedChars(this._charBuffer, 0, charCount, this._responseEncoder, flushEncoder);
            }
            this._charBufferFree = this._charBufferLength;
        }

        internal long GetBufferedLength()
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            long num = 0L;
            if (this._buffers != null)
            {
                int count = this._buffers.Count;
                for (int i = 0; i < count; i++)
                {
                    num += ((IHttpResponseElement) this._buffers[i]).GetSize();
                }
            }
            return num;
        }

        internal Stream GetCurrentFilter()
        {
            if (this._installedFilter != null)
            {
                return this._installedFilter;
            }
            if (this._filterSink == null)
            {
                this._filterSink = new HttpResponseStreamFilterSink(this);
            }
            return this._filterSink;
        }

        internal ArrayList GetIntegratedSnapshot(out bool hasSubstBlocks, IIS7WorkerRequest wr)
        {
            ArrayList list = null;
            ArrayList snapshot = this.GetSnapshot(out hasSubstBlocks);
            ArrayList list3 = wr.GetBufferedResponseChunks(true, this._substElements, ref hasSubstBlocks);
            if (list3 != null)
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    list3.Add(snapshot[i]);
                }
                list = list3;
            }
            else
            {
                list = snapshot;
            }
            if ((this._substElements != null) && (this._substElements.Count > 0))
            {
                int num2 = 0;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j] is HttpSubstBlockResponseElement)
                    {
                        num2++;
                        if (num2 == this._substElements.Count)
                        {
                            break;
                        }
                    }
                }
                if (num2 != this._substElements.Count)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Substitution_blocks_cannot_be_modified"));
                }
                this._response.Context.Request.SetDynamicCompression(true);
            }
            return list;
        }

        internal int GetResponseBufferCountAfterFlush()
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            this._lastBuffer = null;
            return this._buffers.Count;
        }

        internal ArrayList GetSnapshot(out bool hasSubstBlocks)
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            this._lastBuffer = null;
            hasSubstBlocks = false;
            ArrayList list = new ArrayList();
            int count = this._buffers.Count;
            for (int i = 0; i < count; i++)
            {
                object obj2 = this._buffers[i];
                HttpBaseMemoryResponseBufferElement element = obj2 as HttpBaseMemoryResponseBufferElement;
                if (element != null)
                {
                    if (element.FreeBytes > 0x1000)
                    {
                        obj2 = element.Clone();
                    }
                    else
                    {
                        element.DisableRecycling();
                    }
                }
                else if (obj2 is HttpSubstBlockResponseElement)
                {
                    hasSubstBlocks = true;
                }
                list.Add(obj2);
            }
            return list;
        }

        internal void IgnoreFurtherWrites()
        {
            this._ignoringFurtherWrites = true;
        }

        internal void InstallFilter(Stream filter)
        {
            if (this._filterSink == null)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_response_filter"));
            }
            this._installedFilter = filter;
        }

        internal void MoveResponseBufferRangeForward(int srcIndex, int srcCount, int dstIndex)
        {
            if (srcCount > 0)
            {
                object[] array = new object[srcIndex - dstIndex];
                this._buffers.CopyTo(dstIndex, array, 0, array.Length);
                for (int i = 0; i < srcCount; i++)
                {
                    this._buffers[dstIndex + i] = this._buffers[srcIndex + i];
                }
                for (int j = 0; j < array.Length; j++)
                {
                    this._buffers[(dstIndex + srcCount) + j] = array[j];
                }
            }
            HttpBaseMemoryResponseBufferElement element = this._buffers[this._buffers.Count - 1] as HttpBaseMemoryResponseBufferElement;
            if ((element != null) && (element.FreeBytes > 0))
            {
                this._lastBuffer = element;
            }
            else
            {
                this._lastBuffer = null;
            }
        }

        private void RecycleBufferElements()
        {
            if (this._buffers != null)
            {
                int count = this._buffers.Count;
                for (int i = 0; i < count; i++)
                {
                    HttpBaseMemoryResponseBufferElement element = this._buffers[i] as HttpBaseMemoryResponseBufferElement;
                    if (element != null)
                    {
                        element.Recycle();
                    }
                }
                this._buffers = null;
            }
        }

        internal void RecycleBuffers()
        {
            if (this._charBuffer != null)
            {
                s_Allocator.ReuseBuffer(this._charBuffer);
                this._charBuffer = null;
            }
            this.RecycleBufferElements();
        }

        internal void Send(HttpWorkerRequest wr)
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            int count = this._buffers.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    ((IHttpResponseElement) this._buffers[i]).Send(wr);
                }
            }
        }

        internal void TransmitFile(string filename, long offset, long size, bool isImpersonating, bool supportsLongTransmitFile)
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            this._lastBuffer = null;
            this._buffers.Add(new HttpFileResponseElement(filename, offset, size, isImpersonating, supportsLongTransmitFile));
            if (!this._responseBufferingOn)
            {
                this._response.Flush();
            }
        }

        internal void UpdateResponseBuffering()
        {
            this._responseBufferingOn = this._response.BufferOutput;
        }

        internal void UpdateResponseEncoding()
        {
            if (this._responseEncodingUpdated && (this._charBufferLength != this._charBufferFree))
            {
                this.FlushCharBuffer(true);
            }
            this._responseEncoding = this._response.ContentEncoding;
            this._responseEncoder = this._response.ContentEncoder;
            this._responseCodePage = this._responseEncoding.CodePage;
            this._responseCodePageIsAsciiCompat = CodePageUtils.IsAsciiCompatibleCodePage(this._responseCodePage);
            this._responseEncodingUpdated = true;
        }

        internal void UseSnapshot(ArrayList buffers)
        {
            this.ClearBuffers();
            int count = buffers.Count;
            for (int i = 0; i < count; i++)
            {
                object obj2 = buffers[i];
                HttpSubstBlockResponseElement element = obj2 as HttpSubstBlockResponseElement;
                if (element != null)
                {
                    this._buffers.Add(element.Substitute(this.Encoding));
                }
                else
                {
                    this._buffers.Add(obj2);
                }
            }
        }

        public override void Write(char ch)
        {
            if (!this._ignoringFurtherWrites)
            {
                if (this._charBufferFree == 0)
                {
                    this.FlushCharBuffer(false);
                }
                this._charBuffer[this._charBufferLength - this._charBufferFree] = ch;
                this._charBufferFree--;
                if (!this._responseBufferingOn)
                {
                    this._response.Flush();
                }
            }
        }

        public override void Write(object obj)
        {
            if (!this._ignoringFurtherWrites && (obj != null))
            {
                this.Write(obj.ToString());
            }
        }

        public override void Write(string s)
        {
            if (!this._ignoringFurtherWrites && (s != null))
            {
                if (s.Length != 0)
                {
                    if (s.Length < this._charBufferFree)
                    {
                        StringUtil.UnsafeStringCopy(s, 0, this._charBuffer, this._charBufferLength - this._charBufferFree, s.Length);
                        this._charBufferFree -= s.Length;
                    }
                    else
                    {
                        int length = s.Length;
                        int srcIndex = 0;
                        while (length > 0)
                        {
                            if (this._charBufferFree == 0)
                            {
                                this.FlushCharBuffer(false);
                            }
                            int len = (length < this._charBufferFree) ? length : this._charBufferFree;
                            StringUtil.UnsafeStringCopy(s, srcIndex, this._charBuffer, this._charBufferLength - this._charBufferFree, len);
                            this._charBufferFree -= len;
                            srcIndex += len;
                            length -= len;
                        }
                    }
                }
                if (!this._responseBufferingOn)
                {
                    this._response.Flush();
                }
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (!this._ignoringFurtherWrites)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                if ((buffer.Length - index) < count)
                {
                    throw new ArgumentException(System.Web.SR.GetString("InvalidOffsetOrCount", new object[] { "index", "count" }));
                }
                if (count != 0)
                {
                    while (count > 0)
                    {
                        if (this._charBufferFree == 0)
                        {
                            this.FlushCharBuffer(false);
                        }
                        int length = (count < this._charBufferFree) ? count : this._charBufferFree;
                        Array.Copy(buffer, index, this._charBuffer, this._charBufferLength - this._charBufferFree, length);
                        this._charBufferFree -= length;
                        index += length;
                        count -= length;
                    }
                    if (!this._responseBufferingOn)
                    {
                        this._response.Flush();
                    }
                }
            }
        }

        public void WriteBytes(byte[] buffer, int index, int count)
        {
            if (!this._ignoringFurtherWrites)
            {
                this.WriteFromStream(buffer, index, count);
            }
        }

        internal void WriteFile(string filename, long offset, long size)
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            this._lastBuffer = null;
            this._buffers.Add(new HttpFileResponseElement(filename, offset, size));
            if (!this._responseBufferingOn)
            {
                this._response.Flush();
            }
        }

        internal void WriteFromStream(byte[] data, int offset, int size)
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            this.BufferData(data, offset, size, true);
            if (!this._responseBufferingOn)
            {
                this._response.Flush();
            }
        }

        public override void WriteLine()
        {
            if (!this._ignoringFurtherWrites)
            {
                if (this._charBufferFree < 2)
                {
                    this.FlushCharBuffer(false);
                }
                int index = this._charBufferLength - this._charBufferFree;
                this._charBuffer[index] = '\r';
                this._charBuffer[index + 1] = '\n';
                this._charBufferFree -= 2;
                if (!this._responseBufferingOn)
                {
                    this._response.Flush();
                }
            }
        }

        public void WriteString(string s, int index, int count)
        {
            if (s != null)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                if ((index + count) > s.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (!this._ignoringFurtherWrites)
                {
                    if (count != 0)
                    {
                        if (count >= this._charBufferFree)
                        {
                            while (count > 0)
                            {
                                if (this._charBufferFree == 0)
                                {
                                    this.FlushCharBuffer(false);
                                }
                                int len = (count < this._charBufferFree) ? count : this._charBufferFree;
                                StringUtil.UnsafeStringCopy(s, index, this._charBuffer, this._charBufferLength - this._charBufferFree, len);
                                this._charBufferFree -= len;
                                index += len;
                                count -= len;
                            }
                        }
                        else
                        {
                            StringUtil.UnsafeStringCopy(s, index, this._charBuffer, this._charBufferLength - this._charBufferFree, count);
                            this._charBufferFree -= count;
                        }
                    }
                    if (!this._responseBufferingOn)
                    {
                        this._response.Flush();
                    }
                }
            }
        }

        internal void WriteSubstBlock(HttpResponseSubstitutionCallback callback, IIS7WorkerRequest iis7WorkerRequest)
        {
            if (this._charBufferLength != this._charBufferFree)
            {
                this.FlushCharBuffer(true);
            }
            this._lastBuffer = null;
            IHttpResponseElement element = new HttpSubstBlockResponseElement(callback, this.Encoding, this.Encoder, iis7WorkerRequest);
            this._buffers.Add(element);
            if (iis7WorkerRequest != null)
            {
                this.SubstElements.Add(element);
            }
            if (!this._responseBufferingOn)
            {
                this._response.Flush();
            }
        }

        internal void WriteUTF8ResourceString(IntPtr pv, int offset, int size, bool asciiOnly)
        {
            if (!this._responseEncodingUpdated)
            {
                this.UpdateResponseEncoding();
            }
            if ((this._responseCodePage == 0xfde9) || (asciiOnly && this._responseCodePageIsAsciiCompat))
            {
                this._responseEncodingUsed = true;
                if (this._charBufferLength != this._charBufferFree)
                {
                    this.FlushCharBuffer(true);
                }
                this.BufferResource(pv, offset, size);
                if (!this._responseBufferingOn)
                {
                    this._response.Flush();
                }
            }
            else
            {
                this.Write(StringResourceManager.ResourceToString(pv, offset, size));
            }
        }

        internal System.Text.Encoder Encoder
        {
            get
            {
                if (!this._responseEncodingUpdated)
                {
                    this.UpdateResponseEncoding();
                }
                return this._responseEncoder;
            }
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                if (!this._responseEncodingUpdated)
                {
                    this.UpdateResponseEncoding();
                }
                return this._responseEncoding;
            }
        }

        internal bool FilterInstalled
        {
            get
            {
                return (this._installedFilter != null);
            }
        }

        internal bool HasBeenClearedRecently
        {
            get
            {
                return this._hasBeenClearedRecently;
            }
            set
            {
                this._hasBeenClearedRecently = value;
            }
        }

        internal bool IgnoringFurtherWrites
        {
            get
            {
                return this._ignoringFurtherWrites;
            }
        }

        public Stream OutputStream
        {
            get
            {
                return this._stream;
            }
        }

        internal bool ResponseEncodingUsed
        {
            get
            {
                return this._responseEncodingUsed;
            }
        }

        internal ArrayList SubstElements
        {
            get
            {
                if (this._substElements == null)
                {
                    this._substElements = new ArrayList();
                    this._response.Context.Request.SetDynamicCompression(false);
                }
                return this._substElements;
            }
        }
    }
}

