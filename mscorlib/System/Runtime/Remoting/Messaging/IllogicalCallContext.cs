namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;

    internal class IllogicalCallContext : ICloneable
    {
        private Hashtable m_Datastore;
        private object m_HostContext;

        public object Clone()
        {
            IllogicalCallContext context = new IllogicalCallContext();
            if (this.HasUserData)
            {
                IDictionaryEnumerator enumerator = this.m_Datastore.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    context.Datastore[(string) enumerator.Key] = enumerator.Value;
                }
            }
            return context;
        }

        public void FreeNamedDataSlot(string name)
        {
            this.Datastore.Remove(name);
        }

        public object GetData(string name)
        {
            return this.Datastore[name];
        }

        public void SetData(string name, object data)
        {
            this.Datastore[name] = data;
        }

        private Hashtable Datastore
        {
            get
            {
                if (this.m_Datastore == null)
                {
                    this.m_Datastore = new Hashtable();
                }
                return this.m_Datastore;
            }
        }

        internal bool HasUserData
        {
            get
            {
                return ((this.m_Datastore != null) && (this.m_Datastore.Count > 0));
            }
        }

        internal object HostContext
        {
            get
            {
                return this.m_HostContext;
            }
            set
            {
                this.m_HostContext = value;
            }
        }
    }
}

