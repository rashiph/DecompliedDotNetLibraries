namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ApplicationPartitionCollection : ReadOnlyCollectionBase
    {
        internal ApplicationPartitionCollection()
        {
        }

        internal ApplicationPartitionCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(ApplicationPartition applicationPartition)
        {
            if (applicationPartition == null)
            {
                throw new ArgumentNullException("applicationPartition");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ApplicationPartition partition = (ApplicationPartition) base.InnerList[i];
                if (Utils.Compare(partition.Name, applicationPartition.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ApplicationPartition[] applicationPartitions, int index)
        {
            base.InnerList.CopyTo(applicationPartitions, index);
        }

        public int IndexOf(ApplicationPartition applicationPartition)
        {
            if (applicationPartition == null)
            {
                throw new ArgumentNullException("applicationPartition");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ApplicationPartition partition = (ApplicationPartition) base.InnerList[i];
                if (Utils.Compare(partition.Name, applicationPartition.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ApplicationPartition this[int index]
        {
            get
            {
                return (ApplicationPartition) base.InnerList[index];
            }
        }
    }
}

