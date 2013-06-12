namespace System.Data.OleDb
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [DefaultProperty("Provider"), RefreshProperties(RefreshProperties.All), TypeConverter(typeof(OleDbConnectionStringBuilder.OleDbConnectionStringBuilderConverter))]
    public sealed class OleDbConnectionStringBuilder : DbConnectionStringBuilder
    {
        private string _dataSource;
        private string _fileName;
        private static readonly Dictionary<string, Keywords> _keywords;
        private string[] _knownKeywords;
        private int _oleDbServices;
        private bool _persistSecurityInfo;
        private Dictionary<string, OleDbPropertyInfo> _propertyInfo;
        private string _provider;
        private static readonly string[] _validKeywords;

        static OleDbConnectionStringBuilder()
        {
            string[] strArray = new string[5];
            strArray[2] = "Data Source";
            strArray[0] = "File Name";
            strArray[4] = "OLE DB Services";
            strArray[3] = "Persist Security Info";
            strArray[1] = "Provider";
            _validKeywords = strArray;
            Dictionary<string, Keywords> dictionary = new Dictionary<string, Keywords>(9, StringComparer.OrdinalIgnoreCase);
            dictionary.Add("Data Source", Keywords.DataSource);
            dictionary.Add("File Name", Keywords.FileName);
            dictionary.Add("OLE DB Services", Keywords.OleDbServices);
            dictionary.Add("Persist Security Info", Keywords.PersistSecurityInfo);
            dictionary.Add("Provider", Keywords.Provider);
            _keywords = dictionary;
        }

        public OleDbConnectionStringBuilder() : this(null)
        {
            this._knownKeywords = _validKeywords;
        }

        public OleDbConnectionStringBuilder(string connectionString)
        {
            this._fileName = "";
            this._dataSource = "";
            this._provider = "";
            this._oleDbServices = -13;
            if (!ADP.IsEmpty(connectionString))
            {
                base.ConnectionString = connectionString;
            }
        }

        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < _validKeywords.Length; i++)
            {
                this.Reset((Keywords) i);
            }
            base.ClearPropertyDescriptors();
            this._knownKeywords = _validKeywords;
        }

        private void ClearPropertyDescriptors()
        {
            base.ClearPropertyDescriptors();
            this._knownKeywords = null;
        }

        public override bool ContainsKey(string keyword)
        {
            ADP.CheckArgumentNull(keyword, "keyword");
            if (!_keywords.ContainsKey(keyword))
            {
                return base.ContainsKey(keyword);
            }
            return true;
        }

        private static bool ConvertToBoolean(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToBoolean(value);
        }

        private static int ConvertToInt32(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToInt32(value);
        }

        private static string ConvertToString(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToString(value);
        }

        private object GetAt(Keywords index)
        {
            switch (index)
            {
                case Keywords.FileName:
                    return this.FileName;

                case Keywords.Provider:
                    return this.Provider;

                case Keywords.DataSource:
                    return this.DataSource;

                case Keywords.PersistSecurityInfo:
                    return this.PersistSecurityInfo;

                case Keywords.OleDbServices:
                    return this.OleDbServices;
            }
            throw ADP.KeywordNotSupported(_validKeywords[(int) index]);
        }

        protected override void GetProperties(Hashtable propertyDescriptors)
        {
            Dictionary<string, OleDbPropertyInfo> providerInfo = this.GetProviderInfo(this.Provider);
            if (0 < providerInfo.Count)
            {
                foreach (OleDbPropertyInfo info in providerInfo.Values)
                {
                    Attribute[] attributeArray;
                    DbConnectionStringBuilderDescriptor descriptor;
                    Keywords keywords;
                    if (_keywords.TryGetValue(info._description, out keywords))
                    {
                        continue;
                    }
                    bool isReadOnly = false;
                    bool flag = false;
                    if (OleDbPropertySetGuid.DBInit == info._propertySet)
                    {
                        switch (info._propertyID)
                        {
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 10:
                                attributeArray = new Attribute[] { BrowsableAttribute.Yes, new ResCategoryAttribute("DataCategory_Security"), RefreshPropertiesAttribute.All };
                                flag = 7 == info._propertyID;
                                goto Label_0303;

                            case 9:
                                attributeArray = new Attribute[] { BrowsableAttribute.Yes, PasswordPropertyTextAttribute.Yes, new ResCategoryAttribute("DataCategory_Security"), RefreshPropertiesAttribute.All };
                                isReadOnly = this.ContainsKey("Integrated Security");
                                flag = true;
                                goto Label_0303;

                            case 12:
                                attributeArray = new Attribute[] { BrowsableAttribute.Yes, new ResCategoryAttribute("DataCategory_Security"), RefreshPropertiesAttribute.All };
                                isReadOnly = this.ContainsKey("Integrated Security");
                                flag = true;
                                goto Label_0303;

                            case 0x3d:
                            case 0x3f:
                            case 0x41:
                            case 160:
                            case 270:
                            case 0x10f:
                            case 0xba:
                                attributeArray = new Attribute[] { BrowsableAttribute.Yes, new ResCategoryAttribute("DataCategory_Advanced"), RefreshPropertiesAttribute.All };
                                goto Label_0303;

                            case 0x3e:
                            case 0xe9:
                                attributeArray = new Attribute[] { BrowsableAttribute.Yes, new ResCategoryAttribute("DataCategory_Source"), RefreshPropertiesAttribute.All };
                                goto Label_0303;

                            case 0x42:
                            case 0x11c:
                                attributeArray = new Attribute[] { BrowsableAttribute.Yes, new ResCategoryAttribute("DataCategory_Initialization"), RefreshPropertiesAttribute.All };
                                goto Label_0303;
                        }
                        attributeArray = new Attribute[] { BrowsableAttribute.Yes, RefreshPropertiesAttribute.All };
                    }
                    else if (info._description.EndsWith(" Provider", StringComparison.OrdinalIgnoreCase))
                    {
                        attributeArray = new Attribute[] { BrowsableAttribute.Yes, RefreshPropertiesAttribute.All, new ResCategoryAttribute("DataCategory_Source"), new TypeConverterAttribute(typeof(OleDbProviderConverter)) };
                        flag = true;
                    }
                    else
                    {
                        attributeArray = new Attribute[] { BrowsableAttribute.Yes, RefreshPropertiesAttribute.All, new CategoryAttribute(this.Provider) };
                    }
                Label_0303:
                    descriptor = new DbConnectionStringBuilderDescriptor(info._description, typeof(OleDbConnectionStringBuilder), info._type, isReadOnly, attributeArray);
                    descriptor.RefreshOnChange = flag;
                    propertyDescriptors[info._description] = descriptor;
                }
            }
            base.GetProperties(propertyDescriptors);
        }

        private Dictionary<string, OleDbPropertyInfo> GetProviderInfo(string provider)
        {
            Dictionary<string, OleDbPropertyInfo> dictionary = this._propertyInfo;
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, OleDbPropertyInfo>(StringComparer.OrdinalIgnoreCase);
                if (!ADP.IsEmpty(provider))
                {
                    Dictionary<string, OleDbPropertyInfo> propertyInfo = null;
                    try
                    {
                        StringBuilder builder = new StringBuilder();
                        DbConnectionStringBuilder.AppendKeyValuePair(builder, "Provider", provider);
                        OleDbConnectionString constr = new OleDbConnectionString(builder.ToString(), true);
                        constr.CreatePermissionSet().Demand();
                        using (OleDbConnectionInternal internal2 = new OleDbConnectionInternal(constr, null))
                        {
                            Guid[] propertySets = new Guid[] { OleDbPropertySetGuid.DBInitAll };
                            propertyInfo = internal2.GetPropertyInfo(propertySets);
                            foreach (KeyValuePair<string, OleDbPropertyInfo> pair3 in propertyInfo)
                            {
                                Keywords keywords;
                                OleDbPropertyInfo info2 = pair3.Value;
                                if (!_keywords.TryGetValue(info2._description, out keywords) && ((OleDbPropertySetGuid.DBInit != info2._propertySet) || (((200 != info2._propertyID) && (60 != info2._propertyID)) && (0x40 != info2._propertyID))))
                                {
                                    dictionary[info2._description] = info2;
                                }
                            }
                            List<Guid> list = new List<Guid>();
                            foreach (KeyValuePair<string, OleDbPropertyInfo> pair2 in propertyInfo)
                            {
                                OleDbPropertyInfo info3 = pair2.Value;
                                if (!list.Contains(info3._propertySet))
                                {
                                    list.Add(info3._propertySet);
                                }
                            }
                            Guid[] array = new Guid[list.Count];
                            list.CopyTo(array, 0);
                            using (PropertyIDSet set2 = new PropertyIDSet(array))
                            {
                                using (IDBPropertiesWrapper wrapper = internal2.IDBProperties())
                                {
                                    OleDbHResult result;
                                    using (DBPropSet set = new DBPropSet(wrapper.Value, set2, out result))
                                    {
                                        if (OleDbHResult.S_OK <= result)
                                        {
                                            int propertySetCount = set.PropertySetCount;
                                            for (int i = 0; i < propertySetCount; i++)
                                            {
                                                Guid guid;
                                                foreach (tagDBPROP gdbprop in set.GetPropertySet(i, out guid))
                                                {
                                                    foreach (KeyValuePair<string, OleDbPropertyInfo> pair in propertyInfo)
                                                    {
                                                        OleDbPropertyInfo info = pair.Value;
                                                        if ((info._propertyID == gdbprop.dwPropertyID) && (info._propertySet == guid))
                                                        {
                                                            info._defaultValue = gdbprop.vValue;
                                                            if (info._defaultValue == null)
                                                            {
                                                                if (typeof(string) == info._type)
                                                                {
                                                                    info._defaultValue = "";
                                                                }
                                                                else if (typeof(int) == info._type)
                                                                {
                                                                    info._defaultValue = 0;
                                                                }
                                                                else if (typeof(bool) == info._type)
                                                                {
                                                                    info._defaultValue = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException exception3)
                    {
                        ADP.TraceExceptionWithoutRethrow(exception3);
                    }
                    catch (OleDbException exception2)
                    {
                        ADP.TraceExceptionWithoutRethrow(exception2);
                    }
                    catch (SecurityException exception)
                    {
                        ADP.TraceExceptionWithoutRethrow(exception);
                    }
                }
                this._propertyInfo = dictionary;
            }
            return dictionary;
        }

        public override bool Remove(string keyword)
        {
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            bool flag = base.Remove(keyword);
            if (_keywords.TryGetValue(keyword, out keywords))
            {
                this.Reset(keywords);
                return flag;
            }
            if (flag)
            {
                this.ClearPropertyDescriptors();
            }
            return flag;
        }

        private void Reset(Keywords index)
        {
            switch (index)
            {
                case Keywords.FileName:
                    this._fileName = "";
                    this.RestartProvider();
                    return;

                case Keywords.Provider:
                    this._provider = "";
                    this.RestartProvider();
                    return;

                case Keywords.DataSource:
                    this._dataSource = "";
                    return;

                case Keywords.PersistSecurityInfo:
                    this._persistSecurityInfo = false;
                    return;

                case Keywords.OleDbServices:
                    this._oleDbServices = -13;
                    return;
            }
            throw ADP.KeywordNotSupported(_validKeywords[(int) index]);
        }

        private void RestartProvider()
        {
            this.ClearPropertyDescriptors();
            this._propertyInfo = null;
        }

        private void SetValue(string keyword, bool value)
        {
            base[keyword] = value.ToString(null);
        }

        private void SetValue(string keyword, int value)
        {
            base[keyword] = value.ToString((IFormatProvider) null);
        }

        private void SetValue(string keyword, string value)
        {
            ADP.CheckArgumentNull(value, keyword);
            base[keyword] = value;
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            OleDbPropertyInfo info;
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            if (_keywords.TryGetValue(keyword, out keywords))
            {
                value = this.GetAt(keywords);
                return true;
            }
            if (base.TryGetValue(keyword, out value))
            {
                return true;
            }
            if (this.GetProviderInfo(this.Provider).TryGetValue(keyword, out info))
            {
                value = info._defaultValue;
                return true;
            }
            return false;
        }

        [ResCategory("DataCategory_Source"), ResDescription("DbConnectionString_DataSource"), DisplayName("Data Source"), RefreshProperties(RefreshProperties.All)]
        public string DataSource
        {
            get
            {
                return this._dataSource;
            }
            set
            {
                this.SetValue("Data Source", value);
                this._dataSource = value;
            }
        }

        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResDescription("DbConnectionString_FileName"), ResCategory("DataCategory_NamedConnectionString"), DisplayName("File Name"), RefreshProperties(RefreshProperties.All)]
        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this.SetValue("File Name", value);
                this._fileName = value;
            }
        }

        public override object this[string keyword]
        {
            get
            {
                object obj2;
                Keywords keywords;
                Bid.Trace("<comm.OleDbConnectionStringBuilder.get_Item|API> keyword='%ls'\n", keyword);
                ADP.CheckArgumentNull(keyword, "keyword");
                if (_keywords.TryGetValue(keyword, out keywords))
                {
                    return this.GetAt(keywords);
                }
                if (!base.TryGetValue(keyword, out obj2))
                {
                    OleDbPropertyInfo info = this.GetProviderInfo(this.Provider)[keyword];
                    obj2 = info._defaultValue;
                }
                return obj2;
            }
            set
            {
                Bid.Trace("<comm.OleDbConnectionStringBuilder.set_Item|API> keyword='%ls'\n", keyword);
                if (value != null)
                {
                    Keywords keywords2;
                    ADP.CheckArgumentNull(keyword, "keyword");
                    if (_keywords.TryGetValue(keyword, out keywords2))
                    {
                        switch (keywords2)
                        {
                            case Keywords.FileName:
                                this.FileName = ConvertToString(value);
                                return;

                            case Keywords.Provider:
                                this.Provider = ConvertToString(value);
                                return;

                            case Keywords.DataSource:
                                this.DataSource = ConvertToString(value);
                                return;

                            case Keywords.PersistSecurityInfo:
                                this.PersistSecurityInfo = ConvertToBoolean(value);
                                return;

                            case Keywords.OleDbServices:
                                this.OleDbServices = ConvertToInt32(value);
                                return;
                        }
                        throw ADP.KeywordNotSupported(keyword);
                    }
                    base[keyword] = value;
                    this.ClearPropertyDescriptors();
                }
                else
                {
                    this.Remove(keyword);
                }
            }
        }

        public override ICollection Keys
        {
            get
            {
                string[] array = this._knownKeywords;
                if (array == null)
                {
                    Dictionary<string, OleDbPropertyInfo> providerInfo = this.GetProviderInfo(this.Provider);
                    if (0 < providerInfo.Count)
                    {
                        array = new string[_validKeywords.Length + providerInfo.Count];
                        _validKeywords.CopyTo(array, 0);
                        providerInfo.Keys.CopyTo(array, _validKeywords.Length);
                    }
                    else
                    {
                        array = _validKeywords;
                    }
                    int num3 = 0;
                    foreach (string str4 in base.Keys)
                    {
                        bool flag2 = true;
                        foreach (string str3 in array)
                        {
                            if (StringComparer.OrdinalIgnoreCase.Equals(str3, str4))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            num3++;
                        }
                    }
                    if (0 < num3)
                    {
                        string[] strArray2 = new string[array.Length + num3];
                        array.CopyTo(strArray2, 0);
                        int length = array.Length;
                        foreach (string str in base.Keys)
                        {
                            bool flag = true;
                            foreach (string str2 in array)
                            {
                                if (StringComparer.OrdinalIgnoreCase.Equals(str2, str))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                strArray2[length++] = str;
                            }
                        }
                        array = strArray2;
                    }
                    this._knownKeywords = array;
                }
                return new ReadOnlyCollection<string>(array);
            }
        }

        [ResDescription("DbConnectionString_OleDbServices"), ResCategory("DataCategory_Pooling"), TypeConverter(typeof(OleDbServicesConverter)), RefreshProperties(RefreshProperties.All), DisplayName("OLE DB Services")]
        public int OleDbServices
        {
            get
            {
                return this._oleDbServices;
            }
            set
            {
                this.SetValue("OLE DB Services", value);
                this._oleDbServices = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Security"), ResDescription("DbConnectionString_PersistSecurityInfo"), DisplayName("Persist Security Info")]
        public bool PersistSecurityInfo
        {
            get
            {
                return this._persistSecurityInfo;
            }
            set
            {
                this.SetValue("Persist Security Info", value);
                this._persistSecurityInfo = value;
            }
        }

        [TypeConverter(typeof(OleDbProviderConverter)), DisplayName("Provider"), ResCategory("DataCategory_Source"), ResDescription("DbConnectionString_Provider"), RefreshProperties(RefreshProperties.All)]
        public string Provider
        {
            get
            {
                return this._provider;
            }
            set
            {
                this.SetValue("Provider", value);
                this._provider = value;
                this.RestartProvider();
            }
        }

        private enum Keywords
        {
            FileName,
            Provider,
            DataSource,
            PersistSecurityInfo,
            OleDbServices
        }

        internal sealed class OleDbConnectionStringBuilderConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if (typeof(InstanceDescriptor) == destinationType)
                {
                    OleDbConnectionStringBuilder options = value as OleDbConnectionStringBuilder;
                    if (options != null)
                    {
                        return this.ConvertToInstanceDescriptor(options);
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private InstanceDescriptor ConvertToInstanceDescriptor(OleDbConnectionStringBuilder options)
            {
                Type[] types = new Type[] { typeof(string) };
                return new InstanceDescriptor(typeof(OleDbConnectionStringBuilder).GetConstructor(types), new object[] { options.ConnectionString });
            }
        }

        private sealed class OleDbProviderConverter : StringConverter
        {
            private TypeConverter.StandardValuesCollection _standardValues;
            private const int DBSOURCETYPE_DATASOURCE_MDP = 3;
            private const int DBSOURCETYPE_DATASOURCE_TDP = 1;

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                TypeConverter.StandardValuesCollection valuess = this._standardValues;
                if (this._standardValues == null)
                {
                    DataTable elements = new OleDbEnumerator().GetElements();
                    DataColumn column2 = elements.Columns["SOURCES_NAME"];
                    DataColumn column = elements.Columns["SOURCES_TYPE"];
                    List<string> values = new List<string>(elements.Rows.Count);
                    foreach (DataRow row in elements.Rows)
                    {
                        int num = (int) row[column];
                        if ((1 == num) || (3 == num))
                        {
                            string item = (string) row[column2];
                            if (!OleDbConnectionString.IsMSDASQL(item.ToLower(CultureInfo.InvariantCulture)) && (0 > values.IndexOf(item)))
                            {
                                values.Add(item);
                            }
                        }
                    }
                    valuess = new TypeConverter.StandardValuesCollection(values);
                    this._standardValues = valuess;
                }
                return valuess;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        internal sealed class OleDbServicesConverter : TypeConverter
        {
            private TypeConverter.StandardValuesCollection _standardValues;

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (!(typeof(string) == sourceType))
                {
                    return base.CanConvertFrom(context, sourceType);
                }
                return true;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (!(typeof(string) == destinationType))
                {
                    return base.CanConvertTo(context, destinationType);
                }
                return true;
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                int num3;
                string s = value as string;
                if (s == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                if (int.TryParse(s, out num3))
                {
                    return num3;
                }
                if (s.IndexOf(',') == -1)
                {
                    return (int) ((OleDbConnectionStringBuilder.OleDbServiceValues) Enum.Parse(typeof(OleDbConnectionStringBuilder.OleDbServiceValues), s, true));
                }
                int num2 = 0;
                foreach (string str2 in s.Split(new char[] { ',' }))
                {
                    num2 |= (OleDbConnectionStringBuilder.OleDbServiceValues) Enum.Parse(typeof(OleDbConnectionStringBuilder.OleDbServiceValues), str2, true);
                }
                return num2;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (((typeof(string) == destinationType) && (value != null)) && (typeof(int) == value.GetType()))
                {
                    return Enum.Format(typeof(OleDbConnectionStringBuilder.OleDbServiceValues), (OleDbConnectionStringBuilder.OleDbServiceValues) ((int) value), "G");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                TypeConverter.StandardValuesCollection valuess = this._standardValues;
                if (valuess == null)
                {
                    Array values = Enum.GetValues(typeof(OleDbConnectionStringBuilder.OleDbServiceValues));
                    Array.Sort(values, 0, values.Length);
                    valuess = new TypeConverter.StandardValuesCollection(values);
                    this._standardValues = valuess;
                }
                return valuess;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool IsValid(ITypeDescriptorContext context, object value)
            {
                return true;
            }
        }

        [Flags]
        internal enum OleDbServiceValues
        {
            AggregationAfterSession = 8,
            ClientCursor = 4,
            Default = -13,
            DisableAll = 0,
            EnableAll = -1,
            ResourcePooling = 1,
            TransactionEnlistment = 2
        }
    }
}

