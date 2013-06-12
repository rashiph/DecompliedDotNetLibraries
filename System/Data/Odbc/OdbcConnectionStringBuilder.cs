namespace System.Data.Odbc
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

    [DefaultProperty("Driver"), TypeConverter(typeof(OdbcConnectionStringBuilder.OdbcConnectionStringBuilderConverter))]
    public sealed class OdbcConnectionStringBuilder : DbConnectionStringBuilder
    {
        private string _driver;
        private string _dsn;
        private static readonly Dictionary<string, Keywords> _keywords;
        private string[] _knownKeywords;
        private static readonly string[] _validKeywords;

        static OdbcConnectionStringBuilder()
        {
            string[] strArray = new string[2];
            strArray[1] = "Driver";
            strArray[0] = "Dsn";
            _validKeywords = strArray;
            Dictionary<string, Keywords> dictionary = new Dictionary<string, Keywords>(2, StringComparer.OrdinalIgnoreCase);
            dictionary.Add("Driver", Keywords.Driver);
            dictionary.Add("Dsn", Keywords.Dsn);
            _keywords = dictionary;
        }

        public OdbcConnectionStringBuilder() : this(null)
        {
        }

        public OdbcConnectionStringBuilder(string connectionString) : base(true)
        {
            this._dsn = "";
            this._driver = "";
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
            this._knownKeywords = _validKeywords;
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

        private static string ConvertToString(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToString(value);
        }

        private object GetAt(Keywords index)
        {
            switch (index)
            {
                case Keywords.Dsn:
                    return this.Dsn;

                case Keywords.Driver:
                    return this.Driver;
            }
            throw ADP.KeywordNotSupported(_validKeywords[(int) index]);
        }

        public override bool Remove(string keyword)
        {
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            if (!base.Remove(keyword))
            {
                return false;
            }
            if (_keywords.TryGetValue(keyword, out keywords))
            {
                this.Reset(keywords);
            }
            else
            {
                base.ClearPropertyDescriptors();
                this._knownKeywords = null;
            }
            return true;
        }

        private void Reset(Keywords index)
        {
            switch (index)
            {
                case Keywords.Dsn:
                    this._dsn = "";
                    return;

                case Keywords.Driver:
                    this._driver = "";
                    return;
            }
            throw ADP.KeywordNotSupported(_validKeywords[(int) index]);
        }

        private void SetValue(string keyword, string value)
        {
            ADP.CheckArgumentNull(value, keyword);
            base[keyword] = value;
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            if (_keywords.TryGetValue(keyword, out keywords))
            {
                value = this.GetAt(keywords);
                return true;
            }
            return base.TryGetValue(keyword, out value);
        }

        [ResCategory("DataCategory_Source"), RefreshProperties(RefreshProperties.All), DisplayName("Driver"), ResDescription("DbConnectionString_Driver")]
        public string Driver
        {
            get
            {
                return this._driver;
            }
            set
            {
                this.SetValue("Driver", value);
                this._driver = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_DSN"), DisplayName("Dsn"), ResCategory("DataCategory_NamedConnectionString")]
        public string Dsn
        {
            get
            {
                return this._dsn;
            }
            set
            {
                this.SetValue("Dsn", value);
                this._dsn = value;
            }
        }

        public override object this[string keyword]
        {
            get
            {
                Keywords keywords;
                Bid.Trace("<comm.OdbcConnectionStringBuilder.get_Item|API> keyword='%ls'\n", keyword);
                ADP.CheckArgumentNull(keyword, "keyword");
                if (_keywords.TryGetValue(keyword, out keywords))
                {
                    return this.GetAt(keywords);
                }
                return base[keyword];
            }
            set
            {
                Bid.Trace("<comm.OdbcConnectionStringBuilder.set_Item|API> keyword='%ls'\n", keyword);
                ADP.CheckArgumentNull(keyword, "keyword");
                if (value != null)
                {
                    Keywords keywords2;
                    if (_keywords.TryGetValue(keyword, out keywords2))
                    {
                        switch (keywords2)
                        {
                            case Keywords.Dsn:
                                this.Dsn = ConvertToString(value);
                                return;

                            case Keywords.Driver:
                                this.Driver = ConvertToString(value);
                                return;
                        }
                        throw ADP.KeywordNotSupported(keyword);
                    }
                    base[keyword] = value;
                    base.ClearPropertyDescriptors();
                    this._knownKeywords = null;
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
                string[] items = this._knownKeywords;
                if (items == null)
                {
                    items = _validKeywords;
                    int num3 = 0;
                    foreach (string str4 in base.Keys)
                    {
                        bool flag2 = true;
                        foreach (string str3 in items)
                        {
                            if (str3 == str4)
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
                        string[] array = new string[items.Length + num3];
                        items.CopyTo(array, 0);
                        int length = items.Length;
                        foreach (string str in base.Keys)
                        {
                            bool flag = true;
                            foreach (string str2 in items)
                            {
                                if (str2 == str)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                array[length++] = str;
                            }
                        }
                        items = array;
                    }
                    this._knownKeywords = items;
                }
                return new ReadOnlyCollection<string>(items);
            }
        }

        private enum Keywords
        {
            Dsn,
            Driver
        }

        internal sealed class OdbcConnectionStringBuilderConverter : ExpandableObjectConverter
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
                    OdbcConnectionStringBuilder options = value as OdbcConnectionStringBuilder;
                    if (options != null)
                    {
                        return this.ConvertToInstanceDescriptor(options);
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private InstanceDescriptor ConvertToInstanceDescriptor(OdbcConnectionStringBuilder options)
            {
                Type[] types = new Type[] { typeof(string) };
                return new InstanceDescriptor(typeof(OdbcConnectionStringBuilder).GetConstructor(types), new object[] { options.ConnectionString });
            }
        }
    }
}

