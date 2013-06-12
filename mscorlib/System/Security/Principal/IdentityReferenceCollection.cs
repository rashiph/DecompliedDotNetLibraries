namespace System.Security.Principal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(false)]
    public class IdentityReferenceCollection : ICollection<IdentityReference>, IEnumerable<IdentityReference>, IEnumerable
    {
        private List<IdentityReference> _Identities;

        public IdentityReferenceCollection() : this(0)
        {
        }

        public IdentityReferenceCollection(int capacity)
        {
            this._Identities = new List<IdentityReference>(capacity);
        }

        public void Add(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this._Identities.Add(identity);
        }

        public void Clear()
        {
            this._Identities.Clear();
        }

        public bool Contains(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            return this._Identities.Contains(identity);
        }

        public void CopyTo(IdentityReference[] array, int offset)
        {
            this._Identities.CopyTo(0, array, offset, this.Count);
        }

        public IEnumerator<IdentityReference> GetEnumerator()
        {
            return new IdentityReferenceEnumerator(this);
        }

        public bool Remove(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (this.Contains(identity))
            {
                this._Identities.Remove(identity);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IdentityReferenceCollection Translate(Type targetType)
        {
            return this.Translate(targetType, false);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
        public IdentityReferenceCollection Translate(Type targetType, bool forceSuccess)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (!targetType.IsSubclassOf(typeof(IdentityReference)))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
            }
            if (this.Identities.Count == 0)
            {
                return new IdentityReferenceCollection();
            }
            int capacity = 0;
            int num2 = 0;
            for (int i = 0; i < this.Identities.Count; i++)
            {
                Type type = this.Identities[i].GetType();
                if (type != targetType)
                {
                    if (type != typeof(SecurityIdentifier))
                    {
                        if (type != typeof(NTAccount))
                        {
                            throw new SystemException();
                        }
                        num2++;
                    }
                    else
                    {
                        capacity++;
                    }
                }
            }
            bool flag = false;
            IdentityReferenceCollection sourceSids = null;
            IdentityReferenceCollection sourceAccounts = null;
            if (capacity == this.Count)
            {
                flag = true;
                sourceSids = this;
            }
            else if (capacity > 0)
            {
                sourceSids = new IdentityReferenceCollection(capacity);
            }
            if (num2 == this.Count)
            {
                flag = true;
                sourceAccounts = this;
            }
            else if (num2 > 0)
            {
                sourceAccounts = new IdentityReferenceCollection(num2);
            }
            IdentityReferenceCollection unmappedIdentities = null;
            if (!flag)
            {
                unmappedIdentities = new IdentityReferenceCollection(this.Identities.Count);
                for (int j = 0; j < this.Identities.Count; j++)
                {
                    IdentityReference identity = this[j];
                    Type type2 = identity.GetType();
                    if (type2 != targetType)
                    {
                        if (type2 != typeof(SecurityIdentifier))
                        {
                            if (type2 != typeof(NTAccount))
                            {
                                throw new SystemException();
                            }
                            sourceAccounts.Add(identity);
                        }
                        else
                        {
                            sourceSids.Add(identity);
                        }
                    }
                }
            }
            bool someFailed = false;
            IdentityReferenceCollection references4 = null;
            IdentityReferenceCollection references5 = null;
            if (capacity > 0)
            {
                references4 = SecurityIdentifier.Translate(sourceSids, targetType, out someFailed);
                if (flag && (!forceSuccess || !someFailed))
                {
                    unmappedIdentities = references4;
                }
            }
            if (num2 > 0)
            {
                references5 = NTAccount.Translate(sourceAccounts, targetType, out someFailed);
                if (flag && (!forceSuccess || !someFailed))
                {
                    unmappedIdentities = references5;
                }
            }
            if (forceSuccess && someFailed)
            {
                unmappedIdentities = new IdentityReferenceCollection();
                if (references4 != null)
                {
                    foreach (IdentityReference reference2 in references4)
                    {
                        if (reference2.GetType() != targetType)
                        {
                            unmappedIdentities.Add(reference2);
                        }
                    }
                }
                if (references5 != null)
                {
                    foreach (IdentityReference reference3 in references5)
                    {
                        if (reference3.GetType() != targetType)
                        {
                            unmappedIdentities.Add(reference3);
                        }
                    }
                }
                throw new IdentityNotMappedException(Environment.GetResourceString("IdentityReference_IdentityNotMapped"), unmappedIdentities);
            }
            if (!flag)
            {
                capacity = 0;
                num2 = 0;
                unmappedIdentities = new IdentityReferenceCollection(this.Identities.Count);
                for (int k = 0; k < this.Identities.Count; k++)
                {
                    IdentityReference reference4 = this[k];
                    Type type3 = reference4.GetType();
                    if (type3 == targetType)
                    {
                        unmappedIdentities.Add(reference4);
                    }
                    else if (type3 == typeof(SecurityIdentifier))
                    {
                        unmappedIdentities.Add(references4[capacity++]);
                    }
                    else
                    {
                        if (type3 != typeof(NTAccount))
                        {
                            throw new SystemException();
                        }
                        unmappedIdentities.Add(references5[num2++]);
                    }
                }
            }
            return unmappedIdentities;
        }

        public int Count
        {
            get
            {
                return this._Identities.Count;
            }
        }

        internal List<IdentityReference> Identities
        {
            get
            {
                return this._Identities;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public IdentityReference this[int index]
        {
            get
            {
                return this._Identities[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._Identities[index] = value;
            }
        }
    }
}

