namespace System.Web.Services.Description
{
    using System;

    internal abstract class MimeReflector
    {
        private HttpProtocolReflector protocol;

        protected MimeReflector()
        {
        }

        internal abstract bool ReflectParameters();
        internal abstract bool ReflectReturn();

        internal HttpProtocolReflector ReflectionContext
        {
            get
            {
                return this.protocol;
            }
            set
            {
                this.protocol = value;
            }
        }
    }
}

