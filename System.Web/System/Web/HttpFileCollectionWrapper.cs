namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpFileCollectionWrapper : HttpFileCollectionBase
    {
        private HttpFileCollection _collection;

        public HttpFileCollectionWrapper(HttpFileCollection httpFileCollection)
        {
            if (httpFileCollection == null)
            {
                throw new ArgumentNullException("httpFileCollection");
            }
            this._collection = httpFileCollection;
        }

        public override void CopyTo(Array dest, int index)
        {
            this._collection.CopyTo(dest, index);
        }

        public override HttpPostedFileBase Get(int index)
        {
            HttpPostedFile httpPostedFile = this._collection.Get(index);
            if (httpPostedFile == null)
            {
                return null;
            }
            return new HttpPostedFileWrapper(httpPostedFile);
        }

        public override HttpPostedFileBase Get(string name)
        {
            HttpPostedFile httpPostedFile = this._collection.Get(name);
            if (httpPostedFile == null)
            {
                return null;
            }
            return new HttpPostedFileWrapper(httpPostedFile);
        }

        public override IEnumerator GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        public override string GetKey(int index)
        {
            return this._collection.GetKey(index);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this._collection.GetObjectData(info, context);
        }

        public override void OnDeserialization(object sender)
        {
            this._collection.OnDeserialization(sender);
        }

        public override string[] AllKeys
        {
            get
            {
                return this._collection.AllKeys;
            }
        }

        public override int Count
        {
            get
            {
                return this._collection.Count;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return this._collection.IsSynchronized;
            }
        }

        public override HttpPostedFileBase this[string name]
        {
            get
            {
                HttpPostedFile httpPostedFile = this._collection[name];
                if (httpPostedFile == null)
                {
                    return null;
                }
                return new HttpPostedFileWrapper(httpPostedFile);
            }
        }

        public override HttpPostedFileBase this[int index]
        {
            get
            {
                HttpPostedFile httpPostedFile = this._collection[index];
                if (httpPostedFile == null)
                {
                    return null;
                }
                return new HttpPostedFileWrapper(httpPostedFile);
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return this._collection.Keys;
            }
        }

        public override object SyncRoot
        {
            get
            {
                return this._collection.SyncRoot;
            }
        }
    }
}

