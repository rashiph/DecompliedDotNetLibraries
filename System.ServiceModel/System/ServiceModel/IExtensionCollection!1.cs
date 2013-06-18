namespace System.ServiceModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface IExtensionCollection<T> : ICollection<IExtension<T>>, IEnumerable<IExtension<T>>, IEnumerable where T: IExtensibleObject<T>
    {
        E Find<E>();
        Collection<E> FindAll<E>();
    }
}

