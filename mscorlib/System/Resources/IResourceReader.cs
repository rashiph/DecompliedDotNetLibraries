namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IResourceReader : IEnumerable, IDisposable
    {
        void Close();
        IDictionaryEnumerator GetEnumerator();
    }
}

