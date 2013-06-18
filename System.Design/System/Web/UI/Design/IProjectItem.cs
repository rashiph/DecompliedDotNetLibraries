namespace System.Web.UI.Design
{
    using System;

    public interface IProjectItem
    {
        string AppRelativeUrl { get; }

        string Name { get; }

        IProjectItem Parent { get; }

        string PhysicalPath { get; }
    }
}

