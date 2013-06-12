namespace System.Net
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public abstract class WebResponse : MarshalByRefObject, ISerializable, IDisposable
    {
        private bool m_IsCacheFresh;
        private bool m_IsFromCache;

        protected WebResponse()
        {
        }

        protected WebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        public virtual void Close()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.Close();
                }
                catch
                {
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        public virtual Stream GetResponseStream()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public virtual long ContentLength
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual string ContentType
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual WebHeaderCollection Headers
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        internal bool InternalSetFromCache
        {
            set
            {
                this.m_IsFromCache = value;
            }
        }

        internal bool InternalSetIsCacheFresh
        {
            set
            {
                this.m_IsCacheFresh = value;
            }
        }

        internal virtual bool IsCacheFresh
        {
            get
            {
                return this.m_IsCacheFresh;
            }
        }

        public virtual bool IsFromCache
        {
            get
            {
                return this.m_IsFromCache;
            }
        }

        public virtual bool IsMutuallyAuthenticated
        {
            get
            {
                return false;
            }
        }

        public virtual Uri ResponseUri
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual bool SupportsHeaders
        {
            get
            {
                return false;
            }
        }
    }
}

