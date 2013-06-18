namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal interface IOleDragClient
    {
        bool AddComponent(IComponent component, string name, bool firstAdd);
        Control GetControlForComponent(object component);
        Control GetDesignerControl();
        bool IsDropOk(IComponent component);

        bool CanModifyComponents { get; }

        IComponent Component { get; }
    }
}

