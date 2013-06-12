namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;

    public interface IDesignerLoaderHost : IDesignerHost, IServiceContainer, IServiceProvider
    {
        void EndLoad(string baseClassName, bool successful, ICollection errorCollection);
        void Reload();
    }
}

