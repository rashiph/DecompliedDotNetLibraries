namespace Microsoft.VisualBasic.Devices
{
    using System;
    using System.Net;

    internal class WebClientExtended : WebClient
    {
        private int m_Timeout = 0x186a0;
        private bool m_UseNonPassiveFtp;

        internal WebClientExtended()
        {
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            if (webRequest != null)
            {
                webRequest.Timeout = this.m_Timeout;
                if (this.m_UseNonPassiveFtp)
                {
                    FtpWebRequest request4 = webRequest as FtpWebRequest;
                    if (request4 != null)
                    {
                        request4.UsePassive = false;
                    }
                }
                HttpWebRequest request3 = webRequest as HttpWebRequest;
                if (request3 != null)
                {
                    request3.AllowAutoRedirect = false;
                }
            }
            return webRequest;
        }

        public int Timeout
        {
            set
            {
                this.m_Timeout = value;
            }
        }

        public bool UseNonPassiveFtp
        {
            set
            {
                this.m_UseNonPassiveFtp = value;
            }
        }
    }
}

