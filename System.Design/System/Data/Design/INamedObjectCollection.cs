namespace System.Data.Design
{
    using System.Collections;

    internal interface INamedObjectCollection : ICollection, IEnumerable
    {
        INameService GetNameService();
    }
}

