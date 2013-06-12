namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpStaticObjectsCollectionWrapper : HttpStaticObjectsCollectionBase
    {
        private HttpStaticObjectsCollection _collection;

        public HttpStaticObjectsCollectionWrapper(HttpStaticObjectsCollection httpStaticObjectsCollection)
        {
            if (httpStaticObjectsCollection == null)
            {
                throw new ArgumentNullException("httpStaticObjectsCollection");
            }
            this._collection = httpStaticObjectsCollection;
        }

        public override void CopyTo(Array array, int index)
        {
            this._collection.CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        public override object GetObject(string name)
        {
            return this._collection.GetObject(name);
        }

        public override void Serialize(BinaryWriter writer)
        {
            this._collection.Serialize(writer);
        }

        public override int Count
        {
            get
            {
                return this._collection.Count;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this._collection.IsReadOnly;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return this._collection.IsSynchronized;
            }
        }

        public override object this[string name]
        {
            get
            {
                return this._collection[name];
            }
        }

        public override bool NeverAccessed
        {
            get
            {
                return this._collection.NeverAccessed;
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

