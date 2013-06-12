namespace System.Net.Mime
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Net;

    internal class MimeMultiPart : MimeBasePart
    {
        private static int boundary;
        private AsyncCallback mimePartSentCallback;
        private Collection<MimeBasePart> parts;

        internal MimeMultiPart(System.Net.Mime.MimeMultiPartType type)
        {
            this.MimeMultiPartType = type;
        }

        internal override IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, object state)
        {
            writer.WriteHeaders(base.Headers);
            MimeBasePart.MimePartAsyncResult result = new MimeBasePart.MimePartAsyncResult(this, state, callback);
            MimePartContext context = new MimePartContext(writer, result, this.Parts.GetEnumerator());
            IAsyncResult result2 = writer.BeginGetContentStream(new AsyncCallback(this.ContentStreamCallback), context);
            if (result2.CompletedSynchronously)
            {
                this.ContentStreamCallbackHandler(result2);
            }
            return result;
        }

        internal void Complete(IAsyncResult result, Exception e)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            if (asyncState.completed)
            {
                throw e;
            }
            try
            {
                asyncState.outputStream.Close();
            }
            catch (Exception exception)
            {
                if (e == null)
                {
                    e = exception;
                }
            }
            asyncState.completed = true;
            asyncState.result.InvokeCallback(e);
        }

        internal void ContentStreamCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((MimePartContext) result.AsyncState).completedSynchronously = false;
                try
                {
                    this.ContentStreamCallbackHandler(result);
                }
                catch (Exception exception)
                {
                    this.Complete(result, exception);
                }
            }
        }

        private void ContentStreamCallbackHandler(IAsyncResult result)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            asyncState.outputStream = asyncState.writer.EndGetContentStream(result);
            asyncState.writer = new MimeWriter(asyncState.outputStream, base.ContentType.Boundary);
            if (asyncState.partsEnumerator.MoveNext())
            {
                MimeBasePart current = asyncState.partsEnumerator.Current;
                this.mimePartSentCallback = new AsyncCallback(this.MimePartSentCallback);
                IAsyncResult result2 = current.BeginSend(asyncState.writer, this.mimePartSentCallback, asyncState);
                if (result2.CompletedSynchronously)
                {
                    this.MimePartSentCallbackHandler(result2);
                }
            }
            else
            {
                IAsyncResult result3 = ((MimeWriter) asyncState.writer).BeginClose(new AsyncCallback(this.MimeWriterCloseCallback), asyncState);
                if (result3.CompletedSynchronously)
                {
                    this.MimeWriterCloseCallbackHandler(result3);
                }
            }
        }

        internal string GetNextBoundary()
        {
            string str = "--boundary_" + boundary.ToString(CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString(null, CultureInfo.InvariantCulture);
            boundary++;
            return str;
        }

        internal void MimePartSentCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((MimePartContext) result.AsyncState).completedSynchronously = false;
                try
                {
                    this.MimePartSentCallbackHandler(result);
                }
                catch (Exception exception)
                {
                    this.Complete(result, exception);
                }
            }
        }

        private void MimePartSentCallbackHandler(IAsyncResult result)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            asyncState.partsEnumerator.Current.EndSend(result);
            if (asyncState.partsEnumerator.MoveNext())
            {
                IAsyncResult result2 = asyncState.partsEnumerator.Current.BeginSend(asyncState.writer, this.mimePartSentCallback, asyncState);
                if (result2.CompletedSynchronously)
                {
                    this.MimePartSentCallbackHandler(result2);
                }
            }
            else
            {
                IAsyncResult result3 = ((MimeWriter) asyncState.writer).BeginClose(new AsyncCallback(this.MimeWriterCloseCallback), asyncState);
                if (result3.CompletedSynchronously)
                {
                    this.MimeWriterCloseCallbackHandler(result3);
                }
            }
        }

        internal void MimeWriterCloseCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((MimePartContext) result.AsyncState).completedSynchronously = false;
                try
                {
                    this.MimeWriterCloseCallbackHandler(result);
                }
                catch (Exception exception)
                {
                    this.Complete(result, exception);
                }
            }
        }

        private void MimeWriterCloseCallbackHandler(IAsyncResult result)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            ((MimeWriter) asyncState.writer).EndClose(result);
            this.Complete(result, null);
        }

        internal override void Send(BaseWriter writer)
        {
            writer.WriteHeaders(base.Headers);
            Stream contentStream = writer.GetContentStream();
            MimeWriter writer2 = new MimeWriter(contentStream, base.ContentType.Boundary);
            foreach (MimeBasePart part in this.Parts)
            {
                part.Send(writer2);
            }
            writer2.Close();
            contentStream.Close();
        }

        private void SetType(System.Net.Mime.MimeMultiPartType type)
        {
            base.ContentType.MediaType = "multipart/" + type.ToString().ToLower(CultureInfo.InvariantCulture);
            base.ContentType.Boundary = this.GetNextBoundary();
        }

        internal System.Net.Mime.MimeMultiPartType MimeMultiPartType
        {
            set
            {
                if ((value > System.Net.Mime.MimeMultiPartType.Related) || (value < System.Net.Mime.MimeMultiPartType.Mixed))
                {
                    throw new NotSupportedException(value.ToString());
                }
                this.SetType(value);
            }
        }

        internal Collection<MimeBasePart> Parts
        {
            get
            {
                if (this.parts == null)
                {
                    this.parts = new Collection<MimeBasePart>();
                }
                return this.parts;
            }
        }

        internal class MimePartContext
        {
            internal bool completed;
            internal bool completedSynchronously = true;
            internal Stream outputStream;
            internal IEnumerator<MimeBasePart> partsEnumerator;
            internal LazyAsyncResult result;
            internal BaseWriter writer;

            internal MimePartContext(BaseWriter writer, LazyAsyncResult result, IEnumerator<MimeBasePart> partsEnumerator)
            {
                this.writer = writer;
                this.result = result;
                this.partsEnumerator = partsEnumerator;
            }
        }
    }
}

