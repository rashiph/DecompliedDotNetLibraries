namespace System.Deployment.Application
{
    using System;

    internal class ServerInformation
    {
        private string _aspNetVersion;
        private string _poweredBy;
        private string _server;

        public string AspNetVersion
        {
            get
            {
                return this._aspNetVersion;
            }
            set
            {
                this._aspNetVersion = value;
            }
        }

        public string PoweredBy
        {
            get
            {
                return this._poweredBy;
            }
            set
            {
                this._poweredBy = value;
            }
        }

        public string Server
        {
            get
            {
                return this._server;
            }
            set
            {
                this._server = value;
            }
        }
    }
}

