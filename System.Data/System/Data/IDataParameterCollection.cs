namespace System.Data
{
    using System;
    using System.Collections;
    using System.Reflection;

    public interface IDataParameterCollection : IList, ICollection, IEnumerable
    {
        bool Contains(string parameterName);
        int IndexOf(string parameterName);
        void RemoveAt(string parameterName);

        object this[string parameterName] { get; set; }
    }
}

