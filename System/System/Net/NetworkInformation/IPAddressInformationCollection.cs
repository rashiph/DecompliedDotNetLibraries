namespace System.Net.NetworkInformation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class IPAddressInformationCollection : ICollection<IPAddressInformation>, IEnumerable<IPAddressInformation>, IEnumerable
    {
        private Collection<IPAddressInformation> addresses = new Collection<IPAddressInformation>();

        internal IPAddressInformationCollection()
        {
        }

        public virtual void Add(IPAddressInformation address)
        {
            throw new NotSupportedException(SR.GetString("net_collection_readonly"));
        }

        public virtual void Clear()
        {
            throw new NotSupportedException(SR.GetString("net_collection_readonly"));
        }

        public virtual bool Contains(IPAddressInformation address)
        {
            return this.addresses.Contains(address);
        }

        public virtual void CopyTo(IPAddressInformation[] array, int offset)
        {
            this.addresses.CopyTo(array, offset);
        }

        public virtual IEnumerator<IPAddressInformation> GetEnumerator()
        {
            return this.addresses.GetEnumerator();
        }

        internal void InternalAdd(IPAddressInformation address)
        {
            this.addresses.Add(address);
        }

        public virtual bool Remove(IPAddressInformation address)
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

        public virtual IPAddressInformation this[int index]
        {
            get
            {
                return this.addresses[index];
            }
        }
    }
}

