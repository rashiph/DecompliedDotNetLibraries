namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class DbConnectionStringBuilder : IDictionary, ICollection, IEnumerable, ICustomTypeDescriptor
    {
        private bool _browsableConnectionString;
        private string _connectionString;
        private Dictionary<string, object> _currentValues;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private PropertyDescriptorCollection _propertyDescriptors;
        private readonly bool UseOdbcRules;

        public DbConnectionStringBuilder()
        {
            this._connectionString = "";
            this._browsableConnectionString = true;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
        }

        public DbConnectionStringBuilder(bool useOdbcRules)
        {
            this._connectionString = "";
            this._browsableConnectionString = true;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this.UseOdbcRules = useOdbcRules;
        }

        public void Add(string keyword, object value)
        {
            this[keyword] = value;
        }

        public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value)
        {
            DbConnectionOptions.AppendKeyValuePairBuilder(builder, keyword, value, false);
        }

        public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value, bool useOdbcRules)
        {
            DbConnectionOptions.AppendKeyValuePairBuilder(builder, keyword, value, useOdbcRules);
        }

        public virtual void Clear()
        {
            Bid.Trace("<comm.DbConnectionStringBuilder.Clear|API>\n");
            this._connectionString = "";
            this._propertyDescriptors = null;
            this.CurrentValues.Clear();
        }

        protected internal void ClearPropertyDescriptors()
        {
            this._propertyDescriptors = null;
        }

        public virtual bool ContainsKey(string keyword)
        {
            ADP.CheckArgumentNull(keyword, "keyword");
            return this.CurrentValues.ContainsKey(keyword);
        }

        public virtual bool EquivalentTo(DbConnectionStringBuilder connectionStringBuilder)
        {
            ADP.CheckArgumentNull(connectionStringBuilder, "connectionStringBuilder");
            Bid.Trace("<comm.DbConnectionStringBuilder.EquivalentTo|API> %d#, connectionStringBuilder=%d#\n", this.ObjectID, connectionStringBuilder.ObjectID);
            if ((base.GetType() != connectionStringBuilder.GetType()) || (this.CurrentValues.Count != connectionStringBuilder.CurrentValues.Count))
            {
                return false;
            }
            foreach (KeyValuePair<string, object> pair in this.CurrentValues)
            {
                object obj2;
                if (!connectionStringBuilder.CurrentValues.TryGetValue(pair.Key, out obj2) || !pair.Value.Equals(obj2))
                {
                    return false;
                }
            }
            return true;
        }

        internal Attribute[] GetAttributesFromCollection(AttributeCollection collection)
        {
            Attribute[] array = new Attribute[collection.Count];
            collection.CopyTo(array, 0);
            return array;
        }

        private PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection descriptors = this._propertyDescriptors;
            if (descriptors == null)
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<comm.DbConnectionStringBuilder.GetProperties|INFO> %d#", this.ObjectID);
                try
                {
                    Hashtable propertyDescriptors = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    this.GetProperties(propertyDescriptors);
                    PropertyDescriptor[] array = new PropertyDescriptor[propertyDescriptors.Count];
                    propertyDescriptors.Values.CopyTo(array, 0);
                    descriptors = new PropertyDescriptorCollection(array);
                    this._propertyDescriptors = descriptors;
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
            return descriptors;
        }

        private PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = this.GetProperties();
            if ((attributes == null) || (attributes.Length == 0))
            {
                return properties;
            }
            PropertyDescriptor[] sourceArray = new PropertyDescriptor[properties.Count];
            int index = 0;
            foreach (PropertyDescriptor descriptor in properties)
            {
                bool flag = true;
                foreach (Attribute attribute in attributes)
                {
                    Attribute attribute2 = descriptor.Attributes[attribute.GetType()];
                    if (((attribute2 == null) && !attribute.IsDefaultAttribute()) || !attribute2.Match(attribute))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    sourceArray[index] = descriptor;
                    index++;
                }
            }
            PropertyDescriptor[] destinationArray = new PropertyDescriptor[index];
            Array.Copy(sourceArray, destinationArray, index);
            return new PropertyDescriptorCollection(destinationArray);
        }

        protected virtual void GetProperties(Hashtable propertyDescriptors)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbConnectionStringBuilder.GetProperties|API> %d#", this.ObjectID);
            try
            {
                Attribute[] attributesFromCollection;
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this, true))
                {
                    if ("ConnectionString" != descriptor.Name)
                    {
                        string displayName = descriptor.DisplayName;
                        if (!propertyDescriptors.ContainsKey(displayName))
                        {
                            attributesFromCollection = this.GetAttributesFromCollection(descriptor.Attributes);
                            PropertyDescriptor descriptor3 = new DbConnectionStringBuilderDescriptor(descriptor.Name, descriptor.ComponentType, descriptor.PropertyType, descriptor.IsReadOnly, attributesFromCollection);
                            propertyDescriptors[displayName] = descriptor3;
                        }
                    }
                    else if (this.BrowsableConnectionString)
                    {
                        propertyDescriptors["ConnectionString"] = descriptor;
                    }
                    else
                    {
                        propertyDescriptors.Remove("ConnectionString");
                    }
                }
                if (!this.IsFixedSize)
                {
                    attributesFromCollection = null;
                    foreach (string str in this.Keys)
                    {
                        if (!propertyDescriptors.ContainsKey(str))
                        {
                            Type type;
                            object obj2 = this[str];
                            if (obj2 != null)
                            {
                                type = obj2.GetType();
                                if (typeof(string) == type)
                                {
                                    int num;
                                    if (int.TryParse((string) obj2, out num))
                                    {
                                        type = typeof(int);
                                    }
                                    else
                                    {
                                        bool flag;
                                        if (bool.TryParse((string) obj2, out flag))
                                        {
                                            type = typeof(bool);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                type = typeof(string);
                            }
                            Attribute[] attributes = attributesFromCollection;
                            if (StringComparer.OrdinalIgnoreCase.Equals("Password", str) || StringComparer.OrdinalIgnoreCase.Equals("pwd", str))
                            {
                                attributes = new Attribute[] { BrowsableAttribute.Yes, PasswordPropertyTextAttribute.Yes, new ResCategoryAttribute("DataCategory_Security"), RefreshPropertiesAttribute.All };
                            }
                            else if (attributesFromCollection == null)
                            {
                                attributesFromCollection = new Attribute[] { BrowsableAttribute.Yes, RefreshPropertiesAttribute.All };
                                attributes = attributesFromCollection;
                            }
                            PropertyDescriptor descriptor2 = new DbConnectionStringBuilderDescriptor(str, base.GetType(), type, false, attributes);
                            propertyDescriptors[str] = descriptor2;
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private string ObjectToString(object keyword)
        {
            string str;
            try
            {
                str = (string) keyword;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("keyword", "not a string");
            }
            return str;
        }

        public virtual bool Remove(string keyword)
        {
            Bid.Trace("<comm.DbConnectionStringBuilder.Remove|API> %d#, keyword='%ls'\n", this.ObjectID, keyword);
            ADP.CheckArgumentNull(keyword, "keyword");
            if (this.CurrentValues.Remove(keyword))
            {
                this._connectionString = null;
                this._propertyDescriptors = null;
                return true;
            }
            return false;
        }

        public virtual bool ShouldSerialize(string keyword)
        {
            Bid.Trace("<comm.DbConnectionStringBuilder.ShouldSerialize|API> keyword='%ls'\n", keyword);
            ADP.CheckArgumentNull(keyword, "keyword");
            return this.CurrentValues.ContainsKey(keyword);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Bid.Trace("<comm.DbConnectionStringBuilder.ICollection.CopyTo|API> %d#\n", this.ObjectID);
            this.Collection.CopyTo(array, index);
        }

        void IDictionary.Add(object keyword, object value)
        {
            this.Add(this.ObjectToString(keyword), value);
        }

        bool IDictionary.Contains(object keyword)
        {
            return this.ContainsKey(this.ObjectToString(keyword));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            Bid.Trace("<comm.DbConnectionStringBuilder.IDictionary.GetEnumerator|API> %d#\n", this.ObjectID);
            return this.Dictionary.GetEnumerator();
        }

        void IDictionary.Remove(object keyword)
        {
            this.Remove(this.ObjectToString(keyword));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Bid.Trace("<comm.DbConnectionStringBuilder.IEnumerable.GetEnumerator|API> %d#\n", this.ObjectID);
            return this.Collection.GetEnumerator();
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.GetProperties(attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public override string ToString()
        {
            return this.ConnectionString;
        }

        public virtual bool TryGetValue(string keyword, out object value)
        {
            ADP.CheckArgumentNull(keyword, "keyword");
            return this.CurrentValues.TryGetValue(keyword, out value);
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DesignOnly(true), Browsable(false)]
        public bool BrowsableConnectionString
        {
            get
            {
                return this._browsableConnectionString;
            }
            set
            {
                this._browsableConnectionString = value;
                this._propertyDescriptors = null;
            }
        }

        private ICollection Collection
        {
            get
            {
                return this.CurrentValues;
            }
        }

        [ResDescription("DbConnectionString_ConnectionString"), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Data")]
        public string ConnectionString
        {
            get
            {
                Bid.Trace("<comm.DbConnectionStringBuilder.get_ConnectionString|API> %d#\n", this.ObjectID);
                string str = this._connectionString;
                if (str == null)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (string str2 in this.Keys)
                    {
                        object obj2;
                        if (this.ShouldSerialize(str2) && this.TryGetValue(str2, out obj2))
                        {
                            string str3 = (obj2 != null) ? Convert.ToString(obj2, CultureInfo.InvariantCulture) : null;
                            AppendKeyValuePair(builder, str2, str3, this.UseOdbcRules);
                        }
                    }
                    str = builder.ToString();
                    this._connectionString = str;
                }
                return str;
            }
            set
            {
                Bid.Trace("<comm.DbConnectionStringBuilder.set_ConnectionString|API> %d#\n", this.ObjectID);
                DbConnectionOptions options = new DbConnectionOptions(value, null, this.UseOdbcRules);
                string connectionString = this.ConnectionString;
                this.Clear();
                try
                {
                    for (NameValuePair pair = options.KeyChain; pair != null; pair = pair.Next)
                    {
                        if (pair.Value != null)
                        {
                            this[pair.Name] = pair.Value;
                        }
                        else
                        {
                            this.Remove(pair.Name);
                        }
                    }
                    this._connectionString = null;
                }
                catch (ArgumentException)
                {
                    this.ConnectionString = connectionString;
                    this._connectionString = connectionString;
                    throw;
                }
            }
        }

        [Browsable(false)]
        public virtual int Count
        {
            get
            {
                return this.CurrentValues.Count;
            }
        }

        private Dictionary<string, object> CurrentValues
        {
            get
            {
                Dictionary<string, object> dictionary = this._currentValues;
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    this._currentValues = dictionary;
                }
                return dictionary;
            }
        }

        private IDictionary Dictionary
        {
            get
            {
                return this.CurrentValues;
            }
        }

        [Browsable(false)]
        public virtual bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        [Browsable(false)]
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        [Browsable(false)]
        public virtual object this[string keyword]
        {
            get
            {
                object obj2;
                Bid.Trace("<comm.DbConnectionStringBuilder.get_Item|API> %d#, keyword='%ls'\n", this.ObjectID, keyword);
                ADP.CheckArgumentNull(keyword, "keyword");
                if (!this.CurrentValues.TryGetValue(keyword, out obj2))
                {
                    throw ADP.KeywordNotSupported(keyword);
                }
                return obj2;
            }
            set
            {
                ADP.CheckArgumentNull(keyword, "keyword");
                bool flag = false;
                if (value != null)
                {
                    string str = DbConnectionStringBuilderUtil.ConvertToString(value);
                    DbConnectionOptions.ValidateKeyValuePair(keyword, str);
                    flag = this.CurrentValues.ContainsKey(keyword);
                    this.CurrentValues[keyword] = str;
                }
                else
                {
                    flag = this.Remove(keyword);
                }
                this._connectionString = null;
                if (flag)
                {
                    this._propertyDescriptors = null;
                }
            }
        }

        [Browsable(false)]
        public virtual ICollection Keys
        {
            get
            {
                Bid.Trace("<comm.DbConnectionStringBuilder.Keys|API> %d#\n", this.ObjectID);
                return this.Dictionary.Keys;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.Collection.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.Collection.SyncRoot;
            }
        }

        object IDictionary.this[object keyword]
        {
            get
            {
                return this[this.ObjectToString(keyword)];
            }
            set
            {
                this[this.ObjectToString(keyword)] = value;
            }
        }

        [Browsable(false)]
        public virtual ICollection Values
        {
            get
            {
                Bid.Trace("<comm.DbConnectionStringBuilder.Values|API> %d#\n", this.ObjectID);
                ICollection<string> keys = (ICollection<string>) this.Keys;
                IEnumerator<string> enumerator = keys.GetEnumerator();
                object[] items = new object[keys.Count];
                for (int i = 0; i < items.Length; i++)
                {
                    enumerator.MoveNext();
                    items[i] = this[enumerator.Current];
                }
                return new ReadOnlyCollection<object>(items);
            }
        }
    }
}

