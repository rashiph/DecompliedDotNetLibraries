namespace System.ComponentModel
{
    using System;
    using System.Collections;

    public interface IBindingListView : IBindingList, IList, ICollection, IEnumerable
    {
        void ApplySort(ListSortDescriptionCollection sorts);
        void RemoveFilter();

        string Filter { get; set; }

        ListSortDescriptionCollection SortDescriptions { get; }

        bool SupportsAdvancedSorting { get; }

        bool SupportsFiltering { get; }
    }
}

