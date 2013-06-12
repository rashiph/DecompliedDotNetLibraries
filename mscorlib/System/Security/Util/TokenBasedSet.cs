namespace System.Security.Util
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    internal class TokenBasedSet
    {
        private int m_cElt;
        private int m_increment;
        private int m_initSize;
        private int m_maxIndex;
        [OptionalField(VersionAdded=2)]
        private object m_Obj;
        private object[] m_objSet;
        [OptionalField(VersionAdded=2)]
        private object[] m_Set;

        internal TokenBasedSet()
        {
            this.m_initSize = 0x18;
            this.m_increment = 8;
            this.Reset();
        }

        internal TokenBasedSet(TokenBasedSet tbSet)
        {
            this.m_initSize = 0x18;
            this.m_increment = 8;
            if (tbSet == null)
            {
                this.Reset();
            }
            else
            {
                if (tbSet.m_cElt > 1)
                {
                    object[] set = tbSet.m_Set;
                    int length = set.Length;
                    object[] destinationArray = new object[length];
                    Array.Copy(set, 0, destinationArray, 0, length);
                    this.m_Set = destinationArray;
                }
                else
                {
                    this.m_Obj = tbSet.m_Obj;
                }
                this.m_cElt = tbSet.m_cElt;
                this.m_maxIndex = tbSet.m_maxIndex;
            }
        }

        internal bool FastIsEmpty()
        {
            return (this.m_cElt == 0);
        }

        internal int GetCount()
        {
            return this.m_cElt;
        }

        internal object GetItem(int index)
        {
            switch (this.m_cElt)
            {
                case 0:
                    return null;

                case 1:
                    if (index != this.m_maxIndex)
                    {
                        return null;
                    }
                    return this.m_Obj;
            }
            if (index < this.m_Set.Length)
            {
                return this.m_Set[index];
            }
            return null;
        }

        internal int GetMaxUsedIndex()
        {
            return this.m_maxIndex;
        }

        internal int GetStartingIndex()
        {
            if (this.m_cElt <= 1)
            {
                return this.m_maxIndex;
            }
            return 0;
        }

        internal bool MoveNext(ref TokenBasedSetEnumerator e)
        {
            switch (this.m_cElt)
            {
                case 0:
                    return false;

                case 1:
                    if (e.Index != -1)
                    {
                        e.Index = (short) (this.m_maxIndex + 1);
                        e.Current = null;
                        return false;
                    }
                    e.Index = this.m_maxIndex;
                    e.Current = this.m_Obj;
                    return true;
            }
            while (++e.Index <= this.m_maxIndex)
            {
                e.Current = this.m_Set[e.Index];
                if (e.Current != null)
                {
                    return true;
                }
            }
            e.Current = null;
            return false;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.OnDeserializedInternal();
        }

        private void OnDeserializedInternal()
        {
            if (this.m_objSet != null)
            {
                if (this.m_cElt == 1)
                {
                    this.m_Obj = this.m_objSet[this.m_maxIndex];
                }
                else
                {
                    this.m_Set = this.m_objSet;
                }
                this.m_objSet = null;
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_objSet = null;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                if (this.m_cElt == 1)
                {
                    this.m_objSet = new object[this.m_maxIndex + 1];
                    this.m_objSet[this.m_maxIndex] = this.m_Obj;
                }
                else if (this.m_cElt > 0)
                {
                    this.m_objSet = this.m_Set;
                }
            }
        }

        internal object RemoveItem(int index)
        {
            object obj2 = null;
            switch (this.m_cElt)
            {
                case 0:
                    return null;

                case 1:
                    if (index == this.m_maxIndex)
                    {
                        obj2 = this.m_Obj;
                        this.Reset();
                        return obj2;
                    }
                    return null;
            }
            if ((index < this.m_Set.Length) && (this.m_Set[index] != null))
            {
                obj2 = this.m_Set[index];
                this.m_Set[index] = null;
                this.m_cElt--;
                if (index == this.m_maxIndex)
                {
                    this.ResetMaxIndex(this.m_Set);
                }
                if (this.m_cElt == 1)
                {
                    this.m_Obj = this.m_Set[this.m_maxIndex];
                    this.m_Set = null;
                }
            }
            return obj2;
        }

        internal void Reset()
        {
            this.m_Obj = null;
            this.m_Set = null;
            this.m_cElt = 0;
            this.m_maxIndex = -1;
        }

        private void ResetMaxIndex(object[] aObj)
        {
            for (int i = aObj.Length - 1; i >= 0; i--)
            {
                if (aObj[i] != null)
                {
                    this.m_maxIndex = (short) i;
                    return;
                }
            }
            this.m_maxIndex = -1;
        }

        internal void SetItem(int index, object item)
        {
            object[] sourceArray = null;
            if (item == null)
            {
                this.RemoveItem(index);
            }
            else
            {
                switch (this.m_cElt)
                {
                    case 0:
                        this.m_cElt = 1;
                        this.m_maxIndex = (short) index;
                        this.m_Obj = item;
                        return;

                    case 1:
                        if (index != this.m_maxIndex)
                        {
                            object obj2 = this.m_Obj;
                            int num = Math.Max(this.m_maxIndex, index);
                            sourceArray = new object[num + 1];
                            sourceArray[this.m_maxIndex] = obj2;
                            sourceArray[index] = item;
                            this.m_maxIndex = (short) num;
                            this.m_cElt = 2;
                            this.m_Set = sourceArray;
                            this.m_Obj = null;
                            return;
                        }
                        this.m_Obj = item;
                        return;
                }
                sourceArray = this.m_Set;
                if (index >= sourceArray.Length)
                {
                    object[] destinationArray = new object[index + 1];
                    Array.Copy(sourceArray, 0, destinationArray, 0, this.m_maxIndex + 1);
                    this.m_maxIndex = (short) index;
                    destinationArray[index] = item;
                    this.m_Set = destinationArray;
                    this.m_cElt++;
                }
                else
                {
                    if (sourceArray[index] == null)
                    {
                        this.m_cElt++;
                    }
                    sourceArray[index] = item;
                    if (index > this.m_maxIndex)
                    {
                        this.m_maxIndex = (short) index;
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal void SpecialSplit(ref TokenBasedSet unrestrictedPermSet, ref TokenBasedSet normalPermSet, bool ignoreTypeLoadFailures)
        {
            int maxUsedIndex = this.GetMaxUsedIndex();
            for (int i = this.GetStartingIndex(); i <= maxUsedIndex; i++)
            {
                object item = this.GetItem(i);
                if (item != null)
                {
                    IPermission perm = item as IPermission;
                    if (perm == null)
                    {
                        perm = PermissionSet.CreatePerm(item, ignoreTypeLoadFailures);
                    }
                    PermissionToken token = PermissionToken.GetToken(perm);
                    if ((perm != null) && (token != null))
                    {
                        if (perm is IUnrestrictedPermission)
                        {
                            if (unrestrictedPermSet == null)
                            {
                                unrestrictedPermSet = new TokenBasedSet();
                            }
                            unrestrictedPermSet.SetItem(token.m_index, perm);
                        }
                        else
                        {
                            if (normalPermSet == null)
                            {
                                normalPermSet = new TokenBasedSet();
                            }
                            normalPermSet.SetItem(token.m_index, perm);
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal TokenBasedSet SpecialUnion(TokenBasedSet other)
        {
            int maxUsedIndex;
            this.OnDeserializedInternal();
            TokenBasedSet set = new TokenBasedSet();
            if (other != null)
            {
                other.OnDeserializedInternal();
                maxUsedIndex = (this.GetMaxUsedIndex() > other.GetMaxUsedIndex()) ? this.GetMaxUsedIndex() : other.GetMaxUsedIndex();
            }
            else
            {
                maxUsedIndex = this.GetMaxUsedIndex();
            }
            for (int i = 0; i <= maxUsedIndex; i++)
            {
                object item = this.GetItem(i);
                IPermission perm = item as IPermission;
                ISecurityElementFactory factory = item as ISecurityElementFactory;
                object obj3 = (other != null) ? other.GetItem(i) : null;
                IPermission permission2 = obj3 as IPermission;
                ISecurityElementFactory factory2 = obj3 as ISecurityElementFactory;
                if ((item != null) || (obj3 != null))
                {
                    if (item == null)
                    {
                        if (factory2 != null)
                        {
                            permission2 = PermissionSet.CreatePerm(factory2, false);
                        }
                        PermissionToken token = PermissionToken.GetToken(permission2);
                        if (token == null)
                        {
                            throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
                        }
                        set.SetItem(token.m_index, permission2);
                    }
                    else if (obj3 == null)
                    {
                        if (factory != null)
                        {
                            perm = PermissionSet.CreatePerm(factory, false);
                        }
                        PermissionToken token2 = PermissionToken.GetToken(perm);
                        if (token2 == null)
                        {
                            throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
                        }
                        set.SetItem(token2.m_index, perm);
                    }
                }
            }
            return set;
        }
    }
}

