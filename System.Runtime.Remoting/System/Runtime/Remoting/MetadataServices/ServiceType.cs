namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Runtime;

    public class ServiceType
    {
        private Type _type;
        private string _url;

        public ServiceType(Type type)
        {
            this._type = type;
            this._url = null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceType(Type type, string url)
        {
            this._type = type;
            this._url = url;
        }

        public Type ObjectType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._type;
            }
        }

        public string Url
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._url;
            }
        }
    }
}

