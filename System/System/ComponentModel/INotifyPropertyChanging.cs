namespace System.ComponentModel
{
    using System;

    public interface INotifyPropertyChanging
    {
        event PropertyChangingEventHandler PropertyChanging;
    }
}

