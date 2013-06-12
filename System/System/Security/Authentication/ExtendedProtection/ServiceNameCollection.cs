namespace System.Security.Authentication.ExtendedProtection
{
    using System;
    using System.Collections;

    [Serializable]
    public class ServiceNameCollection : ReadOnlyCollectionBase
    {
        public ServiceNameCollection(ICollection items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            base.InnerList.AddRange(items);
        }

        private void AddIfNew(ArrayList newServiceNames, string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException(SR.GetString("security_ServiceNameCollection_EmptyServiceName"));
            }
            if (!this.Contains(serviceName, newServiceNames))
            {
                newServiceNames.Add(serviceName);
            }
        }

        private bool Contains(string searchServiceName, ICollection serviceNames)
        {
            foreach (string str in serviceNames)
            {
                if (string.Compare(str, searchServiceName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public ServiceNameCollection Merge(IEnumerable serviceNames)
        {
            ArrayList newServiceNames = new ArrayList();
            newServiceNames.AddRange(base.InnerList);
            foreach (object obj2 in serviceNames)
            {
                this.AddIfNew(newServiceNames, obj2 as string);
            }
            return new ServiceNameCollection(newServiceNames);
        }

        public ServiceNameCollection Merge(string serviceName)
        {
            ArrayList newServiceNames = new ArrayList();
            newServiceNames.AddRange(base.InnerList);
            this.AddIfNew(newServiceNames, serviceName);
            return new ServiceNameCollection(newServiceNames);
        }
    }
}

