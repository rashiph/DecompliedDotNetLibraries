namespace System.ComponentModel
{
    using System;

    public interface IExtenderProvider
    {
        bool CanExtend(object extendee);
    }
}

