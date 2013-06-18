namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class XamlDeferLoadAttribute : Attribute
    {
        private string _contentTypeName;
        private string _loaderTypeName;

        public XamlDeferLoadAttribute(string loaderType, string contentType)
        {
            if (loaderType == null)
            {
                throw new ArgumentNullException("loaderType");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            this._loaderTypeName = loaderType;
            this._contentTypeName = contentType;
        }

        public XamlDeferLoadAttribute(Type loaderType, Type contentType)
        {
            if (loaderType == null)
            {
                throw new ArgumentNullException("loaderType");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            this._loaderTypeName = loaderType.AssemblyQualifiedName;
            this._contentTypeName = contentType.AssemblyQualifiedName;
            this.LoaderType = loaderType;
            this.ContentType = contentType;
        }

        public Type ContentType { get; private set; }

        public string ContentTypeName
        {
            get
            {
                return this._contentTypeName;
            }
        }

        public Type LoaderType { get; private set; }

        public string LoaderTypeName
        {
            get
            {
                return this._loaderTypeName;
            }
        }
    }
}

