namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class AdamInstanceCollection : ReadOnlyCollectionBase
    {
        internal AdamInstanceCollection()
        {
        }

        internal AdamInstanceCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(AdamInstance adamInstance)
        {
            if (adamInstance == null)
            {
                throw new ArgumentNullException("adamInstance");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                AdamInstance instance = (AdamInstance) base.InnerList[i];
                if (Utils.Compare(instance.Name, adamInstance.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(AdamInstance[] adamInstances, int index)
        {
            base.InnerList.CopyTo(adamInstances, index);
        }

        public int IndexOf(AdamInstance adamInstance)
        {
            if (adamInstance == null)
            {
                throw new ArgumentNullException("adamInstance");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                AdamInstance instance = (AdamInstance) base.InnerList[i];
                if (Utils.Compare(instance.Name, adamInstance.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public AdamInstance this[int index]
        {
            get
            {
                return (AdamInstance) base.InnerList[index];
            }
        }
    }
}

