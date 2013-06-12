namespace System.Net.NetworkInformation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class UnicastIPAddressInformationCollection : ICollection<UnicastIPAddressInformation>, IEnumerable<UnicastIPAddressInformation>, IEnumerable
    {
        private Collection<UnicastIPAddressInformation> addresses = new Collection<UnicastIPAddressInformation>();

        protected internal UnicastIPAddressInformationCollection()
        {
        }

        public virtual void Add(UnicastIPAddressInformation address)
        {
            throw new NotSupportedException(SR.GetString("net_collection_readonly"));
        }

        public virtual void Clear()
        {
            throw new NotSupportedException(SR.GetString("net_collection_readonly"));
        }

        public virtual bool Contains(UnicastIPAddressInformation address)
        {
            return this.addresses.Contains(address);
        }

        public virtual void CopyTo(UnicastIPAddressInformation[] array, int offset)
        {
            this.addresses.CopyTo(array, offset);
        }

        public virtual IEnumerator<UnicastIPAddressInformation> GetEnumerator()
        {
            return this.addresses.GetEnumerator();
        }

        internal void InternalAdd(UnicastIPAddressInformation address)
        {
            this.addresses.Add(address);
        }

        public virtual bool Remove(UnicastIPAddressInformation address)
        {
            throw new NotSupportedException(SR.GetString("net_collection_readonly"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public virtual int Count
        {
            get
            {
                return this.addresses.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public virtual UnicastIPAddressInformation this[int index]
        {
            get
            {
                return this.addresses[index];
            }
        }
    }
}

