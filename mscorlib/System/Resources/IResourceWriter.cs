namespace System.Resources
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IResourceWriter : IDisposable
    {
        void AddResource(string name, object value);
        void AddResource(string name, string value);
        void AddResource(string name, byte[] value);
        void Close();
        void Generate();
    }
}

