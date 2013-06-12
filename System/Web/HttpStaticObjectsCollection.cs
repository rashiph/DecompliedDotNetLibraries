namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Web.Util;

    public sealed class HttpStaticObjectsCollection : ICollection, IEnumerable
    {
        private IDictionary _objects = new Hashtable(StringComparer.OrdinalIgnoreCase);

        internal void Add(string name, Type t, bool lateBound)
        {
            this._objects.Add(name, new HttpStaticObjectsEntry(name, t, lateBound));
        }

        internal HttpStaticObjectsCollection Clone()
        {
            HttpStaticObjectsCollection objectss = new HttpStaticObjectsCollection();
            IDictionaryEnumerator enumerator = this._objects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) enumerator.Value;
                objectss.Add(entry.Name, entry.ObjectType, entry.LateBound);
            }
            return objectss;
        }

        public void CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        public static HttpStaticObjectsCollection Deserialize(BinaryReader reader)
        {
            HttpStaticObjectsCollection objectss = new HttpStaticObjectsCollection();
            int num = reader.ReadInt32();
            while (num-- > 0)
            {
                HttpStaticObjectsEntry entry;
                string name = reader.ReadString();
                if (reader.ReadBoolean())
                {
                    object instance = AltSerialization.ReadValueFromStream(reader);
                    entry = new HttpStaticObjectsEntry(name, instance, 0);
                }
                else
                {
                    string typeName = reader.ReadString();
                    bool lateBound = reader.ReadBoolean();
                    entry = new HttpStaticObjectsEntry(name, Type.GetType(typeName), lateBound);
                }
                objectss._objects.Add(name, entry);
            }
            return objectss;
        }

        public IEnumerator GetEnumerator()
        {
            return new HttpStaticObjectsEnumerator(this._objects.GetEnumerator());
        }

        internal int GetInstanceCount()
        {
            int num = 0;
            IDictionaryEnumerator enumerator = this._objects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) enumerator.Value;
                if (entry.HasInstance)
                {
                    num++;
                }
            }
            return num;
        }

        public object GetObject(string name)
        {
            return this[name];
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(this.Count);
            IDictionaryEnumerator enumerator = this._objects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) enumerator.Value;
                writer.Write(entry.Name);
                bool hasInstance = entry.HasInstance;
                writer.Write(hasInstance);
                if (hasInstance)
                {
                    AltSerialization.WriteValueToStream(entry.Instance, writer);
                }
                else
                {
                    writer.Write(entry.ObjectType.FullName);
                    writer.Write(entry.LateBound);
                }
            }
        }

        public int Count
        {
            get
            {
                return this._objects.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object this[string name]
        {
            get
            {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) this._objects[name];
                if (entry == null)
                {
                    return null;
                }
                return entry.Instance;
            }
        }

        public bool NeverAccessed
        {
            get
            {
                return (this.GetInstanceCount() == 0);
            }
        }

        internal IDictionary Objects
        {
            get
            {
                return this._objects;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

