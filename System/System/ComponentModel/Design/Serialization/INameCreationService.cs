namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.ComponentModel;

    public interface INameCreationService
    {
        string CreateName(IContainer container, Type dataType);
        bool IsValidName(string name);
        void ValidateName(string name);
    }
}

