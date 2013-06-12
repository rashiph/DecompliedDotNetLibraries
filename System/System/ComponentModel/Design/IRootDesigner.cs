namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IRootDesigner : IDesigner, IDisposable
    {
        object GetView(ViewTechnology technology);

        ViewTechnology[] SupportedTechnologies { get; }
    }
}

