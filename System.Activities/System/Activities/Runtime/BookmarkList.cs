namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class BookmarkList : HybridCollection<Bookmark>
    {
        internal bool Contains(Bookmark bookmark)
        {
            if (base.SingleItem != null)
            {
                if (base.SingleItem.Equals(bookmark))
                {
                    return true;
                }
            }
            else if (base.MultipleItems != null)
            {
                for (int i = 0; i < base.MultipleItems.Count; i++)
                {
                    if (bookmark.Equals(base.MultipleItems[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void TransferBookmarks(out Bookmark singleItem, out IList<Bookmark> multipleItems)
        {
            singleItem = base.SingleItem;
            multipleItems = base.MultipleItems;
        }
    }
}

