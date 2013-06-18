namespace System.Web.Configuration
{
    using System;

    public sealed class WebContext
    {
        private string _appConfigPath;
        private string _applicationPath;
        private string _locationSubPath;
        private string _path;
        private WebApplicationLevel _pathLevel;
        private string _site;

        public WebContext(WebApplicationLevel pathLevel, string site, string applicationPath, string path, string locationSubPath, string appConfigPath)
        {
            this._pathLevel = pathLevel;
            this._site = site;
            this._applicationPath = applicationPath;
            this._path = path;
            this._locationSubPath = locationSubPath;
            this._appConfigPath = appConfigPath;
        }

        public override string ToString()
        {
            return this._appConfigPath;
        }

        public WebApplicationLevel ApplicationLevel
        {
            get
            {
                return this._pathLevel;
            }
        }

        public string ApplicationPath
        {
            get
            {
                return this._applicationPath;
            }
        }

        public string LocationSubPath
        {
            get
            {
                return this._locationSubPath;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        public string Site
        {
            get
            {
                return this._site;
            }
        }
    }
}

