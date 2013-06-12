namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;

    public interface IDesignerLoaderService
    {
        void AddLoadDependency();
        void DependentLoadComplete(bool successful, ICollection errorCollection);
        bool Reload();
    }
}

