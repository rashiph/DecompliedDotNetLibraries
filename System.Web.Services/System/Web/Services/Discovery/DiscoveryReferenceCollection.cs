namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class DiscoveryReferenceCollection : CollectionBase
    {
        public int Add(DiscoveryReference value)
        {
            return base.List.Add(value);
        }

        public bool Contains(DiscoveryReference value)
        {
            return base.List.Contains(value);
        }

        public void Remove(DiscoveryReference value)
        {
            base.List.Remove(value);
        }

        public DiscoveryReference this[int i]
        {
            get
            {
                return (DiscoveryReference) base.List[i];
            }
            set
            {
                base.List[i] = value;
            }
        }
    }
}

