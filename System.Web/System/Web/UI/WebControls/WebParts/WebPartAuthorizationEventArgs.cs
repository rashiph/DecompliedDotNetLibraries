namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public class WebPartAuthorizationEventArgs : EventArgs
    {
        private string _authorizationFilter;
        private bool _isAuthorized;
        private bool _isShared;
        private string _path;
        private System.Type _type;

        public WebPartAuthorizationEventArgs(System.Type type, string path, string authorizationFilter, bool isShared)
        {
            this._type = type;
            this._path = path;
            this._authorizationFilter = authorizationFilter;
            this._isShared = isShared;
            this._isAuthorized = true;
        }

        public string AuthorizationFilter
        {
            get
            {
                return this._authorizationFilter;
            }
        }

        public bool IsAuthorized
        {
            get
            {
                return this._isAuthorized;
            }
            set
            {
                this._isAuthorized = value;
            }
        }

        public bool IsShared
        {
            get
            {
                return this._isShared;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        public System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

