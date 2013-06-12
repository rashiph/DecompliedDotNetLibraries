namespace System.ComponentModel
{
    using System;

    public interface ICancelAddNew
    {
        void CancelNew(int itemIndex);
        void EndNew(int itemIndex);
    }
}

