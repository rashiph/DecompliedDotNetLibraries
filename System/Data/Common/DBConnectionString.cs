namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;

    [Serializable]
    internal sealed class DBConnectionString
    {
        private readonly KeyRestrictionBehavior _behavior;
        private readonly string _encryptedActualConnectionString;
        private readonly string _encryptedUsersConnectionString;
        private readonly bool _hasPassword;
        private readonly NameValuePair _keychain;
        private readonly Hashtable _parsetable;
        private readonly string _restrictions;
        private readonly string[] _restrictionValues;

        internal DBConnectionString(DbConnectionOptions connectionOptions) : this(connectionOptions, null, KeyRestrictionBehavior.AllowOnly, null, true)
        {
        }

        private DBConnectionString(DBConnectionString connectionString, string[] restrictionValues, KeyRestrictionBehavior behavior)
        {
            this._encryptedUsersConnectionString = connectionString._encryptedUsersConnectionString;
            this._parsetable = connectionString._parsetable;
            this._keychain = connectionString._keychain;
            this._hasPassword = connectionString._hasPassword;
            this._restrictionValues = restrictionValues;
            this._restrictions = null;
            this._behavior = behavior;
        }

        private DBConnectionString(DbConnectionOptions connectionOptions, string restrictions, KeyRestrictionBehavior behavior, Hashtable synonyms, bool mustCloneDictionary)
        {
            switch (behavior)
            {
                case KeyRestrictionBehavior.AllowOnly:
                case KeyRestrictionBehavior.PreventUsage:
                    this._behavior = behavior;
                    this._encryptedUsersConnectionString = connectionOptions.UsersConnectionString(false);
                    this._hasPassword = connectionOptions.HasPasswordKeyword;
                    this._parsetable = connectionOptions.Parsetable;
                    this._keychain = connectionOptions.KeyChain;
                    if (this._hasPassword && !connectionOptions.HasPersistablePassword)
                    {
                        if (mustCloneDictionary)
                        {
                            this._parsetable = (Hashtable) this._parsetable.Clone();
                        }
                        if (this._parsetable.ContainsKey("password"))
                        {
                            this._parsetable["password"] = "*";
                        }
                        if (this._parsetable.ContainsKey("pwd"))
                        {
                            this._parsetable["pwd"] = "*";
                        }
                        this._keychain = connectionOptions.ReplacePasswordPwd(out this._encryptedUsersConnectionString, true);
                    }
                    if (!ADP.IsEmpty(restrictions))
                    {
                        this._restrictionValues = ParseRestrictions(restrictions, synonyms);
                        this._restrictions = restrictions;
                    }
                    return;
            }
            throw ADP.InvalidKeyRestrictionBehavior(behavior);
        }

        internal DBConnectionString(string value, string restrictions, KeyRestrictionBehavior behavior, Hashtable synonyms, bool useOdbcRules) : this(new DbConnectionOptions(value, synonyms, useOdbcRules), restrictions, behavior, synonyms, false)
        {
        }

        internal bool ContainsKey(string keyword)
        {
            return this._parsetable.ContainsKey(keyword);
        }

        internal DBConnectionString Intersect(DBConnectionString entry)
        {
            KeyRestrictionBehavior allowOnly = this._behavior;
            string[] restrictionValues = null;
            if (entry == null)
            {
                allowOnly = KeyRestrictionBehavior.AllowOnly;
            }
            else if (this._behavior != entry._behavior)
            {
                allowOnly = KeyRestrictionBehavior.AllowOnly;
                if (entry._behavior == KeyRestrictionBehavior.AllowOnly)
                {
                    if (!ADP.IsEmptyArray(this._restrictionValues))
                    {
                        if (!ADP.IsEmptyArray(entry._restrictionValues))
                        {
                            restrictionValues = NewRestrictionAllowOnly(entry._restrictionValues, this._restrictionValues);
                        }
                    }
                    else
                    {
                        restrictionValues = entry._restrictionValues;
                    }
                }
                else if (!ADP.IsEmptyArray(this._restrictionValues))
                {
                    if (!ADP.IsEmptyArray(entry._restrictionValues))
                    {
                        restrictionValues = NewRestrictionAllowOnly(this._restrictionValues, entry._restrictionValues);
                    }
                    else
                    {
                        restrictionValues = this._restrictionValues;
                    }
                }
            }
            else if (KeyRestrictionBehavior.PreventUsage == this._behavior)
            {
                if (ADP.IsEmptyArray(this._restrictionValues))
                {
                    restrictionValues = entry._restrictionValues;
                }
                else if (ADP.IsEmptyArray(entry._restrictionValues))
                {
                    restrictionValues = this._restrictionValues;
                }
                else
                {
                    restrictionValues = NoDuplicateUnion(this._restrictionValues, entry._restrictionValues);
                }
            }
            else if (!ADP.IsEmptyArray(this._restrictionValues) && !ADP.IsEmptyArray(entry._restrictionValues))
            {
                if (this._restrictionValues.Length <= entry._restrictionValues.Length)
                {
                    restrictionValues = NewRestrictionIntersect(this._restrictionValues, entry._restrictionValues);
                }
                else
                {
                    restrictionValues = NewRestrictionIntersect(entry._restrictionValues, this._restrictionValues);
                }
            }
            return new DBConnectionString(this, restrictionValues, allowOnly);
        }

        private bool IsRestrictedKeyword(string key)
        {
            if (this._restrictionValues != null)
            {
                return (0 > Array.BinarySearch<string>(this._restrictionValues, key, StringComparer.Ordinal));
            }
            return true;
        }

        internal bool IsSupersetOf(DBConnectionString entry)
        {
            switch (this._behavior)
            {
                case KeyRestrictionBehavior.AllowOnly:
                    for (NameValuePair pair = entry.KeyChain; pair != null; pair = pair.Next)
                    {
                        if (!this.ContainsKey(pair.Name) && this.IsRestrictedKeyword(pair.Name))
                        {
                            return false;
                        }
                    }
                    break;

                case KeyRestrictionBehavior.PreventUsage:
                    if (this._restrictionValues != null)
                    {
                        foreach (string str in this._restrictionValues)
                        {
                            if (entry.ContainsKey(str))
                            {
                                return false;
                            }
                        }
                    }
                    break;

                default:
                    throw ADP.InvalidKeyRestrictionBehavior(this._behavior);
            }
            return true;
        }

        private static string[] NewRestrictionAllowOnly(string[] allowonly, string[] preventusage)
        {
            List<string> list = null;
            for (int i = 0; i < allowonly.Length; i++)
            {
                if (0 > Array.BinarySearch<string>(preventusage, allowonly[i], StringComparer.Ordinal))
                {
                    if (list == null)
                    {
                        list = new List<string>();
                    }
                    list.Add(allowonly[i]);
                }
            }
            string[] strArray = null;
            if (list != null)
            {
                strArray = list.ToArray();
            }
            return strArray;
        }

        private static string[] NewRestrictionIntersect(string[] a, string[] b)
        {
            List<string> list = null;
            for (int i = 0; i < a.Length; i++)
            {
                if (0 <= Array.BinarySearch<string>(b, a[i], StringComparer.Ordinal))
                {
                    if (list == null)
                    {
                        list = new List<string>();
                    }
                    list.Add(a[i]);
                }
            }
            string[] strArray = null;
            if (list != null)
            {
                strArray = list.ToArray();
            }
            return strArray;
        }

        private static string[] NoDuplicateUnion(string[] a, string[] b)
        {
            List<string> list = new List<string>(a.Length + b.Length);
            for (int i = 0; i < a.Length; i++)
            {
                list.Add(a[i]);
            }
            for (int j = 0; j < b.Length; j++)
            {
                if (0 > Array.BinarySearch<string>(a, b[j], StringComparer.Ordinal))
                {
                    list.Add(b[j]);
                }
            }
            string[] array = list.ToArray();
            Array.Sort<string>(array, StringComparer.Ordinal);
            return array;
        }

        private static string[] ParseRestrictions(string restrictions, Hashtable synonyms)
        {
            List<string> list = new List<string>();
            StringBuilder buffer = new StringBuilder(restrictions.Length);
            int num = 0;
            int length = restrictions.Length;
            while (num < length)
            {
                string str;
                string str3;
                int currentPosition = num;
                num = DbConnectionOptions.GetKeyValuePair(restrictions, currentPosition, buffer, false, out str, out str3);
                if (!ADP.IsEmpty(str))
                {
                    string str2 = (synonyms != null) ? ((string) synonyms[str]) : str;
                    if (ADP.IsEmpty(str2))
                    {
                        throw ADP.KeywordNotSupported(str);
                    }
                    list.Add(str2);
                }
            }
            return RemoveDuplicates(list.ToArray());
        }

        internal static string[] RemoveDuplicates(string[] restrictions)
        {
            int length = restrictions.Length;
            if (0 < length)
            {
                Array.Sort<string>(restrictions, StringComparer.Ordinal);
                for (int i = 1; i < restrictions.Length; i++)
                {
                    string str = restrictions[i - 1];
                    if ((str.Length == 0) || (str == restrictions[i]))
                    {
                        restrictions[i - 1] = null;
                        length--;
                    }
                }
                if (restrictions[restrictions.Length - 1].Length == 0)
                {
                    restrictions[restrictions.Length - 1] = null;
                    length--;
                }
                if (length == restrictions.Length)
                {
                    return restrictions;
                }
                string[] strArray = new string[length];
                length = 0;
                for (int j = 0; j < restrictions.Length; j++)
                {
                    if (restrictions[j] != null)
                    {
                        strArray[length++] = restrictions[j];
                    }
                }
                restrictions = strArray;
            }
            return restrictions;
        }

        [Conditional("DEBUG")]
        private static void Verify(string[] restrictionValues)
        {
            if (restrictionValues != null)
            {
                for (int i = 1; i < restrictionValues.Length; i++)
                {
                }
            }
        }

        internal KeyRestrictionBehavior Behavior
        {
            get
            {
                return this._behavior;
            }
        }

        internal string ConnectionString
        {
            get
            {
                return this._encryptedUsersConnectionString;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (null == this._keychain);
            }
        }

        internal string this[string keyword]
        {
            get
            {
                return (string) this._parsetable[keyword];
            }
        }

        internal NameValuePair KeyChain
        {
            get
            {
                return this._keychain;
            }
        }

        internal string Restrictions
        {
            get
            {
                string str = this._restrictions;
                if (str == null)
                {
                    string[] strArray = this._restrictionValues;
                    if ((strArray != null) && (0 < strArray.Length))
                    {
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            if (!ADP.IsEmpty(strArray[i]))
                            {
                                builder.Append(strArray[i]);
                                builder.Append("=;");
                            }
                        }
                        str = builder.ToString();
                    }
                }
                if (str == null)
                {
                    return "";
                }
                return str;
            }
        }
    }
}

