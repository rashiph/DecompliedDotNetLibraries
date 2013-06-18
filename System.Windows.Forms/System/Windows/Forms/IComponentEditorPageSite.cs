namespace System.Windows.Forms
{
    using System;

    public interface IComponentEditorPageSite
    {
        Control GetControl();
        void SetDirty();
    }
}

