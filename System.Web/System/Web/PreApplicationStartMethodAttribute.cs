namespace System.Web
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public sealed class PreApplicationStartMethodAttribute : Attribute
    {
        private readonly string _methodName;
        private readonly System.Type _type;

        public PreApplicationStartMethodAttribute(System.Type type, string methodName)
        {
            this._type = type;
            this._methodName = methodName;
        }

        public string MethodName
        {
            get
            {
                return this._methodName;
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

