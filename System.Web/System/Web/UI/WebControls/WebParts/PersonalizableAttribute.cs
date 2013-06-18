namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.UI;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PersonalizableAttribute : Attribute
    {
        private bool _isPersonalizable;
        private bool _isSensitive;
        private PersonalizationScope _scope;
        public static readonly PersonalizableAttribute Default = NotPersonalizable;
        public static readonly PersonalizableAttribute NotPersonalizable = new PersonalizableAttribute(false);
        public static readonly PersonalizableAttribute Personalizable = new PersonalizableAttribute(true);
        internal static readonly Type PersonalizableAttributeType = typeof(PersonalizableAttribute);
        private static readonly IDictionary PersonalizableTypeTable = Hashtable.Synchronized(new Hashtable());
        public static readonly PersonalizableAttribute SharedPersonalizable = new PersonalizableAttribute(PersonalizationScope.Shared);
        public static readonly PersonalizableAttribute UserPersonalizable = new PersonalizableAttribute(PersonalizationScope.User);

        public PersonalizableAttribute() : this(true, PersonalizationScope.User, false)
        {
        }

        public PersonalizableAttribute(bool isPersonalizable) : this(isPersonalizable, PersonalizationScope.User, false)
        {
        }

        public PersonalizableAttribute(PersonalizationScope scope) : this(true, scope, false)
        {
        }

        public PersonalizableAttribute(PersonalizationScope scope, bool isSensitive) : this(true, scope, isSensitive)
        {
        }

        private PersonalizableAttribute(bool isPersonalizable, PersonalizationScope scope, bool isSensitive)
        {
            this._isPersonalizable = isPersonalizable;
            this._isSensitive = isSensitive;
            if (this._isPersonalizable)
            {
                this._scope = scope;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            PersonalizableAttribute attribute = obj as PersonalizableAttribute;
            if (attribute == null)
            {
                return false;
            }
            return (((attribute.IsPersonalizable == this.IsPersonalizable) && (attribute.Scope == this.Scope)) && (attribute.IsSensitive == this.IsSensitive));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this._isPersonalizable.GetHashCode(), this._scope.GetHashCode(), this._isSensitive.GetHashCode());
        }

        public static ICollection GetPersonalizableProperties(Type type)
        {
            PersonalizableTypeEntry entry = (PersonalizableTypeEntry) PersonalizableTypeTable[type];
            if (entry == null)
            {
                entry = new PersonalizableTypeEntry(type);
                PersonalizableTypeTable[type] = entry;
            }
            return entry.PropertyInfos;
        }

        internal static IDictionary GetPersonalizablePropertyEntries(Type type)
        {
            PersonalizableTypeEntry entry = (PersonalizableTypeEntry) PersonalizableTypeTable[type];
            if (entry == null)
            {
                entry = new PersonalizableTypeEntry(type);
                PersonalizableTypeTable[type] = entry;
            }
            return entry.PropertyEntries;
        }

        internal static IDictionary GetPersonalizablePropertyValues(Control control, PersonalizationScope scope, bool excludeSensitive)
        {
            IDictionary dictionary = null;
            IDictionary personalizablePropertyEntries = GetPersonalizablePropertyEntries(control.GetType());
            if (personalizablePropertyEntries.Count != 0)
            {
                foreach (DictionaryEntry entry in personalizablePropertyEntries)
                {
                    string key = (string) entry.Key;
                    PersonalizablePropertyEntry entry2 = (PersonalizablePropertyEntry) entry.Value;
                    if ((!excludeSensitive || !entry2.IsSensitive) && ((scope != PersonalizationScope.User) || (entry2.Scope != PersonalizationScope.Shared)))
                    {
                        if (dictionary == null)
                        {
                            dictionary = new HybridDictionary(personalizablePropertyEntries.Count, false);
                        }
                        object y = FastPropertyAccessor.GetProperty(control, key, control.DesignMode);
                        dictionary[key] = new Pair(entry2.PropertyInfo, y);
                    }
                }
            }
            if (dictionary == null)
            {
                dictionary = new HybridDictionary(false);
            }
            return dictionary;
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public override bool Match(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            PersonalizableAttribute attribute = obj as PersonalizableAttribute;
            return ((attribute != null) && (attribute.IsPersonalizable == this.IsPersonalizable));
        }

        public bool IsPersonalizable
        {
            get
            {
                return this._isPersonalizable;
            }
        }

        public bool IsSensitive
        {
            get
            {
                return this._isSensitive;
            }
        }

        public PersonalizationScope Scope
        {
            get
            {
                return this._scope;
            }
        }
    }
}

