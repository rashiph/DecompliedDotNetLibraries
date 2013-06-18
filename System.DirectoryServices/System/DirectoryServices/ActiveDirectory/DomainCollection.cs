namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DomainCollection : ReadOnlyCollectionBase
    {
        internal DomainCollection()
        {
        }

        internal DomainCollection(ArrayList values)
        {
            if (values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    this.Add((Domain) values[i]);
                }
            }
        }

        internal int Add(Domain domain)
        {
            return base.InnerList.Add(domain);
        }

        internal void Clear()
        {
            base.InnerList.Clear();
        }

        public bool Contains(Domain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                Domain domain2 = (Domain) base.InnerList[i];
                if (Utils.Compare(domain2.Name, domain.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(Domain[] domains, int index)
        {
            base.InnerList.CopyTo(domains, index);
        }

        public int IndexOf(Domain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                Domain domain2 = (Domain) base.InnerList[i];
                if (Utils.Compare(domain2.Name, domain.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public Domain this[int index]
        {
            get
            {
                return (Domain) base.InnerList[index];
            }
        }
    }
}

