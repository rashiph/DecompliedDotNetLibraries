namespace System.Xaml
{
    using System;

    public enum XamlNodeType : byte
    {
        EndMember = 5,
        EndObject = 3,
        GetObject = 2,
        NamespaceDeclaration = 7,
        None = 0,
        StartMember = 4,
        StartObject = 1,
        Value = 6
    }
}

