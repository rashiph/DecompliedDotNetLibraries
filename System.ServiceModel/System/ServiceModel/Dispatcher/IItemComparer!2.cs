namespace System.ServiceModel.Dispatcher
{
    using System;

    internal interface IItemComparer<K, V>
    {
        int Compare(K key, V value);
    }
}

