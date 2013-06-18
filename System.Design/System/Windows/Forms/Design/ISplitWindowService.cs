namespace System.Windows.Forms.Design
{
    using System;
    using System.Windows.Forms;

    internal interface ISplitWindowService
    {
        void AddSplitWindow(Control window);
        void RemoveSplitWindow(Control window);
    }
}

