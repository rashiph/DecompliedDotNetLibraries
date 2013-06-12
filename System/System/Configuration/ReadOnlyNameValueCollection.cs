namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    internal class ReadOnlyNameValueCollection : NameValueCollection
    {
        internal ReadOnlyNameValueCollection(IEqualityComparer equalityComparer) : base(equalityComparer)
        {
        }

        internal ReadOnlyNameValueCollection(ReadOnlyNameValueCollection value) : base(value)
        {
        }

        internal void SetReadOnly()
        {
            base.IsReadOnly = true;
        }
    }
}

