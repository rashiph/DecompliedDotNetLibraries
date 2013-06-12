namespace System.ComponentModel
{
    using System;

    public interface IEditableObject
    {
        void BeginEdit();
        void CancelEdit();
        void EndEdit();
    }
}

