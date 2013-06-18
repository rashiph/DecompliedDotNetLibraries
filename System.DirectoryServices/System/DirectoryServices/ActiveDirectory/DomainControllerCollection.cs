namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DomainControllerCollection : ReadOnlyCollectionBase
    {
        internal DomainControllerCollection()
        {
        }

        internal DomainControllerCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(DomainController domainController)
        {
            if (domainController == null)
            {
                throw new ArgumentNullException("domainController");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DomainController controller = (DomainController) base.InnerList[i];
                if (Utils.Compare(controller.Name, domainController.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(DomainController[] domainControllers, int index)
        {
            base.InnerList.CopyTo(domainControllers, index);
        }

        public int IndexOf(DomainController domainController)
        {
            if (domainController == null)
            {
                throw new ArgumentNullException("domainController");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DomainController controller = (DomainController) base.InnerList[i];
                if (Utils.Compare(controller.Name, domainController.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public DomainController this[int index]
        {
            get
            {
                return (DomainController) base.InnerList[index];
            }
        }
    }
}

