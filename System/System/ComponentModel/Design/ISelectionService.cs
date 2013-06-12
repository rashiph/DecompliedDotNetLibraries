namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISelectionService
    {
        event EventHandler SelectionChanged;

        event EventHandler SelectionChanging;

        bool GetComponentSelected(object component);
        ICollection GetSelectedComponents();
        void SetSelectedComponents(ICollection components);
        void SetSelectedComponents(ICollection components, SelectionTypes selectionType);

        object PrimarySelection { get; }

        int SelectionCount { get; }
    }
}

