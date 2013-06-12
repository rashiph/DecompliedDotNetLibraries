namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public interface IReferenceService
    {
        IComponent GetComponent(object reference);
        string GetName(object reference);
        object GetReference(string name);
        object[] GetReferences();
        object[] GetReferences(Type baseType);
    }
}

