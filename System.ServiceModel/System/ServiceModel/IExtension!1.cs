namespace System.ServiceModel
{
    using System;

    public interface IExtension<T> where T: IExtensibleObject<T>
    {
        void Attach(T owner);
        void Detach(T owner);
    }
}

