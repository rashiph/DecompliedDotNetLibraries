namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ForestTrustDomainInfoCollection : ReadOnlyCollectionBase
    {
        internal ForestTrustDomainInfoCollection()
        {
        }

        internal int Add(ForestTrustDomainInformation info)
        {
            return base.InnerList.Add(info);
        }

        public bool Contains(ForestTrustDomainInformation information)
        {
            if (information == null)
            {
                throw new ArgumentNullException("information");
            }
            return base.InnerList.Contains(information);
        }

        public void CopyTo(ForestTrustDomainInformation[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(ForestTrustDomainInformation information)
        {
            if (information == null)
            {
                throw new ArgumentNullException("information");
            }
            return base.InnerList.IndexOf(information);
        }

        public ForestTrustDomainInformation this[int index]
        {
            get
            {
                return (ForestTrustDomainInformation) base.InnerList[index];
            }
        }
    }
}

