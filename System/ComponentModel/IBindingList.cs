namespace System.ComponentModel
{
    using System;
    using System.Collections;

    public interface IBindingList : IList, ICollection, IEnumerable
    {
        event ListChangedEventHandler ListChanged;

        void AddIndex(PropertyDescriptor property);
        object AddNew();
        void ApplySort(PropertyDescriptor property, ListSortDirection direction);
        int Find(PropertyDescriptor property, object key);
        void RemoveIndex(PropertyDescriptor property);
        void RemoveSort();

        bool AllowEdit { get; }

        bool AllowNew { get; }

        bool AllowRemove { get; }

        bool IsSorted { get; }

        ListSortDirection SortDirection { get; }

        PropertyDescriptor SortProperty { get; }

        bool SupportsChangeNotification { get; }

        bool SupportsSearching { get; }

        bool SupportsSorting { get; }
    }
}

