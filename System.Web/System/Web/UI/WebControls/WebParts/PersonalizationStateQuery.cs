namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    [Serializable]
    public sealed class PersonalizationStateQuery
    {
        private HybridDictionary _data = new HybridDictionary(true);
        private static readonly Dictionary<string, Type> _knownPropertyTypeMappings = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        static PersonalizationStateQuery()
        {
            _knownPropertyTypeMappings["PathToMatch"] = typeof(string);
            _knownPropertyTypeMappings["UserInactiveSinceDate"] = typeof(DateTime);
            _knownPropertyTypeMappings["UsernameToMatch"] = typeof(string);
        }

        public PersonalizationStateQuery()
        {
            this._data["UserInactiveSinceDate"] = PersonalizationAdministration.DefaultInactiveSinceDate;
        }

        public object this[string queryKey]
        {
            get
            {
                queryKey = StringUtil.CheckAndTrimString(queryKey, "queryKey");
                return this._data[queryKey];
            }
            set
            {
                queryKey = StringUtil.CheckAndTrimString(queryKey, "queryKey");
                if (_knownPropertyTypeMappings.ContainsKey(queryKey))
                {
                    Type type = _knownPropertyTypeMappings[queryKey];
                    if (((value == null) && type.IsValueType) || ((value != null) && !type.IsAssignableFrom(value.GetType())))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("PersonalizationStateQuery_IncorrectValueType", new object[] { queryKey, type.FullName }));
                    }
                }
                this._data[queryKey] = value;
            }
        }

        public string PathToMatch
        {
            get
            {
                return (string) this["PathToMatch"];
            }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                }
                this._data["PathToMatch"] = value;
            }
        }

        public DateTime UserInactiveSinceDate
        {
            get
            {
                object obj2 = this["UserInactiveSinceDate"];
                return (DateTime) obj2;
            }
            set
            {
                this._data["UserInactiveSinceDate"] = value;
            }
        }

        public string UsernameToMatch
        {
            get
            {
                return (string) this["UsernameToMatch"];
            }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                }
                this._data["UsernameToMatch"] = value;
            }
        }
    }
}

