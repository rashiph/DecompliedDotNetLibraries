namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;

    public interface IComponentInitializer
    {
        void InitializeExistingComponent(IDictionary defaultValues);
        void InitializeNewComponent(IDictionary defaultValues);
    }
}

