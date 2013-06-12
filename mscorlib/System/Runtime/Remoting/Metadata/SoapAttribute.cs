namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class SoapAttribute : Attribute
    {
        private bool _bEmbedded;
        private bool _bUseAttribute;
        protected string ProtXmlNamespace;
        protected object ReflectInfo;

        internal void SetReflectInfo(object info)
        {
            this.ReflectInfo = info;
        }

        public virtual bool Embedded
        {
            get
            {
                return this._bEmbedded;
            }
            set
            {
                this._bEmbedded = value;
            }
        }

        public virtual bool UseAttribute
        {
            get
            {
                return this._bUseAttribute;
            }
            set
            {
                this._bUseAttribute = value;
            }
        }

        public virtual string XmlNamespace
        {
            get
            {
                return this.ProtXmlNamespace;
            }
            set
            {
                this.ProtXmlNamespace = value;
            }
        }
    }
}

