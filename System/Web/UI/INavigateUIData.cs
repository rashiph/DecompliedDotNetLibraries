namespace System.Web.UI
{
    using System;

    public interface INavigateUIData
    {
        string Description { get; }

        string Name { get; }

        string NavigateUrl { get; }

        string Value { get; }
    }
}

