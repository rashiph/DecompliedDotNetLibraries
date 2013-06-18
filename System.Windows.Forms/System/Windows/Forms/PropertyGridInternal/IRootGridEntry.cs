namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel;

    public interface IRootGridEntry
    {
        void ResetBrowsableAttributes();
        void ShowCategories(bool showCategories);

        AttributeCollection BrowsableAttributes { get; set; }
    }
}

