namespace System.Web.Caching
{
    using System;

    internal interface ICacheDependencyChanged
    {
        void DependencyChanged(object sender, EventArgs e);
    }
}

