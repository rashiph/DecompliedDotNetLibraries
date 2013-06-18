namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ExclusiveHandleList : HybridCollection<ExclusiveHandle>
    {
        internal bool Contains(ExclusiveHandle handle)
        {
            if (base.SingleItem != null)
            {
                if (base.SingleItem.Equals(handle))
                {
                    return true;
                }
            }
            else if (base.MultipleItems != null)
            {
                for (int i = 0; i < base.MultipleItems.Count; i++)
                {
                    if (handle.Equals(base.MultipleItems[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

