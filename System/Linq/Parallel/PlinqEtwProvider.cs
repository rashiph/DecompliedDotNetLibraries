namespace System.Linq.Parallel
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class PlinqEtwProvider : EventProviderBase
    {
        public static PlinqEtwProvider Log = new PlinqEtwProvider();
        internal static int s_queryId = 0;

        private PlinqEtwProvider() : base(new Guid(0x159eeeec, 0x4a14, 0x4418, 0xa8, 0xfe, 250, 0xab, 0xcd, 0x98, 120, 0x87))
        {
        }

        internal static int NextQueryId()
        {
            return Interlocked.Increment(ref s_queryId);
        }

        internal void ParallelQueryBegin(int queryId)
        {
            if (base.IsEnabled())
            {
                int? currentId = Task.CurrentId;
                int num = currentId.HasValue ? currentId.GetValueOrDefault() : 0;
                base.WriteEvent(1, 0, num, queryId);
            }
        }

        internal void ParallelQueryEnd(int queryId)
        {
            if (base.IsEnabled())
            {
                int? currentId = Task.CurrentId;
                int num = currentId.HasValue ? currentId.GetValueOrDefault() : 0;
                base.WriteEvent(2, 0, num, queryId);
            }
        }

        internal void ParallelQueryFork(int queryId)
        {
            if (base.IsEnabled())
            {
                int? currentId = Task.CurrentId;
                int num = currentId.HasValue ? currentId.GetValueOrDefault() : 0;
                if (base.IsEnabled())
                {
                    base.WriteEvent(3, 0, num, queryId);
                }
            }
        }

        internal void ParallelQueryJoin(int queryId)
        {
            if (base.IsEnabled())
            {
                int? currentId = Task.CurrentId;
                int num = currentId.HasValue ? currentId.GetValueOrDefault() : 0;
                base.WriteEvent(4, 0, num, queryId);
            }
        }
    }
}

