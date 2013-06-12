namespace System.Web.UI
{
    using System;

    public interface IHierarchyData
    {
        IHierarchicalEnumerable GetChildren();
        IHierarchyData GetParent();

        bool HasChildren { get; }

        object Item { get; }

        string Path { get; }

        string Type { get; }
    }
}

