namespace System.Web.SessionState
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;

    public sealed class SessionStateItemCollection : NameObjectCollectionBase, ISessionStateItemCollection, ICollection, IEnumerable
    {
        private bool _dirty;
        private int _iLastOffset;
        private KeyedCollection _serializedItems;
        private object _serializedItemsLock;
        private Stream _stream;
        private const int NO_NULL_KEY = -1;
        private static Hashtable s_immutableTypes = new Hashtable(0x13);
        private const int SIZE_OF_INT32 = 4;

        static SessionStateItemCollection()
        {
            Type key = typeof(string);
            s_immutableTypes.Add(key, key);
            key = typeof(int);
            s_immutableTypes.Add(key, key);
            key = typeof(bool);
            s_immutableTypes.Add(key, key);
            key = typeof(DateTime);
            s_immutableTypes.Add(key, key);
            key = typeof(decimal);
            s_immutableTypes.Add(key, key);
            key = typeof(byte);
            s_immutableTypes.Add(key, key);
            key = typeof(char);
            s_immutableTypes.Add(key, key);
            key = typeof(float);
            s_immutableTypes.Add(key, key);
            key = typeof(double);
            s_immutableTypes.Add(key, key);
            key = typeof(sbyte);
            s_immutableTypes.Add(key, key);
            key = typeof(short);
            s_immutableTypes.Add(key, key);
            key = typeof(long);
            s_immutableTypes.Add(key, key);
            key = typeof(ushort);
            s_immutableTypes.Add(key, key);
            key = typeof(uint);
            s_immutableTypes.Add(key, key);
            key = typeof(ulong);
            s_immutableTypes.Add(key, key);
            key = typeof(TimeSpan);
            s_immutableTypes.Add(key, key);
            key = typeof(Guid);
            s_immutableTypes.Add(key, key);
            key = typeof(IntPtr);
            s_immutableTypes.Add(key, key);
            key = typeof(UIntPtr);
            s_immutableTypes.Add(key, key);
        }

        public SessionStateItemCollection() : base(Misc.CaseInsensitiveInvariantKeyComparer)
        {
            this._serializedItemsLock = new object();
        }

        public void Clear()
        {
            lock (this._serializedItemsLock)
            {
                if (this._serializedItems != null)
                {
                    this._serializedItems.Clear();
                }
                base.BaseClear();
                this._dirty = true;
            }
        }

        public static SessionStateItemCollection Deserialize(BinaryReader reader)
        {
            SessionStateItemCollection items = new SessionStateItemCollection();
            int count = reader.ReadInt32();
            if (count > 0)
            {
                int num3;
                int num2 = reader.ReadInt32();
                items._serializedItems = new KeyedCollection(count);
                for (num3 = 0; num3 < count; num3++)
                {
                    string str;
                    if (num3 == num2)
                    {
                        str = null;
                    }
                    else
                    {
                        str = reader.ReadString();
                    }
                    items.BaseSet(str, null);
                }
                int dataLength = reader.ReadInt32();
                items._serializedItems[items.BaseGetKey(0)] = new SerializedItemPosition(0, dataLength);
                int num5 = 0;
                for (num3 = 1; num3 < count; num3++)
                {
                    num5 = reader.ReadInt32();
                    items._serializedItems[items.BaseGetKey(num3)] = new SerializedItemPosition(dataLength, num5 - dataLength);
                    dataLength = num5;
                }
                items._iLastOffset = dataLength;
                byte[] buffer = new byte[items._iLastOffset];
                if (reader.BaseStream.Read(buffer, 0, items._iLastOffset) != items._iLastOffset)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_session_state"));
                }
                items._stream = new MemoryStream(buffer);
            }
            items._dirty = false;
            return items;
        }

        internal void DeserializeAllItems()
        {
            if (this._serializedItems != null)
            {
                lock (this._serializedItemsLock)
                {
                    for (int i = 0; i < this._serializedItems.Count; i++)
                    {
                        this.DeserializeItem(this._serializedItems.GetKey(i), false);
                    }
                }
            }
        }

        private void DeserializeItem(int index)
        {
            if (this._serializedItems != null)
            {
                lock (this._serializedItemsLock)
                {
                    if (index < this._serializedItems.Count)
                    {
                        this.DeserializeItem(this._serializedItems.GetKey(index), false);
                    }
                }
            }
        }

        private void DeserializeItem(string name, bool check)
        {
            lock (this._serializedItemsLock)
            {
                if (!check || ((this._serializedItems != null) && this._serializedItems.ContainsKey(name)))
                {
                    SerializedItemPosition position = (SerializedItemPosition) this._serializedItems[name];
                    if (!position.IsDeserialized)
                    {
                        this._stream.Seek((long) position.Offset, SeekOrigin.Begin);
                        if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
                        {
                            HttpRuntime.NamedPermissionSet.PermitOnly();
                        }
                        object obj2 = this.ReadValueFromStreamWithAssert();
                        base.BaseSet(name, obj2);
                        position.MarkDeserializedOffsetAndCheck();
                    }
                }
            }
        }

        public override IEnumerator GetEnumerator()
        {
            this.DeserializeAllItems();
            return base.GetEnumerator();
        }

        internal static bool IsImmutable(object o)
        {
            return (s_immutableTypes[o.GetType()] != null);
        }

        private void MarkItemDeserialized(int index)
        {
            if (this._serializedItems != null)
            {
                lock (this._serializedItemsLock)
                {
                    if (index < this._serializedItems.Count)
                    {
                        ((SerializedItemPosition) this._serializedItems[index]).MarkDeserializedOffset();
                    }
                }
            }
        }

        private void MarkItemDeserialized(string name)
        {
            if (this._serializedItems != null)
            {
                lock (this._serializedItemsLock)
                {
                    if (this._serializedItems.ContainsKey(name))
                    {
                        ((SerializedItemPosition) this._serializedItems[name]).MarkDeserializedOffset();
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private object ReadValueFromStreamWithAssert()
        {
            return AltSerialization.ReadValueFromStream(new BinaryReader(this._stream));
        }

        public void Remove(string name)
        {
            lock (this._serializedItemsLock)
            {
                if (this._serializedItems != null)
                {
                    this._serializedItems.Remove(name);
                }
                base.BaseRemove(name);
                this._dirty = true;
            }
        }

        public void RemoveAt(int index)
        {
            lock (this._serializedItemsLock)
            {
                if ((this._serializedItems != null) && (index < this._serializedItems.Count))
                {
                    this._serializedItems.RemoveAt(index);
                }
                base.BaseRemoveAt(index);
                this._dirty = true;
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            byte[] buffer = null;
            Stream baseStream = writer.BaseStream;
            if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
            {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }
            lock (this._serializedItemsLock)
            {
                int count = this.Count;
                writer.Write(count);
                if (count > 0)
                {
                    int num2;
                    if (base.BaseGet((string) null) != null)
                    {
                        for (num2 = 0; num2 < count; num2++)
                        {
                            if (base.BaseGetKey(num2) == null)
                            {
                                writer.Write(num2);
                                break;
                            }
                        }
                    }
                    else
                    {
                        writer.Write(-1);
                    }
                    num2 = 0;
                    while (num2 < count)
                    {
                        string str = base.BaseGetKey(num2);
                        if (str != null)
                        {
                            writer.Write(str);
                        }
                        num2++;
                    }
                    long num3 = baseStream.Position;
                    baseStream.Seek((long) (4 * count), SeekOrigin.Current);
                    long num4 = baseStream.Position;
                    for (num2 = 0; num2 < count; num2++)
                    {
                        if (((this._serializedItems != null) && (num2 < this._serializedItems.Count)) && !((SerializedItemPosition) this._serializedItems[num2]).IsDeserialized)
                        {
                            SerializedItemPosition position = (SerializedItemPosition) this._serializedItems[num2];
                            this._stream.Seek((long) position.Offset, SeekOrigin.Begin);
                            if ((buffer == null) || (buffer.Length < position.DataLength))
                            {
                                buffer = new byte[position.DataLength];
                            }
                            this._stream.Read(buffer, 0, position.DataLength);
                            baseStream.Write(buffer, 0, position.DataLength);
                        }
                        else
                        {
                            object obj2 = base.BaseGet(num2);
                            this.WriteValueToStreamWithAssert(obj2, writer);
                        }
                        long offset = baseStream.Position;
                        baseStream.Seek((num2 * 4) + num3, SeekOrigin.Begin);
                        writer.Write((int) (offset - num4));
                        baseStream.Seek(offset, SeekOrigin.Begin);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private void WriteValueToStreamWithAssert(object value, BinaryWriter writer)
        {
            AltSerialization.WriteValueToStream(value, writer);
        }

        public bool Dirty
        {
            get
            {
                return this._dirty;
            }
            set
            {
                this._dirty = value;
            }
        }

        public object this[string name]
        {
            get
            {
                this.DeserializeItem(name, true);
                object o = base.BaseGet(name);
                if ((o != null) && !IsImmutable(o))
                {
                    this._dirty = true;
                }
                return o;
            }
            set
            {
                this.MarkItemDeserialized(name);
                base.BaseSet(name, value);
                this._dirty = true;
            }
        }

        public object this[int index]
        {
            get
            {
                this.DeserializeItem(index);
                object o = base.BaseGet(index);
                if ((o != null) && !IsImmutable(o))
                {
                    this._dirty = true;
                }
                return o;
            }
            set
            {
                this.MarkItemDeserialized(index);
                base.BaseSet(index, value);
                this._dirty = true;
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                this.DeserializeAllItems();
                return base.Keys;
            }
        }

        private class KeyedCollection : NameObjectCollectionBase
        {
            internal KeyedCollection(int count) : base(count, Misc.CaseInsensitiveInvariantKeyComparer)
            {
            }

            internal void Clear()
            {
                base.BaseClear();
            }

            internal bool ContainsKey(string name)
            {
                return (base.BaseGet(name) != null);
            }

            internal string GetKey(int index)
            {
                return base.BaseGetKey(index);
            }

            internal void Remove(string name)
            {
                base.BaseRemove(name);
            }

            internal void RemoveAt(int index)
            {
                base.BaseRemoveAt(index);
            }

            internal object this[string name]
            {
                get
                {
                    return base.BaseGet(name);
                }
                set
                {
                    if ((base.BaseGet(name) != null) || (value != null))
                    {
                        base.BaseSet(name, value);
                    }
                }
            }

            internal object this[int index]
            {
                get
                {
                    return base.BaseGet(index);
                }
            }
        }

        private class SerializedItemPosition
        {
            private int _dataLength;
            private int _offset;

            internal SerializedItemPosition(int offset, int dataLength)
            {
                this._offset = offset;
                this._dataLength = dataLength;
            }

            internal void MarkDeserializedOffset()
            {
                this._offset = -1;
            }

            internal void MarkDeserializedOffsetAndCheck()
            {
                if (this._offset >= 0)
                {
                    this.MarkDeserializedOffset();
                }
            }

            internal int DataLength
            {
                get
                {
                    return this._dataLength;
                }
            }

            internal bool IsDeserialized
            {
                get
                {
                    return (this._offset < 0);
                }
            }

            internal int Offset
            {
                get
                {
                    return this._offset;
                }
            }
        }
    }
}

