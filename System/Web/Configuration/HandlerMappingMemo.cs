namespace System.Web.Configuration
{
    using System;
    using System.Web;

    internal class HandlerMappingMemo
    {
        private HttpHandlerAction _mapping;
        private VirtualPath _path;
        private string _verb;

        internal HandlerMappingMemo(HttpHandlerAction mapping, string verb, VirtualPath path)
        {
            this._mapping = mapping;
            this._verb = verb;
            this._path = path;
        }

        internal bool IsMatch(string verb, VirtualPath path)
        {
            return (this._verb.Equals(verb) && this._path.Equals(path));
        }

        internal HttpHandlerAction Mapping
        {
            get
            {
                return this._mapping;
            }
        }
    }
}

