namespace System.ServiceModel
{
    public interface IExtensibleObject<T> where T: IExtensibleObject<T>
    {
        IExtensionCollection<T> Extensions { get; }
    }
}

