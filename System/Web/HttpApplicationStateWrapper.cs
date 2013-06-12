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
    public class HttpApplicationStateWrapper : HttpApplicationStateBase
    {
        private HttpApplicationState _application;

        public HttpApplicationStateWrapper(HttpApplicationState httpApplicationState)
        {
            if (httpApplicationState == null)
            {
                throw new ArgumentNullException("httpApplicationState");
            }
            this._application = httpApplicationState;
        }

        public override void Add(string name, object value)
        {
            this._application.Add(name, value);
        }

        public override void Clear()
        {
            this._application.Clear();
        }

        public override void CopyTo(Array array, int index)
        {
            ((ICollection) this._application).CopyTo(array, index);
        }

        public override object Get(int index)
        {
            return this._application.Get(index);
        }

        public override object Get(string name)
        {
            return this._application.Get(name);
        }

        public override IEnumerator GetEnumerator()
        {
            return this._application.GetEnumerator();
        }

        public override string GetKey(int index)
        {
            return this._application.GetKey(index);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this._application.GetObjectData(info, context);
        }

        public override void Lock()
        {
            this._application.Lock();
        }

        public override void OnDeserialization(object sender)
        {
            this._application.OnDeserialization(sender);
        }

        public override void Remove(string name)
        {
            this._application.Remove(name);
        }

        public override void RemoveAll()
        {
            this._application.RemoveAll();
        }

        public override void RemoveAt(int index)
        {
            this._application.RemoveAt(index);
        }

        public override void Set(string name, object value)
        {
            this._application.Set(name, value);
        }

        public override void UnLock()
        {
            this._application.UnLock();
        }

        public override string[] AllKeys
        {
            get
            {
                return this._application.AllKeys;
            }
        }

        public override HttpApplicationStateBase Contents
        {
            get
            {
                return this;
            }
        }

        public override int Count
        {
            get
            {
                return this._application.Count;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return this._application.IsSynchronized;
            }
        }

        public override object this[int index]
        {
            get
            {
                return this._application[index];
            }
        }

        public override object this[string name]
        {
            get
            {
                return this._application[name];
            }
            set
            {
                this._application[name] = value;
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return this._application.Keys;
            }
        }

        public override HttpStaticObjectsCollectionBase StaticObjects
        {
            get
            {
                return new HttpStaticObjectsCollectionWrapper(this._application.StaticObjects);
            }
        }

        public override object SyncRoot
        {
            get
            {
                return this._application.SyncRoot;
            }
        }
    }
}

