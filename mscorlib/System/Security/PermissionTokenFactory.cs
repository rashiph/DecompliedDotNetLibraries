namespace System.Security
{
    using System;
    using System.Collections;

    internal class PermissionTokenFactory
    {
        private PermissionToken[] m_builtIn = new PermissionToken[0x11];
        private Hashtable m_handleTable;
        private int m_index;
        private Hashtable m_indexTable;
        private int m_size;
        private Hashtable m_tokenTable;
        private const string s_unrestrictedPermissionInferfaceName = "System.Security.Permissions.IUnrestrictedPermission";

        internal PermissionTokenFactory(int size)
        {
            this.m_size = size;
            this.m_index = 0x11;
            this.m_tokenTable = null;
            this.m_handleTable = new Hashtable(size);
            this.m_indexTable = new Hashtable(size);
        }

        internal PermissionToken BuiltInGetToken(int index, IPermission perm, Type cls)
        {
            PermissionToken item = this.m_builtIn[index];
            if (item == null)
            {
                lock (this)
                {
                    item = this.m_builtIn[index];
                    if (item == null)
                    {
                        PermissionTokenType dontKnow = PermissionTokenType.DontKnow;
                        if (perm != null)
                        {
                            dontKnow = PermissionTokenType.IUnrestricted;
                        }
                        else if (cls != null)
                        {
                            dontKnow = PermissionTokenType.IUnrestricted;
                        }
                        item = new PermissionToken(index, dontKnow | PermissionTokenType.BuiltIn, null);
                        this.m_builtIn[index] = item;
                        PermissionToken.s_tokenSet.SetItem(item.m_index, item);
                    }
                }
            }
            if ((item.m_type & PermissionTokenType.DontKnow) != 0)
            {
                item.m_type = PermissionTokenType.BuiltIn;
                if (perm != null)
                {
                    item.m_type |= PermissionTokenType.IUnrestricted;
                    return item;
                }
                if (cls != null)
                {
                    item.m_type |= PermissionTokenType.IUnrestricted;
                    return item;
                }
                item.m_type |= PermissionTokenType.DontKnow;
            }
            return item;
        }

        [SecuritySafeCritical]
        internal PermissionToken FindToken(Type cls)
        {
            IntPtr key = cls.TypeHandle.Value;
            PermissionToken token = (PermissionToken) this.m_handleTable[key];
            if (token == null)
            {
                if (this.m_tokenTable == null)
                {
                    return null;
                }
                token = (PermissionToken) this.m_tokenTable[cls.AssemblyQualifiedName];
                if (token == null)
                {
                    return token;
                }
                lock (this)
                {
                    this.m_handleTable.Add(key, token);
                }
            }
            return token;
        }

        internal PermissionToken FindTokenByIndex(int i)
        {
            if (i < 0x11)
            {
                return this.BuiltInGetToken(i, null, null);
            }
            return (PermissionToken) this.m_indexTable[i];
        }

        internal PermissionToken GetToken(string typeStr)
        {
            object obj2 = null;
            obj2 = (this.m_tokenTable != null) ? this.m_tokenTable[typeStr] : null;
            if (obj2 == null)
            {
                lock (this)
                {
                    if (this.m_tokenTable != null)
                    {
                        obj2 = this.m_tokenTable[typeStr];
                    }
                    else
                    {
                        this.m_tokenTable = new Hashtable(this.m_size, 1f, new PermissionTokenKeyComparer());
                    }
                    if (obj2 == null)
                    {
                        obj2 = new PermissionToken(this.m_index++, PermissionTokenType.DontKnow, typeStr);
                        this.m_tokenTable.Add(typeStr, obj2);
                        this.m_indexTable.Add(this.m_index - 1, obj2);
                        PermissionToken.s_tokenSet.SetItem(((PermissionToken) obj2).m_index, obj2);
                    }
                }
            }
            return (PermissionToken) obj2;
        }

        [SecuritySafeCritical]
        internal PermissionToken GetToken(Type cls, IPermission perm)
        {
            IntPtr key = cls.TypeHandle.Value;
            object obj2 = this.m_handleTable[key];
            if (obj2 == null)
            {
                string assemblyQualifiedName = cls.AssemblyQualifiedName;
                obj2 = (this.m_tokenTable != null) ? this.m_tokenTable[assemblyQualifiedName] : null;
                if (obj2 == null)
                {
                    lock (this)
                    {
                        if (this.m_tokenTable != null)
                        {
                            obj2 = this.m_tokenTable[assemblyQualifiedName];
                        }
                        else
                        {
                            this.m_tokenTable = new Hashtable(this.m_size, 1f, new PermissionTokenKeyComparer());
                        }
                        if (obj2 == null)
                        {
                            if (perm != null)
                            {
                                obj2 = new PermissionToken(this.m_index++, PermissionTokenType.IUnrestricted, assemblyQualifiedName);
                            }
                            else if (cls.GetInterface("System.Security.Permissions.IUnrestrictedPermission") != null)
                            {
                                obj2 = new PermissionToken(this.m_index++, PermissionTokenType.IUnrestricted, assemblyQualifiedName);
                            }
                            else
                            {
                                obj2 = new PermissionToken(this.m_index++, PermissionTokenType.Normal, assemblyQualifiedName);
                            }
                            this.m_tokenTable.Add(assemblyQualifiedName, obj2);
                            this.m_indexTable.Add(this.m_index - 1, obj2);
                            PermissionToken.s_tokenSet.SetItem(((PermissionToken) obj2).m_index, obj2);
                        }
                        if (!this.m_handleTable.Contains(key))
                        {
                            this.m_handleTable.Add(key, obj2);
                        }
                        goto Label_01AC;
                    }
                }
                lock (this)
                {
                    if (!this.m_handleTable.Contains(key))
                    {
                        this.m_handleTable.Add(key, obj2);
                    }
                }
            }
        Label_01AC:
            if ((((PermissionToken) obj2).m_type & PermissionTokenType.DontKnow) != 0)
            {
                if (perm != null)
                {
                    ((PermissionToken) obj2).m_type = PermissionTokenType.IUnrestricted;
                    ((PermissionToken) obj2).m_strTypeName = perm.GetType().AssemblyQualifiedName;
                }
                else
                {
                    if (cls.GetInterface("System.Security.Permissions.IUnrestrictedPermission") != null)
                    {
                        ((PermissionToken) obj2).m_type = PermissionTokenType.IUnrestricted;
                    }
                    else
                    {
                        ((PermissionToken) obj2).m_type = PermissionTokenType.Normal;
                    }
                    ((PermissionToken) obj2).m_strTypeName = cls.AssemblyQualifiedName;
                }
            }
            return (PermissionToken) obj2;
        }
    }
}

