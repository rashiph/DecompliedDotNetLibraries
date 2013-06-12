namespace System.Web.Caching
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;

    [Serializable, AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Unrestricted)]
    public class SubstitutionResponseElement : ResponseElement
    {
        [NonSerialized]
        private HttpResponseSubstitutionCallback _callback;
        private string _methodName;
        private string _targetTypeName;

        private SubstitutionResponseElement()
        {
        }

        public SubstitutionResponseElement(HttpResponseSubstitutionCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            this._callback = callback;
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            Type target = BuildManager.GetType(this._targetTypeName, true, false);
            this._callback = (HttpResponseSubstitutionCallback) Delegate.CreateDelegate(typeof(HttpResponseSubstitutionCallback), target, this._methodName);
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            this._targetTypeName = Util.GetAssemblyQualifiedTypeName(this._callback.Method.ReflectedType);
            this._methodName = this._callback.Method.Name;
        }

        public HttpResponseSubstitutionCallback Callback
        {
            get
            {
                return this._callback;
            }
        }
    }
}

