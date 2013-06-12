namespace System.Web.Configuration
{
    using System;

    public class HttpConfigurationContext
    {
        private string vpath;

        internal HttpConfigurationContext(string vpath)
        {
            this.vpath = vpath;
        }

        public string VirtualPath
        {
            get
            {
                return this.vpath;
            }
        }
    }
}

