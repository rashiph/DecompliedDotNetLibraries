namespace System.Linq.Parallel
{
    using System;

    internal interface IPartitionedStreamRecipient<TElement>
    {
        void Receive<TKey>(PartitionedStream<TElement, TKey> partitionedStream);
    }
}

