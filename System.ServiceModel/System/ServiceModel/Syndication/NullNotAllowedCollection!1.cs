namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    internal class NullNotAllowedCollection<TCollectionItem> : Collection<TCollectionItem> where TCollectionItem: class
    {
        protected override void InsertItem(int index, TCollectionItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, TCollectionItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}

