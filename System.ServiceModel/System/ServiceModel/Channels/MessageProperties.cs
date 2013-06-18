namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class MessageProperties : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IDisposable
    {
        private object allowOutputBatching;
        private const int AllowOutputBatchingIndex = -3;
        private const string AllowOutputBatchingKey = "AllowOutputBatching";
        private bool disposed;
        private MessageEncoder encoder;
        private const int EncoderIndex = -5;
        private const string EncoderKey = "Encoder";
        private static object falseBool = false;
        private const int InitialPropertyCount = 2;
        private const int MaxRecycledArrayLength = 8;
        private const int NotFoundIndex = -1;
        private Property[] properties;
        private int propertyCount;
        private SecurityMessageProperty security;
        private const int SecurityIndex = -4;
        private const string SecurityKey = "Security";
        private static object trueBool = true;
        private Uri via;
        private const int ViaIndex = -2;
        private const string ViaKey = "Via";

        public MessageProperties()
        {
        }

        public MessageProperties(MessageProperties properties)
        {
            this.CopyProperties(properties);
        }

        internal MessageProperties(KeyValuePair<string, object>[] array)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            }
            this.CopyProperties(array);
        }

        public void Add(string name, object property)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (property == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("property"));
            }
            this.UpdateProperty(name, property, true);
        }

        private void AdjustPropertyCount(bool oldValueIsNull, bool newValueIsNull)
        {
            if (newValueIsNull)
            {
                if (!oldValueIsNull)
                {
                    this.propertyCount--;
                }
            }
            else if (oldValueIsNull)
            {
                this.propertyCount++;
            }
        }

        public void Clear()
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (this.properties != null)
            {
                for (int i = 0; i < this.properties.Length; i++)
                {
                    if (this.properties[i].Name == null)
                    {
                        break;
                    }
                    this.properties[i] = new Property();
                }
            }
            this.via = null;
            this.allowOutputBatching = null;
            this.security = null;
            this.encoder = null;
            this.propertyCount = 0;
        }

        public bool ContainsKey(string name)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            switch (this.FindProperty(name))
            {
                case -5:
                    return (this.encoder != null);

                case -4:
                    return (this.security != null);

                case -3:
                    return (this.allowOutputBatching != null);

                case -2:
                    return (this.via != null);

                case -1:
                    return false;
            }
            return true;
        }

        public void CopyProperties(MessageProperties properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (properties.properties != null)
            {
                for (int i = 0; i < properties.properties.Length; i++)
                {
                    if (properties.properties[i].Name == null)
                    {
                        break;
                    }
                    Property property = properties.properties[i];
                    this[property.Name] = property.Value;
                }
            }
            this.Via = properties.Via;
            this.AllowOutputBatching = properties.AllowOutputBatching;
            this.Security = (properties.Security != null) ? ((SecurityMessageProperty) properties.Security.CreateCopy()) : null;
            this.Encoder = properties.Encoder;
        }

        internal void CopyProperties(KeyValuePair<string, object>[] array)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            for (int i = 0; i < array.Length; i++)
            {
                KeyValuePair<string, object> pair = array[i];
                this[pair.Key] = pair.Value;
            }
        }

        private object CreateCopyOfPropertyValue(object propertyValue)
        {
            IMessageProperty property = propertyValue as IMessageProperty;
            if (property == null)
            {
                return propertyValue;
            }
            object obj2 = property.CreateCopy();
            if (obj2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessagePropertyReturnedNullCopy")));
            }
            return obj2;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (this.properties != null)
                {
                    for (int i = 0; i < this.properties.Length; i++)
                    {
                        if (this.properties[i].Name == null)
                        {
                            break;
                        }
                        this.properties[i].Dispose();
                    }
                }
                if (this.security != null)
                {
                    this.security.Dispose();
                }
            }
        }

        private int FindProperty(string name)
        {
            if (name == "Via")
            {
                return -2;
            }
            if (name == "AllowOutputBatching")
            {
                return -3;
            }
            if (name == "Encoder")
            {
                return -5;
            }
            if (name == "Security")
            {
                return -4;
            }
            if (this.properties != null)
            {
                for (int i = 0; i < this.properties.Length; i++)
                {
                    string str = this.properties[i].Name;
                    if (str == null)
                    {
                        break;
                    }
                    if (str == name)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal void Recycle()
        {
            this.disposed = false;
            this.Clear();
        }

        public bool Remove(string name)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            int propertyCount = this.propertyCount;
            this.UpdateProperty(name, null, false);
            return (propertyCount != this.propertyCount);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> pair)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (pair.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pair.Value"));
            }
            this.UpdateProperty(pair.Key, pair.Value, true);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> pair)
        {
            object obj2;
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (pair.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pair.Value"));
            }
            if (pair.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pair.Key"));
            }
            if (!this.TryGetValue(pair.Key, out obj2))
            {
                return false;
            }
            return obj2.Equals(pair.Value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int index)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            }
            if (array.Length < this.propertyCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessagePropertiesArraySize0")));
            }
            if ((index < 0) || (index > (array.Length - this.propertyCount)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, array.Length - this.propertyCount })));
            }
            if (this.via != null)
            {
                array[index++] = new KeyValuePair<string, object>("Via", this.via);
            }
            if (this.allowOutputBatching != null)
            {
                array[index++] = new KeyValuePair<string, object>("AllowOutputBatching", this.allowOutputBatching);
            }
            if (this.security != null)
            {
                array[index++] = new KeyValuePair<string, object>("Security", this.security.CreateCopy());
            }
            if (this.encoder != null)
            {
                array[index++] = new KeyValuePair<string, object>("Encoder", this.encoder);
            }
            if (this.properties != null)
            {
                for (int i = 0; i < this.properties.Length; i++)
                {
                    string name = this.properties[i].Name;
                    if (name == null)
                    {
                        return;
                    }
                    array[index++] = new KeyValuePair<string, object>(name, this.CreateCopyOfPropertyValue(this.properties[i].Value));
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> pair)
        {
            object obj2;
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (pair.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pair.Value"));
            }
            if (pair.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pair.Key"));
            }
            if (!this.TryGetValue(pair.Key, out obj2))
            {
                return false;
            }
            if (!obj2.Equals(pair.Value))
            {
                return false;
            }
            this.Remove(pair.Key);
            return true;
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>(this.propertyCount);
            if (this.via != null)
            {
                list.Add(new KeyValuePair<string, object>("Via", this.via));
            }
            if (this.allowOutputBatching != null)
            {
                list.Add(new KeyValuePair<string, object>("AllowOutputBatching", this.allowOutputBatching));
            }
            if (this.security != null)
            {
                list.Add(new KeyValuePair<string, object>("Security", this.security));
            }
            if (this.encoder != null)
            {
                list.Add(new KeyValuePair<string, object>("Encoder", this.encoder));
            }
            if (this.properties != null)
            {
                for (int i = 0; i < this.properties.Length; i++)
                {
                    string name = this.properties[i].Name;
                    if (name == null)
                    {
                        break;
                    }
                    list.Add(new KeyValuePair<string, object>(name, this.properties[i].Value));
                }
            }
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            return ((IEnumerable<KeyValuePair<string, object>>) this).GetEnumerator();
        }

        private void ThrowDisposed()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(string.Empty, System.ServiceModel.SR.GetString("ObjectDisposed", new object[] { base.GetType().ToString() })));
        }

        public bool TryGetValue(string name, out object value)
        {
            if (this.disposed)
            {
                this.ThrowDisposed();
            }
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            int index = this.FindProperty(name);
            switch (index)
            {
                case -5:
                    value = this.encoder;
                    break;

                case -4:
                    value = this.security;
                    break;

                case -3:
                    value = this.allowOutputBatching;
                    break;

                case -2:
                    value = this.via;
                    break;

                case -1:
                    value = null;
                    break;

                default:
                    value = this.properties[index].Value;
                    break;
            }
            return (value != null);
        }

        internal bool TryGetValue<TProperty>(string name, out TProperty property)
        {
            object obj2;
            if (this.TryGetValue(name, out obj2))
            {
                property = (TProperty) obj2;
                return true;
            }
            property = default(TProperty);
            return false;
        }

        private void UpdateProperty(string name, object value, bool mustNotExist)
        {
            int num3;
            object obj2;
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            int index = this.FindProperty(name);
            if (index == -1)
            {
                if (value == null)
                {
                    return;
                }
                if (this.properties == null)
                {
                    this.properties = new Property[2];
                    num3 = 0;
                    goto Label_0255;
                }
                num3 = 0;
                while (num3 < this.properties.Length)
                {
                    if (this.properties[num3].Name == null)
                    {
                        break;
                    }
                    num3++;
                }
            }
            else
            {
                if (mustNotExist)
                {
                    bool flag;
                    switch (index)
                    {
                        case -5:
                            flag = this.encoder != null;
                            break;

                        case -4:
                            flag = this.security != null;
                            break;

                        case -3:
                            flag = this.allowOutputBatching != null;
                            break;

                        case -2:
                            flag = this.via != null;
                            break;

                        default:
                            flag = true;
                            break;
                    }
                    if (flag)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("DuplicateMessageProperty", new object[] { name })));
                    }
                }
                if (index < 0)
                {
                    switch (index)
                    {
                        case -5:
                            this.Encoder = (MessageEncoder) value;
                            return;

                        case -4:
                            if (this.Security != null)
                            {
                                this.Security.Dispose();
                            }
                            this.Security = (SecurityMessageProperty) this.CreateCopyOfPropertyValue(value);
                            return;

                        case -3:
                            this.AllowOutputBatching = (bool) value;
                            return;

                        case -2:
                            this.Via = (Uri) value;
                            return;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
                }
                if (value != null)
                {
                    this.properties[index].Value = this.CreateCopyOfPropertyValue(value);
                    return;
                }
                this.properties[index].Dispose();
                int num2 = index + 1;
                while (num2 < this.properties.Length)
                {
                    if (this.properties[num2].Name == null)
                    {
                        break;
                    }
                    this.properties[num2 - 1] = this.properties[num2];
                    num2++;
                }
                this.properties[num2 - 1] = new Property();
                this.propertyCount--;
                return;
            }
            if (num3 == this.properties.Length)
            {
                Property[] destinationArray = new Property[this.properties.Length * 2];
                Array.Copy(this.properties, destinationArray, this.properties.Length);
                this.properties = destinationArray;
            }
        Label_0255:
            obj2 = this.CreateCopyOfPropertyValue(value);
            this.properties[num3] = new Property(name, obj2);
            this.propertyCount++;
        }

        public bool AllowOutputBatching
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return (this.allowOutputBatching == trueBool);
            }
            set
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                this.AdjustPropertyCount(this.allowOutputBatching == null, false);
                if (value)
                {
                    this.allowOutputBatching = trueBool;
                }
                else
                {
                    this.allowOutputBatching = falseBool;
                }
            }
        }

        internal bool CanRecycle
        {
            get
            {
                if (this.properties != null)
                {
                    return (this.properties.Length <= 8);
                }
                return true;
            }
        }

        public int Count
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return this.propertyCount;
            }
        }

        public MessageEncoder Encoder
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return this.encoder;
            }
            set
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                this.AdjustPropertyCount(this.encoder == null, value == null);
                this.encoder = value;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return false;
            }
        }

        public object this[string name]
        {
            get
            {
                object obj2;
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                if (!this.TryGetValue(name, out obj2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessagePropertyNotFound", new object[] { name })));
                }
                return obj2;
            }
            set
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                this.UpdateProperty(name, value, false);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                List<string> list = new List<string>();
                if (this.via != null)
                {
                    list.Add("Via");
                }
                if (this.allowOutputBatching != null)
                {
                    list.Add("AllowOutputBatching");
                }
                if (this.security != null)
                {
                    list.Add("Security");
                }
                if (this.encoder != null)
                {
                    list.Add("Encoder");
                }
                if (this.properties != null)
                {
                    for (int i = 0; i < this.properties.Length; i++)
                    {
                        string name = this.properties[i].Name;
                        if (name == null)
                        {
                            return list;
                        }
                        list.Add(name);
                    }
                }
                return list;
            }
        }

        public SecurityMessageProperty Security
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return this.security;
            }
            set
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                this.AdjustPropertyCount(this.security == null, value == null);
                this.security = value;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                List<object> list = new List<object>();
                if (this.via != null)
                {
                    list.Add(this.via);
                }
                if (this.allowOutputBatching != null)
                {
                    list.Add(this.allowOutputBatching);
                }
                if (this.security != null)
                {
                    list.Add(this.security);
                }
                if (this.encoder != null)
                {
                    list.Add(this.encoder);
                }
                if (this.properties != null)
                {
                    for (int i = 0; i < this.properties.Length; i++)
                    {
                        if (this.properties[i].Name == null)
                        {
                            return list;
                        }
                        list.Add(this.properties[i].Value);
                    }
                }
                return list;
            }
        }

        public Uri Via
        {
            get
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                return this.via;
            }
            set
            {
                if (this.disposed)
                {
                    this.ThrowDisposed();
                }
                this.AdjustPropertyCount(this.via == null, value == null);
                this.via = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Property : IDisposable
        {
            private string name;
            private object value;
            public Property(string name, object value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }
            public object Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                }
            }
            public void Dispose()
            {
                IDisposable disposable = this.value as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}

