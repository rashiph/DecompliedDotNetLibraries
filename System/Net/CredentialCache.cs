namespace System.Net
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    public class CredentialCache : ICredentials, ICredentialsByHost, IEnumerable
    {
        private Hashtable cache = new Hashtable();
        private Hashtable cacheForHosts = new Hashtable();
        private int m_NumbDefaultCredInCache;
        internal int m_version;

        public void Add(Uri uriPrefix, string authType, NetworkCredential cred)
        {
            if (uriPrefix == null)
            {
                throw new ArgumentNullException("uriPrefix");
            }
            if (authType == null)
            {
                throw new ArgumentNullException("authType");
            }
            if ((((cred is SystemNetworkCredential) && (string.Compare(authType, "NTLM", StringComparison.OrdinalIgnoreCase) != 0)) && (!DigestClient.WDigestAvailable || (string.Compare(authType, "Digest", StringComparison.OrdinalIgnoreCase) != 0))) && ((string.Compare(authType, "Kerberos", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(authType, "Negotiate", StringComparison.OrdinalIgnoreCase) != 0)))
            {
                throw new ArgumentException(SR.GetString("net_nodefaultcreds", new object[] { authType }), "authType");
            }
            this.m_version++;
            CredentialKey key = new CredentialKey(uriPrefix, authType);
            this.cache.Add(key, cred);
            if (cred is SystemNetworkCredential)
            {
                this.m_NumbDefaultCredInCache++;
            }
        }

        public void Add(string host, int port, string authenticationType, NetworkCredential credential)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }
            if (host.Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "host" }));
            }
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if ((((credential is SystemNetworkCredential) && (string.Compare(authenticationType, "NTLM", StringComparison.OrdinalIgnoreCase) != 0)) && (!DigestClient.WDigestAvailable || (string.Compare(authenticationType, "Digest", StringComparison.OrdinalIgnoreCase) != 0))) && ((string.Compare(authenticationType, "Kerberos", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(authenticationType, "Negotiate", StringComparison.OrdinalIgnoreCase) != 0)))
            {
                throw new ArgumentException(SR.GetString("net_nodefaultcreds", new object[] { authenticationType }), "authenticationType");
            }
            this.m_version++;
            CredentialHostKey key = new CredentialHostKey(host, port, authenticationType);
            this.cacheForHosts.Add(key, credential);
            if (credential is SystemNetworkCredential)
            {
                this.m_NumbDefaultCredInCache++;
            }
        }

        public NetworkCredential GetCredential(Uri uriPrefix, string authType)
        {
            if (uriPrefix == null)
            {
                throw new ArgumentNullException("uriPrefix");
            }
            if (authType == null)
            {
                throw new ArgumentNullException("authType");
            }
            int num = -1;
            NetworkCredential credential = null;
            IDictionaryEnumerator enumerator = this.cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CredentialKey key = (CredentialKey) enumerator.Key;
                if (key.Match(uriPrefix, authType))
                {
                    int uriPrefixLength = key.UriPrefixLength;
                    if (uriPrefixLength > num)
                    {
                        num = uriPrefixLength;
                        credential = (NetworkCredential) enumerator.Value;
                    }
                }
            }
            return credential;
        }

        public NetworkCredential GetCredential(string host, int port, string authenticationType)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }
            if (host.Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "host" }));
            }
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException("port");
            }
            NetworkCredential credential = null;
            IDictionaryEnumerator enumerator = this.cacheForHosts.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CredentialHostKey key = (CredentialHostKey) enumerator.Key;
                if (key.Match(host, port, authenticationType))
                {
                    credential = (NetworkCredential) enumerator.Value;
                }
            }
            return credential;
        }

        public IEnumerator GetEnumerator()
        {
            return new CredentialEnumerator(this, this.cache, this.cacheForHosts, this.m_version);
        }

        public void Remove(Uri uriPrefix, string authType)
        {
            if ((uriPrefix != null) && (authType != null))
            {
                this.m_version++;
                CredentialKey key = new CredentialKey(uriPrefix, authType);
                if (this.cache[key] is SystemNetworkCredential)
                {
                    this.m_NumbDefaultCredInCache--;
                }
                this.cache.Remove(key);
            }
        }

        public void Remove(string host, int port, string authenticationType)
        {
            if (((host != null) && (authenticationType != null)) && (port >= 0))
            {
                this.m_version++;
                CredentialHostKey key = new CredentialHostKey(host, port, authenticationType);
                if (this.cacheForHosts[key] is SystemNetworkCredential)
                {
                    this.m_NumbDefaultCredInCache--;
                }
                this.cacheForHosts.Remove(key);
            }
        }

        public static ICredentials DefaultCredentials
        {
            get
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME").Demand();
                return SystemNetworkCredential.defaultCredential;
            }
        }

        public static NetworkCredential DefaultNetworkCredentials
        {
            get
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME").Demand();
                return SystemNetworkCredential.defaultCredential;
            }
        }

        internal bool IsDefaultInCache
        {
            get
            {
                return (this.m_NumbDefaultCredInCache != 0);
            }
        }

        private class CredentialEnumerator : IEnumerator
        {
            private ICredentials[] m_array;
            private CredentialCache m_cache;
            private int m_index = -1;
            private int m_version;

            internal CredentialEnumerator(CredentialCache cache, Hashtable table, Hashtable hostTable, int version)
            {
                this.m_cache = cache;
                this.m_array = new ICredentials[table.Count + hostTable.Count];
                table.Values.CopyTo(this.m_array, 0);
                hostTable.Values.CopyTo(this.m_array, table.Count);
                this.m_version = version;
            }

            bool IEnumerator.MoveNext()
            {
                if (this.m_version != this.m_cache.m_version)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
                }
                if (++this.m_index < this.m_array.Length)
                {
                    return true;
                }
                this.m_index = this.m_array.Length;
                return false;
            }

            void IEnumerator.Reset()
            {
                this.m_index = -1;
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((this.m_index < 0) || (this.m_index >= this.m_array.Length))
                    {
                        throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumOpCantHappen"));
                    }
                    if (this.m_version != this.m_cache.m_version)
                    {
                        throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
                    }
                    return this.m_array[this.m_index];
                }
            }
        }
    }
}

