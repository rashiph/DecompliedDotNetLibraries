namespace System.Windows.Markup
{
    using System;

    public interface IQueryAmbient
    {
        bool IsAmbientPropertyAvailable(string propertyName);
    }
}

