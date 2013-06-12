namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IDictionaryEnumerator : IEnumerator
    {
        DictionaryEntry Entry { get; }

        object Key { get; }

        object Value { get; }
    }
}

