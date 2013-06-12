namespace System.Web
{
    using System;

    internal sealed class HttpResponseStreamFilterSink : HttpResponseStream
    {
        private bool _filtering;

        internal HttpResponseStreamFilterSink(HttpWriter writer) : base(writer)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void Flush()
        {
        }

        private void VerifyState()
        {
            if (!this._filtering)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_use_of_response_filter"));
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.VerifyState();
            base.Write(buffer, offset, count);
        }

        internal bool Filtering
        {
            get
            {
                return this._filtering;
            }
            set
            {
                this._filtering = value;
            }
        }
    }
}

