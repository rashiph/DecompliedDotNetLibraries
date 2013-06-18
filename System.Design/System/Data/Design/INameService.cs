namespace System.Data.Design
{
    using System;

    internal interface INameService
    {
        string CreateUniqueName(INamedObjectCollection container, string proposed);
        string CreateUniqueName(INamedObjectCollection container, Type type);
        string CreateUniqueName(INamedObjectCollection container, string proposedNameRoot, int startSuffix);
        void ValidateName(string name);
        void ValidateUniqueName(INamedObjectCollection container, string name);
        void ValidateUniqueName(INamedObjectCollection container, INamedObject namedObject, string proposedName);
    }
}

