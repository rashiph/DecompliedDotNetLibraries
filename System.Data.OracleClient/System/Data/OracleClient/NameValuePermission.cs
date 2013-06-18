namespace System.Data.OracleClient
{
    using System;
    using System.Collections;

    [Serializable]
    internal sealed class NameValuePermission : IComparable
    {
        private DBConnectionString _entry;
        private NameValuePermission[] _tree;
        private string _value;
        internal static readonly NameValuePermission Default;

        internal NameValuePermission()
        {
        }

        private NameValuePermission(NameValuePermission permit)
        {
            this._value = permit._value;
            this._entry = permit._entry;
            this._tree = permit._tree;
            if (this._tree != null)
            {
                NameValuePermission[] permissionArray = this._tree.Clone() as NameValuePermission[];
                for (int i = 0; i < permissionArray.Length; i++)
                {
                    if (permissionArray[i] != null)
                    {
                        permissionArray[i] = permissionArray[i].CopyNameValue();
                    }
                }
                this._tree = permissionArray;
            }
        }

        private NameValuePermission(string keyword)
        {
            this._value = keyword;
        }

        private NameValuePermission(string value, DBConnectionString entry)
        {
            this._value = value;
            this._entry = entry;
        }

        private void Add(NameValuePermission permit)
        {
            NameValuePermission[] permissionArray2 = this._tree;
            int index = (permissionArray2 != null) ? permissionArray2.Length : 0;
            NameValuePermission[] array = new NameValuePermission[1 + index];
            for (int i = 0; i < (array.Length - 1); i++)
            {
                array[i] = permissionArray2[i];
            }
            array[index] = permit;
            Array.Sort<NameValuePermission>(array);
            this._tree = array;
        }

        internal static void AddEntry(NameValuePermission kvtree, ArrayList entries, DBConnectionString entry)
        {
            if (entry.KeyChain != null)
            {
                for (NameValuePair pair = entry.KeyChain; pair != null; pair = pair.Next)
                {
                    NameValuePermission permit = kvtree.CheckKeyForValue(pair.Name);
                    if (permit == null)
                    {
                        permit = new NameValuePermission(pair.Name);
                        kvtree.Add(permit);
                    }
                    kvtree = permit;
                    permit = kvtree.CheckKeyForValue(pair.Value);
                    if (permit == null)
                    {
                        DBConnectionString str2 = (pair.Next != null) ? null : entry;
                        permit = new NameValuePermission(pair.Value, str2);
                        kvtree.Add(permit);
                        if (str2 != null)
                        {
                            entries.Add(str2);
                        }
                    }
                    else if (pair.Next == null)
                    {
                        if (permit._entry != null)
                        {
                            entries.Remove(permit._entry);
                            permit._entry = permit._entry.Intersect(entry);
                        }
                        else
                        {
                            permit._entry = entry;
                        }
                        entries.Add(permit._entry);
                    }
                    kvtree = permit;
                }
            }
            else
            {
                DBConnectionString str = kvtree._entry;
                if (str != null)
                {
                    entries.Remove(str);
                    kvtree._entry = str.Intersect(entry);
                }
                else
                {
                    kvtree._entry = entry;
                }
                entries.Add(kvtree._entry);
            }
        }

        private NameValuePermission CheckKeyForValue(string keyInQuestion)
        {
            NameValuePermission[] permissionArray = this._tree;
            if (permissionArray != null)
            {
                for (int i = 0; i < permissionArray.Length; i++)
                {
                    NameValuePermission permission = permissionArray[i];
                    if (string.Equals(keyInQuestion, permission._value, StringComparison.OrdinalIgnoreCase))
                    {
                        return permission;
                    }
                }
            }
            return null;
        }

        internal bool CheckValueForKeyPermit(DBConnectionString parsetable)
        {
            if (parsetable == null)
            {
                return false;
            }
            bool isEmpty = false;
            NameValuePermission[] permissionArray = this._tree;
            if (permissionArray != null)
            {
                isEmpty = parsetable.IsEmpty;
                if (!isEmpty)
                {
                    for (int i = 0; i < permissionArray.Length; i++)
                    {
                        NameValuePermission permission = permissionArray[i];
                        if (permission != null)
                        {
                            string keyword = permission._value;
                            if (parsetable.ContainsKey(keyword))
                            {
                                string keyInQuestion = parsetable[keyword];
                                NameValuePermission permission2 = permission.CheckKeyForValue(keyInQuestion);
                                if ((permission2 != null) && permission2.CheckValueForKeyPermit(parsetable))
                                {
                                    isEmpty = true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            DBConnectionString str = this._entry;
            if (str != null)
            {
                isEmpty = str.IsSupersetOf(parsetable);
            }
            return isEmpty;
        }

        internal NameValuePermission CopyNameValue()
        {
            return new NameValuePermission(this);
        }

        internal void Intersect(ArrayList entries, NameValuePermission target)
        {
            if (target == null)
            {
                this._tree = null;
                this._entry = null;
            }
            else
            {
                if (this._entry != null)
                {
                    entries.Remove(this._entry);
                    this._entry = this._entry.Intersect(target._entry);
                    entries.Add(this._entry);
                }
                else if (target._entry != null)
                {
                    this._entry = target._entry.Intersect(null);
                    entries.Add(this._entry);
                }
                if (this._tree != null)
                {
                    int length = this._tree.Length;
                    for (int i = 0; i < this._tree.Length; i++)
                    {
                        NameValuePermission permission = target.CheckKeyForValue(this._tree[i]._value);
                        if (permission != null)
                        {
                            this._tree[i].Intersect(entries, permission);
                        }
                        else
                        {
                            this._tree[i] = null;
                            length--;
                        }
                    }
                    if (length == 0)
                    {
                        this._tree = null;
                    }
                    else if (length < this._tree.Length)
                    {
                        NameValuePermission[] permissionArray = new NameValuePermission[length];
                        int index = 0;
                        int num4 = 0;
                        while (index < this._tree.Length)
                        {
                            if (this._tree[index] != null)
                            {
                                permissionArray[num4++] = this._tree[index];
                            }
                            index++;
                        }
                        this._tree = permissionArray;
                    }
                }
            }
        }

        int IComparable.CompareTo(object a)
        {
            return StringComparer.Ordinal.Compare(this._value, ((NameValuePermission) a)._value);
        }
    }
}

