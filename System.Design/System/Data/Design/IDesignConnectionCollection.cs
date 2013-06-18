namespace System.Data.Design
{
    using System;
    using System.Collections;

    internal interface IDesignConnectionCollection : INamedObjectCollection, ICollection, IEnumerable
    {
        void Clear();
        IDesignConnection Get(string name);
        void Remove(string name);
        void Set(IDesignConnection connection);
    }
}

