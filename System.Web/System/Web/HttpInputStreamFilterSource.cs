namespace System.Web
{
    using System;

    internal class HttpInputStreamFilterSource : HttpInputStream
    {
        internal HttpInputStreamFilterSource() : base(null, 0, 0)
        {
        }

        internal void SetContent(HttpRawUploadedContent data)
        {
            if (data != null)
            {
                base.Init(data, 0, data.Length);
            }
            else
            {
                base.Uninit();
            }
        }
    }
}

