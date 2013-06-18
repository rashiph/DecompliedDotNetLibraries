namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal class TerminatingOperationBehavior
    {
        private static void AbortChannel(object state)
        {
            ((IChannel) state).Abort();
        }

        internal void AfterReply(ref MessageRpc rpc)
        {
            if (rpc.Operation.IsTerminating && rpc.Channel.HasSession)
            {
                new IOThreadTimer(new Action<object>(TerminatingOperationBehavior.AbortChannel), rpc.Channel.Binder.Channel, false).Set(rpc.Channel.CloseTimeout);
            }
        }

        internal static void AfterReply(ref ProxyRpc rpc)
        {
            if (rpc.Operation.IsTerminating && rpc.Channel.HasSession)
            {
                IChannel channel = rpc.Channel.Binder.Channel;
                rpc.Channel.Close(rpc.TimeoutHelper.RemainingTime());
            }
        }

        public static TerminatingOperationBehavior CreateIfNecessary(DispatchRuntime dispatch)
        {
            if (IsTerminatingOperationBehaviorNeeded(dispatch))
            {
                return new TerminatingOperationBehavior();
            }
            return null;
        }

        private static bool IsTerminatingOperationBehaviorNeeded(DispatchRuntime dispatch)
        {
            for (int i = 0; i < dispatch.Operations.Count; i++)
            {
                DispatchOperation operation = dispatch.Operations[i];
                if (operation.IsTerminating)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

