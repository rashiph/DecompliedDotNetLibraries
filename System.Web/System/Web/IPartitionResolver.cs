namespace System.Web
{
    using System;

    public interface IPartitionResolver
    {
        void Initialize();
        string ResolvePartition(object key);
    }
}

